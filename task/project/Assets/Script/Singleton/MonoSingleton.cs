using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<T>();

                if (instance == null)
                {
                    instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            Debug.LogError($"Mono singleton is eixst ({GetType()})");
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(instance.transform.root);
        Initialize();
    }

    protected virtual void Initialize()
    {

    }
}
