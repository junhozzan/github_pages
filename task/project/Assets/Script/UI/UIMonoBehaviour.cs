using System;
using UnityEngine;

public abstract class UIMonoBehaviour : MonoBehaviour
{
    private Action onEsc = null;

    public virtual void Initialize()
    {

    }

    public virtual void Show(bool show)
    {
        gameObject.SetActive(show);
    }

    public virtual void Unbind()
    {
        onEsc = null;
    }

    public void BindOnEsc(Action on)
    {
        this.onEsc = on;
    }

    public void Off()
    {
        UIManager.Instance.CloseAt(this);
    }

    public virtual bool CanEsc()
    {
        return true;
    }

    public void OnEsc()
    {
        onEsc?.Invoke();
    }

    public virtual bool IsFixed()
    {
        return false;
    }

    public abstract UIManager.CanvasType GetCanvas();
}