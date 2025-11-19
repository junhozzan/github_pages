using UnityEngine;
using Saga;
using DG.Tweening;

public class ObjectBubble : ObjectBase
{
    [SerializeField] SpriteRenderer spriteRenderer = null;
    [SerializeField] Collider2D coll2D = null;

    private Tween tween = null;
    public Bubble bubble { get; private set; } = null;

    public void SetBubble(Bubble bubble)
    {
        this.bubble = bubble;
        SetSprite(bubble.dataBubble.atlas, bubble.dataBubble.sprite);
    }

    public void SetCollider(bool use)
    {
        coll2D.enabled = use;
    }

    public void SetParent(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        transform.SetParent(parent);
    }

    private void SetSprite(string atlas, string sprite)
    {
        spriteRenderer.sprite = FakeAddressableManager.Instance.LoadSprite(atlas, sprite);
    }

    protected override void OnRelease()
    {
        bubble = null;
        if (tween != null && tween.IsActive())
        {
            tween.Kill();
        }

        tween = null;
        spriteRenderer.color = Color.white;
        base.OnRelease();
    }

    public void FadeOut()
    {
        SetCollider(false);
        tween = spriteRenderer.DOColor(new Color(1f, 1f, 1f, 0f), 1f)
            .OnComplete(() => SetActive(false));
    }

    public void DropOut()
    {
        SetCollider(false);
        tween = transform.DOMoveY(transform.position.y - 4f, 0.6f)
            .SetEase(Ease.InBack)
            .OnComplete(() => SetActive(false));
    }
}
