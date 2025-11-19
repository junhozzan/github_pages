using System.Collections.Generic;
using UnityEngine;

public interface IOUpdate
{
    bool CanUpdate();
    void UpdateDt(float dt);
}

public class ObjectManager : MonoSingleton<ObjectManager>
{
    private readonly Dictionary<string, ObjectPool<ObjectBase>> dicPool = new();
    private readonly List<IOUpdate> updateObjects = new();

    [SerializeField] GameObject world = null;

    public void Clear()
    {
        foreach (var pool in dicPool.Values)
        {
            if (pool == null)
            {
                continue;
            }

            pool.Clear();
        }
    }

    public void UpdateDt(float dt)
    {
        for (int i = 0; i < updateObjects.Count; ++i)
        {
            var o = updateObjects[i];
            if (!o.CanUpdate())
            {
                continue;
            }

            o.UpdateDt(dt);
        }
    }

    public T Pop<T>(string name, bool active = true) where T : ObjectBase
    {
        if (!dicPool.TryGetValue(name, out ObjectPool<ObjectBase> pool))
        {
            var prefab = FakeAddressableManager.Instance.Load(name);
            if (prefab == null)
            {
                return null;
            }

            if (prefab.TryGetComponent(out ObjectBase obj))
            {
                pool = ObjectPool<ObjectBase>.Of(obj, world, OnCreateInit);
                dicPool.Add(name, pool);
            }
        }

        var pop = pool.Pop(active);
        if (pop == null)
        {
            return null;
        }

        return pop as T;
    }

    private void OnCreateInit(ObjectBase obj)
    {
        AddUpdteObject(obj);
    }

    private void AddUpdteObject(ObjectBase o)
    {
        var oupdate = o as IOUpdate; 
        if (oupdate == null || updateObjects.Contains(oupdate))
        {
            return;
        }

        updateObjects.Add(oupdate);
    }
}
