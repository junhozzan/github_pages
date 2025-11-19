using UnityEngine;

public class AutoInactive : MonoBehaviour
{
    [SerializeField] float actTime = 0f;

    private float delta = 0f;

    public void SetActTime(float time)
    {
        actTime = time;
    }

    public void OnEnable()
    {
        delta = 0f;
    }

    public void FixedUpdate()
    {
        delta += Time.fixedDeltaTime;

        if (delta >= actTime)
        {
            gameObject.SetActive(false);
        }
    }
}
