using System;

public class UILobby : UIMonoBehaviour
{
    private Action onPlay = null;

    public UILobby On()
    {
        UIManager.Instance.Show(this);
        return this;
    }

    public override void Unbind()
    {
        base.Unbind();
        onPlay = null;
    }

    public void BindOnPlay(Action onPlay)
    {
        this.onPlay = onPlay;
    }

    /// <summary>
    /// 버튼 이벤트
    /// </summary>
    public void OnPlay()
    {
        onPlay?.Invoke();
    }

    public override bool CanEsc()
    {
        return false;
    }

    public override UIManager.CanvasType GetCanvas()
    {
        return UIManager.CanvasType.CONTENTS;
    }
}
