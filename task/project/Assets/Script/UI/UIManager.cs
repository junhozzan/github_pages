using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IUIUpdate
{
    void UpdateDt(float dt, DateTime now);
}

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField] Canvas defaultCanvas = null;

    private readonly Dictionary<Type, UIMonoBehaviour> cachedUIs = new();
    private readonly Dictionary<CanvasType, Canvas> canvasByType = new();
    private readonly Dictionary<CanvasType, List<UIMonoBehaviour>> openUIsByType = new();
    private readonly List<IUIUpdate> updateUIs = new();

    private UIMonoBehaviour _current
    {
        get
        {
            // 화면에 가까운 캔버스 부터 검색하여 가장 앞쪽의 UI를 검색
            foreach (var openUIs in openUIsByType.Values)
            {
                var count = openUIs.Count;
                if (count == 0)
                {
                    continue;
                }

                for (int i = count - 1; i >= 0; --i)
                {
                    var ui = openUIs[i];
                    
                    // 고정형 UI는 예외처리.
                    if (ui.IsFixed())
                    {
                        continue;
                    }

                    return ui;
                }
            }

            return null;
        }
    }

    protected override void Initialize()
    {
        var canvasTypes = Enum.GetValues(typeof(CanvasType)).Cast<CanvasType>();
        foreach (var type in canvasTypes)
        {
            // 캔버스 오브젝트 복사 
            var canvas = GameObject.Instantiate(defaultCanvas, defaultCanvas.transform.parent);
            canvas.name = type.ToString();
            canvas.sortingOrder = (int)type;
            canvas.gameObject.SetActive(true);
            canvas.transform.SetAsLastSibling();

            canvasByType.Add(type, canvas);
            openUIsByType.Add(type, new List<UIMonoBehaviour>());
        }

        defaultCanvas.gameObject.SetActive(false);
    }

    public void Clear()
    {
        foreach (var openUIs in openUIsByType.Values)
        {
            foreach (var openUI in openUIs)
            {
                openUI.Show(false);
            }

            openUIs.Clear();
        }
    }

    private void Update()
    {
        var dt = Time.deltaTime;
        var now = DateTime.Now;
        for (int i = 0, cnt = updateUIs.Count; i < cnt; ++i)
        {
            updateUIs[i]?.UpdateDt(dt, now);
        }

        // 뒤로가기 키 입력
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEsc();
        }
    }

    public void AddUpdateUI(UIMonoBehaviour mono)
    {
        var uiUpdate = mono as IUIUpdate;
        if (uiUpdate == null || updateUIs.Contains(uiUpdate))
        {
            return;
        }

        updateUIs.Add(uiUpdate);
    }

    public void RemoveUpdateUI(UIMonoBehaviour mono)
    {
        var uiUpdate = mono as IUIUpdate;
        if (uiUpdate == null || !updateUIs.Contains(uiUpdate))
        {
            return;
        }

        updateUIs.Remove(uiUpdate);
    }

    /// <summary>
    /// UI를 열 경우 이 함수를 통해 연다
    /// </summary>
    public bool Show(UIMonoBehaviour open)
    {
        if (!openUIsByType.TryGetValue(open.GetCanvas(), out var list))
        {
            return false;
        }

        if (list.Contains(open))
        {
            return false;
        }

        // 유아이 열기
        open.Show(true);
        list.Add(open);

        AddUpdateUI(open);

        return true;
    }

    /// <summary>
    /// back key
    /// </summary>
    public void OnEsc()
    {
        var current = _current;
        if (current == null)
        {
            return;
        }

        if (!current.CanEsc())
        {
            return;
        }

        current.OnEsc();
        CloseAt(current);
    }

    public void CloseAt(UIMonoBehaviour ui)
    {
        if (ui == null)
        {
            return;
        }

        ui.Show(false);
        ui.Unbind();
        if (openUIsByType.TryGetValue(ui.GetCanvas(), out var list))
        {
            list.Remove(ui);
        }

        RemoveUpdateUI(ui);
    }

    public T GetUI<T>(string name = null) where T : UIMonoBehaviour
    {
        var type = typeof(T);
        if (!cachedUIs.TryGetValue(type, out var v))
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            cachedUIs.Add(type, v = Load(name));
        }
        
        return v as T;
    }

    private UIMonoBehaviour Load(string name)
    {
        UIMonoBehaviour uiMono = null;

        if (!string.IsNullOrEmpty(name))
        {
            var prefab = FakeAddressableManager.Instance.Load(name);
            if (prefab != null)
            {
                uiMono = GameObject.Instantiate(prefab, defaultCanvas.transform).GetComponent<UIMonoBehaviour>();
            }
        }

        if (uiMono == null)
        {
            Debug.Log($"## ui is null : {name}");
            return null;
        }

        uiMono.gameObject.SetActive(false);
        uiMono.Initialize();

        LocateCanvas(uiMono);
        return uiMono;
    }

    private void LocateCanvas(UIMonoBehaviour mono)
    {
        if (canvasByType.TryGetValue(mono.GetCanvas(), out var canvas))
        {
            mono.transform.SetParent(canvas.transform);
        }

        SetScreenSize(mono.transform as RectTransform);
    }

    public static void SetScreenSize(RectTransform rt)
    {
        if (rt == null)
        {
            return;
        }

        rt.localScale = Vector3.one;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public enum CanvasType
    {
        INGAME = 0,
        CONTENTS = 100,
        POPUP = 200,
        LAST = 300,
    }
}
