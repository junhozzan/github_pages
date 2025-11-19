using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public static class CellSOGenerator
{
    [MenuItem("Assets/Prefab Tools/Generate CellSO")]
    public static void GenerateSOFromPrefab()
    {
        var prefab = Selection.activeObject as GameObject;
        if (prefab == null)
        {
            Debug.LogError("dont select");
            return;
        }

        CellSO so = ScriptableObject.CreateInstance<CellSO>();
        so.grid = Resources.Load("so_cell_grid").GetComponent<Grid>();
        so.cells = prefab.GetComponentsInChildren<CellViewer>()
            .Select(x => new CellSO.Cell(x.coord, x.cellType, x.group))
            .ToArray();

        // 저장 경로
        string path = $"Assets/Resources/so_{prefab.name}.asset";
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"complete → {path}");
    }
}