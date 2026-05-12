#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class LevelDataCreator : EditorWindow
{
    [MenuItem("Tools/Create Example Levels")]
    public static void CreateExampleLevels()
    {
        if (!AssetDatabase.IsValidFolder("Assets/LevelData"))
            AssetDatabase.CreateFolder("Assets", "LevelData");

        CreateLevel1();
        CreateLevel2();
        CreateLevel3();
        CreateLevel4();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("4 example levels created in Assets/LevelData/");
    }

    // ===== LEVEL 1: 7x7 简单入门 =====
    // Layout (Y=0 is bottom row):
    //  Row6: # # # # # # #
    //  Row5: # . . . . . #
    //  Row4: # . # . # . #
    //  Row3: # . . . . . #
    //  Row2: # . # . # . #
    //  Row1: # . . . . . #
    //  Row0: # # # # # # #
    //  Player(1,1) Key(5,5) Exit(5,1) Bonus(3,3,+2)
    private static void CreateLevel1()
    {
        LevelData data = ScriptableObject.CreateInstance<LevelData>();
        data.width = 7;
        data.height = 7;
        data.cells = new CellType[]
        {
            // Row 0 (y=0, bottom)
            CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall,
            // Row 1
            CellType.Wall, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Wall,
            // Row 2
            CellType.Wall, CellType.Floor, CellType.Wall, CellType.Floor, CellType.Wall, CellType.Floor, CellType.Wall,
            // Row 3
            CellType.Wall, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Wall,
            // Row 4
            CellType.Wall, CellType.Floor, CellType.Wall, CellType.Floor, CellType.Wall, CellType.Floor, CellType.Wall,
            // Row 5
            CellType.Wall, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Wall,
            // Row 6 (top)
            CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall,
        };
        data.playerSpawn = new Vector2Int(1, 1);
        data.keyPosition = new Vector2Int(5, 5);
        data.exitPosition = new Vector2Int(5, 1);
        data.bonusItems = new BonusItemData[]
        {
            new BonusItemData { position = new Vector2Int(3, 3), bonusAmount = 2 }
        };
        data.enemies = new EnemyData[0];

        AssetDatabase.CreateAsset(data, "Assets/LevelData/Level_01.asset");
    }

    // ===== LEVEL 2: 7x7 有敌人 =====
    // 玩家出生在(5,1) — 即Level1的出口位置
    // Layout:
    //  Row6: # # # # # # #
    //  Row5: # . . . . . #
    //  Row4: # . . # . . #
    //  Row3: # . . . . . #
    //  Row2: # . . # . . #
    //  Row1: # . . . . . #
    //  Row0: # # # # # # #
    //  Key(1,5) Exit(1,1) Enemy patrols (3,5)→(3,1)→(3,5)
    private static void CreateLevel2()
    {
        LevelData data = ScriptableObject.CreateInstance<LevelData>();
        data.width = 7;
        data.height = 7;
        data.cells = new CellType[]
        {
            CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall,
            CellType.Wall, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Wall,
            CellType.Wall, CellType.Floor, CellType.Floor, CellType.Wall, CellType.Floor, CellType.Floor, CellType.Wall,
            CellType.Wall, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Wall,
            CellType.Wall, CellType.Floor, CellType.Floor, CellType.Wall, CellType.Floor, CellType.Floor, CellType.Wall,
            CellType.Wall, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Wall,
            CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall,
        };
        data.playerSpawn = new Vector2Int(5, 1);
        data.keyPosition = new Vector2Int(1, 5);
        data.exitPosition = new Vector2Int(1, 1);
        data.bonusItems = new BonusItemData[]
        {
            new BonusItemData { position = new Vector2Int(5, 5), bonusAmount = 1 }
        };
        data.enemies = new EnemyData[]
        {
            new EnemyData { patrolPath = new Vector2Int[]
            {
                new Vector2Int(3, 5), new Vector2Int(3, 3), new Vector2Int(3, 1),
                new Vector2Int(3, 3), new Vector2Int(3, 5)
            }}
        };

        AssetDatabase.CreateAsset(data, "Assets/LevelData/Level_02.asset");
    }

    // ===== LEVEL 3: 9x9 大一圈 =====
    // 玩家出生在(1,1) — Level2的出口位置
    // 注意：地图变成 9x9
    private static void CreateLevel3()
    {
        LevelData data = ScriptableObject.CreateInstance<LevelData>();
        data.width = 9;
        data.height = 9;
        // Build cells
        CellType[] c = new CellType[81];
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                int idx = y * 9 + x;
                bool border = (x == 0 || x == 8 || y == 0 || y == 8);
                // Internal walls
                bool internalWall = (x == 2 && y == 2) || (x == 4 && y == 4) ||
                                    (x == 6 && y == 2) || (x == 6 && y == 6) ||
                                    (x == 2 && y == 6) || (x == 4 && y == 2) ||
                                    (x == 4 && y == 6);
                c[idx] = (border || internalWall) ? CellType.Wall : CellType.Floor;
            }
        }
        data.cells = c;
        data.playerSpawn = new Vector2Int(1, 1);
        data.keyPosition = new Vector2Int(7, 7);
        data.exitPosition = new Vector2Int(7, 1);
        data.bonusItems = new BonusItemData[]
        {
            new BonusItemData { position = new Vector2Int(1, 7), bonusAmount = 2 },
            new BonusItemData { position = new Vector2Int(5, 3), bonusAmount = 1 }
        };
        data.enemies = new EnemyData[]
        {
            new EnemyData { patrolPath = new Vector2Int[]
            {
                new Vector2Int(3, 1), new Vector2Int(3, 3), new Vector2Int(3, 5),
                new Vector2Int(3, 7), new Vector2Int(3, 5), new Vector2Int(3, 3)
            }},
            new EnemyData { patrolPath = new Vector2Int[]
            {
                new Vector2Int(5, 7), new Vector2Int(5, 5), new Vector2Int(5, 1),
                new Vector2Int(5, 5)
            }}
        };

        AssetDatabase.CreateAsset(data, "Assets/LevelData/Level_03.asset");
    }

    // ===== LEVEL 4: 5x5 小一圈 最终关 =====
    // 玩家出生在(3,1) — 注意 Level3 出口(7,1)映射到小地图需调整
    // 这里 playerSpawn 作为默认，但实际会被 overrideSpawn 覆盖
    // 如果 overrideSpawn 超出地图则 fallback 到 playerSpawn
    private static void CreateLevel4()
    {
        LevelData data = ScriptableObject.CreateInstance<LevelData>();
        data.width = 5;
        data.height = 5;
        data.cells = new CellType[]
        {
            CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall,
            CellType.Wall, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Wall,
            CellType.Wall, CellType.Floor, CellType.Wall, CellType.Floor, CellType.Wall,
            CellType.Wall, CellType.Floor, CellType.Floor, CellType.Floor, CellType.Wall,
            CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall, CellType.Wall,
        };
        data.playerSpawn = new Vector2Int(1, 1);
        data.keyPosition = new Vector2Int(3, 3);
        data.exitPosition = new Vector2Int(1, 3);
        data.bonusItems = new BonusItemData[0];
        data.enemies = new EnemyData[]
        {
            new EnemyData { patrolPath = new Vector2Int[]
            {
                new Vector2Int(3, 1), new Vector2Int(1, 1), new Vector2Int(1, 3),
                new Vector2Int(3, 3), new Vector2Int(3, 1)
            }}
        };

        AssetDatabase.CreateAsset(data, "Assets/LevelData/Level_04.asset");
    }
}
#endif
