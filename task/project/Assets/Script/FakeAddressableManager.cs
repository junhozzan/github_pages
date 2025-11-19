using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class FakeAddressableManager : Singleton<FakeAddressableManager>
{
    private readonly Dictionary<string, Object> cachedObjects = new();
    private readonly Dictionary<string, Sprite> cachedSprites = new();

    private T TempLoad<T>(string name) where T : Object
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        if (!cachedObjects.TryGetValue(name, out var v))
        {
            cachedObjects.Add(name, v = Resources.Load(name));
        }

        return v as T;
    }

    public GameObject Load(string name)
    {
        return TempLoad<GameObject>(name);
    }

    public TextAsset LoadTextAsset(string name)
    {
        return TempLoad<TextAsset>(name);
    }

    public T LoadSO<T>(string name) where T : ScriptableObject
    {
        return TempLoad<T>(name);
    }

    public Sprite LoadSprite(string atlasName, string spriteName)
    {
        var key = $"{atlasName}{spriteName}";
        if (!cachedSprites.TryGetValue(key, out var sprite))
        {
            var sa = TempLoad<SpriteAtlas>(atlasName);
            if (sa != null)
            {
                sprite = sa.GetSprite(spriteName);
            }

            if (sprite == null)
            {
                Debug.Log($"sprite is null ({atlasName})({spriteName})");
            }

            cachedSprites.Add(key, sprite);
        }

        return sprite;
    }
}
