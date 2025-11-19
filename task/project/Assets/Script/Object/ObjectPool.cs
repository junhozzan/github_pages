using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPool<T> where T : ObjectBase
{
    private readonly Stack<T> stack = new();
    private readonly List<T> createList = new();
    private readonly T origin = null;
    private readonly GameObject parent = null;
    private readonly Action<T> onCreateInit = null;

    public static ObjectPool<T> Of(T origin, GameObject parent, Action<T> onCreateInit = null)
    {
        return new ObjectPool<T>(origin, parent, onCreateInit);
    }

    private ObjectPool(T origin, GameObject parent, Action<T> onCreateInit)
    {
        this.origin = origin;
        this.parent = parent;
        this.onCreateInit = onCreateInit;
    }

    public T Create()
    {
        var go = GameObject.Instantiate(origin.gameObject, Vector3.zero, Quaternion.identity, parent.transform);

#if UNITY_EDITOR
        go.name = origin.name;
#endif

        if (!go.TryGetComponent(out T obj))
        {
            Debug.Log($"## pool object type is null !! : {origin.name}");
            return null;
        }

        obj.Initialize();
        obj.BindOnRelease(Release);

        createList.Add(obj);
        onCreateInit?.Invoke(obj);

        return obj;
    }

    public T Pop(bool visible = true)
    {
        var obj = stack.Count > 0 ? stack.Pop() : Create();
        obj.SetActive(visible);

        return obj;
    }

    public void Clear()
    {
        if (createList == null || createList.Count == 0)
        {
            return;
        }

        // 모든 객체 제거 (Invisible)
        foreach (var obj in createList)
        {
            obj.SetActive(false);
        }
    }

    private void Release(ObjectBase obj)
    {
        var t = obj as T;
        if (stack.Contains(t))
        {
            return;
        }

        stack.Push(t);
        t.transform.SetParent(parent.transform);
    }

#if USE_COUNT_DEBUG
    private const bool _DEBUG = true;
#else
    private const bool _DEBUG = false;
#endif
}