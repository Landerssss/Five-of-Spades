using UnityEngine;

public class GridManager : MonoBehaviour
{
    private CellType[,] grid;
    private GameObject[,] cellObjects;
    private int mapWidth;
    private int mapHeight;
    private Transform mapParent;

    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;

    /// <summary>
    /// Build the visual grid from LevelData using Prefabs from SpriteConfig.
    /// </summary>
    public void BuildMap(LevelData data, SpriteConfig config = null)
    {
        ClearMap();

        mapWidth = data.width;
        mapHeight = data.height;
        grid = new CellType[mapWidth, mapHeight];

        mapParent = new GameObject("Map").transform;
        cellObjects = new GameObject[mapWidth, mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                CellType type = data.GetCell(x, y);
                grid[x, y] = type;

                // Check if this is an edge cell
                bool isEdge = (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1);
                
                // Skip visual creation for edge cells (they still provide logic collision)
                if (isEdge)
                {
                    cellObjects[x, y] = null;
                    continue;
                }

                GameObject cell = null;

                if (type == CellType.Wall && config != null && config.wallPrefab != null)
                {
                    cell = Instantiate(config.wallPrefab, mapParent);
                    cell.name = $"Wall_{x}_{y}";
                }
                else if (type == CellType.Slime && config != null && config.slimePrefab != null)
                {
                    cell = Instantiate(config.slimePrefab, mapParent);
                    cell.name = $"Slime_{x}_{y}";
                }
                else if (type == CellType.Floor && config != null && config.floorPrefab != null)
                {
                    cell = Instantiate(config.floorPrefab, mapParent);
                    cell.name = $"Floor_{x}_{y}";
                }
                else
                {
                    // If no prefab is provided, we just create an empty GameObject 
                    // so the array isn't null, but nothing is rendered.
                    cell = new GameObject($"EmptyCell_{x}_{y}");
                    cell.transform.parent = mapParent;
                }

                if (cell != null)
                {
                    cell.transform.position = new Vector3(x, y, 0);
                    cellObjects[x, y] = cell;
                }
            }
        }
    }

    public void ClearMap()
    {
        if (mapParent != null)
        {
            Destroy(mapParent.gameObject);
            mapParent = null;
        }
        grid = null;
        cellObjects = null;
    }

    public bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < mapWidth && pos.y >= 0 && pos.y < mapHeight;
    }

    public bool IsWalkable(Vector2Int pos)
    {
        if (!IsInBounds(pos)) return false;
        CellType type = grid[pos.x, pos.y];
        return type == CellType.Floor || type == CellType.Slime;
    }

    public CellType GetCellType(Vector2Int pos)
    {
        if (!IsInBounds(pos)) return CellType.Wall;
        return grid[pos.x, pos.y];
    }

    /// <summary>
    /// Returns world-space center of the map.
    /// </summary>
    public Vector3 GetMapCenter()
    {
        return new Vector3((mapWidth - 1) * 0.5f, (mapHeight - 1) * 0.5f, 0f);
    }
}
