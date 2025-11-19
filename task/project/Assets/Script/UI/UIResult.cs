using System;
using UnityEngine;
using UnityEngine.UI;

public class UIResult : UIMonoBehaviour
{
    [SerializeField] Text resultText = null;

    private Action onGoToLobby = null;

    public UIResult On()
    {
        if (!UIManager.Instance.Show(this))
        {
            return this;
        }

        return this;
    }

    public override void Unbind()
    {
        base.Unbind();
        onGoToLobby = null;
    }

    public void SetResultText(string s)
    {
        resultText.text = s;
    }

    public void BindOnGoToLobby(Action on)
    {
        this.onGoToLobby = on;
    }

    /// <summary>
    /// 버튼 이벤트
    /// </summary>
    public void OnGoToLobby()
    {
        onGoToLobby?.Invoke();
        Off();
    }

    public override UIManager.CanvasType GetCanvas()
    {
        return UIManager.CanvasType.POPUP;
    }
}
