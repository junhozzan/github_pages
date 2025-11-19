using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CellSO", menuName = "Scriptable Objects/CellSO")]
public class CellSO : ScriptableObject
{
    public Grid grid;
    public Cell[] cells;

    public Vector3Int WorldToCell(Vector2 pos)
    {
        return grid.WorldToCell(pos);
    }

    public Vector2 CellToWorld(Vector3Int cell)
    {
        return grid.CellToWorld(cell);
    }

    [Serializable]
    public class Cell
    {
        public Vector3Int coord;
        public CellType cellType;
        public int group;

        public Cell(Vector3Int coord, CellType cellType, int group)
        {
            this.coord = coord;
            this.cellType = cellType;
            this.group = group;
        }
    }
}

public enum CellType
{
    BUBBLE,
    SPAWNER,
    BOSS,
    CEILING,
}