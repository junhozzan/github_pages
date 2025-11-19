using System;
using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    private Action<ObjectBase> onRelease = null;

    public virtual void Initialize() 
    {

    }

    public void BindOnRelease(Action<ObjectBase> onRelease)
    {
        this.onRelease = onRelease;
    }

    public void SetActive(bool active)
    {
        if (gameObject.activeSelf && !active)
        {
            OnRelease();
        }
     
        gameObject.SetActive(active);
    }

    protected virtual void OnRelease()
    {
        onRelease?.Invoke(this);
    }
}
