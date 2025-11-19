using UnityEngine;
using UnityEngine.UI;

public class UIPlayBoss : UIMonoBehaviour
{
    [SerializeField] RectTransform hpGauge = null;
    [SerializeField] RectTransform hpGaugeFill = null;
    [SerializeField] Text limitText = null;

    public UIPlayBoss On()
    {
        UIManager.Instance.Show(this);
        return this;
    }

    public void SetHpGauge(float rate)
    {
        var fillRect = hpGauge.rect;
        fillRect.width *= rate;

        hpGaugeFill.sizeDelta = fillRect.size;
    }

    public void SetLimitText(string s)
    {
        limitText.text = s;
    }

    public override UIManager.CanvasType GetCanvas()
    {
        return UIManager.CanvasType.INGAME;
    }

    public override bool IsFixed()
    {
        return true;
    }
}
