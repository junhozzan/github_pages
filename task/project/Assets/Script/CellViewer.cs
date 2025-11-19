using UnityEngine;

public class CellViewer : MonoBehaviour
{
    public CellType cellType = CellType.BUBBLE;
    public int group = -1;
    public Vector3Int coord = Vector3Int.zero;

    private void OnDrawGizmos()
    {
        var grid = GetComponentsInParent<Grid>();
        if (grid.Length > 0)
        {
            coord = grid[0].WorldToCell(transform.position);
        }

        Color color = Color.white;
        switch (cellType)
        {
            case CellType.BUBBLE:
                color = Color.green;
                break;
            case CellType.BOSS:
                color = Color.blue;
                break;
            case CellType.SPAWNER:
                color = Color.yellow;
                break;
        }

        color.a = 0.3f;
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
