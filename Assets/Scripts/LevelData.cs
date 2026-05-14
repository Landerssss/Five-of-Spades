using UnityEngine;

public enum CellType
{
    Floor = 0,
    Wall = 1,
    Slime = 2
}

[System.Serializable]
public struct BonusItemData
{
    public Vector2Int position;
    public int bonusAmount;
}

[System.Serializable]
public struct EnemyData
{
    public Vector2Int[] patrolPath;
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int width = 7;
    public int height = 7;

    [Tooltip("Row-major order: index = y * width + x. Length must = width * height.")]
    public CellType[] cells;

    public Vector2Int playerSpawn;
    public Vector2Int exitPosition;
    public Vector2Int keyPosition;
    public BonusItemData[] bonusItems;
    public EnemyData[] enemies;

    public CellType GetCell(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return CellType.Wall;
        int index = y * width + x;
        if (index < 0 || index >= cells.Length) return CellType.Wall;
        return cells[index];
    }

    /// <summary>
    /// Editor helper: auto-fill cells array with Floor, border with Wall.
    /// </summary>
    [ContextMenu("Generate Default Grid")]
    public void GenerateDefaultGrid()
    {
        cells = new CellType[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isBorder = (x == 0 || x == width - 1 || y == 0 || y == height - 1);
                cells[y * width + x] = isBorder ? CellType.Wall : CellType.Floor;
            }
        }
    }
}
