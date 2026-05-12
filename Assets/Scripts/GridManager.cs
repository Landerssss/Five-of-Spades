using UnityEngine;

public class GridManager : MonoBehaviour
{
    private CellType[,] grid;
    private GameObject[,] cellObjects;
    private int mapWidth;
    private int mapHeight;
    private Sprite sharedSprite;
    private Transform mapParent;

    // Colors
    private readonly Color floorColor = new Color(0.85f, 0.85f, 0.85f);
    private readonly Color wallColor = new Color(0.18f, 0.18f, 0.22f);

    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;

    public Sprite SharedSprite
    {
        get
        {
            if (sharedSprite == null) sharedSprite = CreateWhiteSquare();
            return sharedSprite;
        }
    }

    private void Awake()
    {
        sharedSprite = CreateWhiteSquare();
    }

    public void BuildMap(LevelData data)
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

                GameObject cell = new GameObject($"Cell_{x}_{y}");
                cell.transform.parent = mapParent;
                cell.transform.position = new Vector3(x, y, 0);

                SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
                sr.sprite = sharedSprite;
                sr.color = (type == CellType.Wall) ? wallColor : floorColor;
                sr.sortingOrder = 0;

                cellObjects[x, y] = cell;
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
        return grid[pos.x, pos.y] == CellType.Floor;
    }

    /// <summary>
    /// Returns world-space center of the map.
    /// </summary>
    public Vector3 GetMapCenter()
    {
        return new Vector3((mapWidth - 1) * 0.5f, (mapHeight - 1) * 0.5f, 0f);
    }

    private Sprite CreateWhiteSquare()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
