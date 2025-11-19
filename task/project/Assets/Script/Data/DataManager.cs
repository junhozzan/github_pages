using System;
using System.Collections;
using System.Linq;
using System.Reflection;

public class DataManager : Singleton<DataManager>, IPatch
{
    public readonly DataLoaderMode mode = new();
    public readonly DataLoaderSaga saga = new();

    int IPatch.GetPatchCount()
    {
        return 1;
    }

    IEnumerator IPatch.Patch(Action<int> onProgress)
    {
        var loaders = GetAllLoader();
        foreach (var loader in loaders)
        {
            loader.Load();
        }

#if UNITY_EDITOR
        foreach (var loader in loaders)
        {
            loader.Verify(this);
        }
#endif

        onProgress?.Invoke(1);
        yield break;
    }

    private DataLoaderBase[] GetAllLoader()
    {
        return GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => typeof(DataLoaderBase).IsAssignableFrom(x.FieldType))
            .Select(x => x.GetValue(this) as DataLoaderBase)
            .ToArray();
    }
}