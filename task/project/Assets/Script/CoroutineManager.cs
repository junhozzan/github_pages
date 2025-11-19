using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight, single-file Coroutine Manager for Unity.
/// Drop this script into a GameObject (or let it auto-create at runtime) and use
/// CoroutineManager.Instance.StartRoutine(...) from anywhere.
///
/// Features:
/// - Singleton MonoBehaviour that persists between scenes (optional)
/// - Start/Stop coroutines with numeric handles
/// - Start a delayed one-shot or repeating callback
/// - Pause/Resume by handle
/// - Safe cleanup on destroy
///
/// Usage examples:
/// // Start an IEnumerator routine and get the handle
/// int id = CoroutineManager.Instance.StartRoutine(MyCoroutine());
/// // Stop it
/// CoroutineManager.Instance.StopRoutine(id);
/// // Start a delayed action
/// int delayed = CoroutineManager.Instance.StartTimer(2f, () => Debug.Log("Hello after 2s"));
/// // Start a repeating action (repeatCount = -1 for infinite)
/// int rep = CoroutineManager.Instance.StartRepeating(1f, () => Debug.Log("tick"), -1);
/// // Pause / Resume
/// CoroutineManager.Instance.PauseRoutine(rep);
/// CoroutineManager.Instance.ResumeRoutine(rep);
///
/// NOTE: This manager must be used from Unity's main thread. Calls from background threads
/// should be marshalled to main thread before calling these methods.
/// </summary>
public class CoroutineManager : MonoSingleton<CoroutineManager>
{
    private void OnDestroy()
    {
        // stop and clear bookkeeping
        _routines.Clear();
    }

    // --- Routine bookkeeping ---
    private int _nextId = 1;

    private class Routine
    {
        public int id;
        public IEnumerator enumerator; // wrapped enumerator
        public Coroutine unityCoroutine; // the actual Unity Coroutine
        public bool paused;
        public bool running;
    }

    private readonly Dictionary<int, Routine> _routines = new Dictionary<int, Routine>();
    private readonly Dictionary<float, WaitForSeconds> cachedWaitForSeconds = new Dictionary<float, WaitForSeconds>();

    /// <summary>
    /// Start an IEnumerator as a managed routine. Returns an integer handle you can use to stop/pause/resume.
    /// </summary>
    public int StartRoutine(IEnumerator enumerator)
    {
        if (enumerator == null) throw new ArgumentNullException(nameof(enumerator));

        int id = _nextId++;
        var routine = new Routine
        {
            id = id,
            enumerator = enumerator,
            paused = false,
            running = true
        };
        routine.unityCoroutine = StartCoroutine(RunRoutine(routine));
        _routines[id] = routine;
        return id;
    }

    /// <summary>
    /// Stop a routine by its id. Returns true if stopped.
    /// </summary>
    public bool StopRoutine(int id)
    {
        if (_routines.TryGetValue(id, out var r))
        {
            if (r.unityCoroutine != null)
            {
                StopCoroutine(r.unityCoroutine);
            }
            r.running = false;
            _routines.Remove(id);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Pause a routine (it will yield each frame but not progress until resumed).
    /// </summary>
    public bool PauseRoutine(int id)
    {
        if (_routines.TryGetValue(id, out var r) && r.running)
        {
            r.paused = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Resume a paused routine.
    /// </summary>
    public bool ResumeRoutine(int id)
    {
        if (_routines.TryGetValue(id, out var r) && r.running)
        {
            r.paused = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns whether a routine is currently running (and tracked).
    /// </summary>
    public bool IsRunning(int id)
    {
        return _routines.ContainsKey(id);
    }

    public bool IsRunning(List<int> ids)
    {
        foreach (var id in ids)
        {
            if (IsRunning(id))
            {
                // 하나라도 진행중이면 코루틴 유지
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Stop all managed routines.
    /// </summary>
    public void StopAllManaged()
    {
        foreach (var r in _routines.Values)
        {
            if (r.unityCoroutine != null)
                StopCoroutine(r.unityCoroutine);
        }
        _routines.Clear();
    }

    private IEnumerator RunRoutine(Routine routine)
    {
        var enumerator = routine.enumerator;
        while (true)
        {
            if (!routine.running)
                yield break;

            // handle pause: while paused, wait a frame
            if (routine.paused)
            {
                yield return null;
                continue;
            }

            bool moved;
            try
            {
                moved = enumerator.MoveNext();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                break;
            }

            if (!moved)
                break;

            // yield whatever the inner enumerator requested
            var current = enumerator.Current;
            yield return current;
        }

        // 딜레이 없이 루틴이 종료되면 Remove가 먼저 불림
        yield return null;
        // cleanup when finished
        routine.running = false;
        _routines.Remove(routine.id);
    }

    // --- Convenience helpers ---

    /// <summary>
    /// Start a timer that calls action after delaySeconds. Returns the handle.
    /// </summary>
    public int StartTimer(float delaySeconds, Action action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        return StartRoutine(TimerCoroutine(delaySeconds, action));
    }

    private IEnumerator TimerCoroutine(float delay, Action action)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        else yield return null; // next frame
        action();
    }

    /// <summary>
    /// Start a repeating action. repeatCount: -1 for infinite, otherwise number of times to run.
    /// Returns the handle (use StopRoutine to cancel early).
    /// </summary>
    public int StartRepeating(float intervalSeconds, Action action, int repeatCount = -1)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (intervalSeconds < 0f) intervalSeconds = 0f;
        return StartRoutine(RepeatingCoroutine(intervalSeconds, action, repeatCount));
    }

    private IEnumerator RepeatingCoroutine(float interval, Action action, int repeatCount)
    {
        int ran = 0;
        while (repeatCount < 0 || ran < repeatCount)
        {
            if (interval > 0f)
                yield return new WaitForSeconds(interval);
            else
                yield return null;

            action();
            ran++;
        }
    }

    /// <summary>
    /// Start a routine from an async-compatible function returning IEnumerator.
    /// This is a convenience when you want to supply a lambda.
    /// </summary>
    public int StartRoutine(Func<IEnumerator> enumeratorFunc)
    {
        if (enumeratorFunc == null) throw new ArgumentNullException(nameof(enumeratorFunc));
        return StartRoutine(enumeratorFunc());
    }

    public WaitForSeconds GetWaitForSeconds(float time)
    {
        if (!cachedWaitForSeconds.TryGetValue(time, out var v))
        {
            cachedWaitForSeconds.Add(time, v = new WaitForSeconds(time));
        }
        return v;
    }
}
