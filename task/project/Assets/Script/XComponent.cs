using System;
using System.Collections.Generic;
using System.Linq;

public interface IXUpdate
{
    void XUpdate(float dt);
}

public class XComponent
{
    // 최상위 부모 객체 정보
    private readonly XTopComponent xtop = null;

    // 자식 컴포넌트
    private readonly List<XComponent> children = new();

    public XComponent(XComponent parent)
    {
        xtop = parent == null ? new XTopComponent(this) : parent.xtop;
        xtop.AddUpdate(this);
    }

    public virtual void Initialize()
    {
        for (int i = 0; i < children.Count; ++i)
        {
            children[i].Initialize();
        }
    }

    public virtual void OnEnable()
    {
        for (int i = 0; i < children.Count; ++i)
        {
            children[i].OnEnable();
        }
    }

    public virtual void OnDisable()
    {
        for (int i = 0; i < children.Count; ++i)
        {
            children[i].OnDisable();
        }
    }

    public void XUpdateDt(float dt)
    {
        // Update 최적화 작업
        xtop.UpdateDt(dt);
    }

    public T AddComponent<T>(object arg = null) where T : XComponent
    {
        var type = typeof(T);
        var com = (arg == null ? Activator.CreateInstance(type, this) : Activator.CreateInstance(type, this, arg)) as XComponent;

        AddChildren(com);
        return com as T;
    }

    private void AddChildren(XComponent com)
    {
        if (!children.Contains(com))
        {
            children.Add(com);
        }
    }

    public T GetComponent<T>() where T : XComponent
    {
        return xtop.GetComponent<T>();
    }

    private T GetComponentInChildren<T>() where T : XComponent
    {
        for (int i = 0; i < children.Count; ++i)
        {
            var child = children[i];
            if (child is T match)
            {
                return match;
            }

            var nested = child.GetComponentInChildren<T>();
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    public bool TryGetComponent<T>(out T t) where T : XComponent
    {
        t = GetComponent<T>();
        return t != null;
    }

    public List<T> GetChildren<T>() where T : XComponent
    {
        return children.Select(x => x as T).ToList();
    }

    private class XTopComponent
    {
        private readonly Dictionary<Type, XComponent> cached = new();
        private readonly XComponent component = null;
        private Action<float> onUpdateDt = null;

        public XTopComponent(XComponent component)
        {
            this.component = component;
        }

        public T GetComponent<T>() where T : XComponent
        {
            var type = typeof(T);
            if (cached.TryGetValue(type, out var v))
            {
                return v as T;
            }

            var com = component.GetComponentInChildren<T>();
            cached.Add(type, com);
            return com;
        }

        public void AddUpdate(XComponent com)
        {
            if (com is IXUpdate i)
            {
                onUpdateDt += i.XUpdate;
            }
        }

        public void UpdateDt(float dt)
        {
            onUpdateDt?.Invoke(dt);
        }
    }
}
