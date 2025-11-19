public class UIMain : UIMonoBehaviour
{
    public UIMain On()
    {
        UIManager.Instance.Show(this);
        return this;
    }

    public override UIManager.CanvasType GetCanvas()
    {
        return UIManager.CanvasType.LAST;
    }

    public override bool IsFixed()
    {
        return base.IsFixed();
    }
}
