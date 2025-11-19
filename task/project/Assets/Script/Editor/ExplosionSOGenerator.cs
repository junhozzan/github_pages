using System.Linq;
using UnityEditor;
using UnityEngine;

public class ExplosionSOGenerator
{
    [MenuItem("Assets/Prefab Tools/Generate ExplosionSO")]
    public static void GenerateSOFromPrefab()
    {
        var prefab = Selection.activeObject as GameObject;
        if (prefab == null)
        {
            Debug.LogError("dont select");
            return;
        }

        ExplosionSO so = ScriptableObject.CreateInstance<ExplosionSO>();
        so.evenCoords = prefab.GetComponentsInChildren<CellViewer>()
            .Select(x => x.coord)
            .ToArray();

        so.oddCoords = so.evenCoords.Select(k => (k.y % 2 == 0) ? k : new Vector3Int(k.x + 1, k.y)).ToArray();

        // 저장 경로
        string path = $"Assets/Resources/so_{prefab.name}.asset";
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"complete → {path}");
    }
}
