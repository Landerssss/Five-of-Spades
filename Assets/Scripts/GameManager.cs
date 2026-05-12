using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Data (assign in Inspector)")]
    [SerializeField] private LevelData[] levels;

    [Header("References (assign in Inspector or auto-find)")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TurnSystem turnSystem;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CameraController cameraController;

    // Runtime state
    private int currentLevelIndex = 0;
    private int keysCollected = 0;
    private bool currentLevelHasKey = false;

    // Runtime objects
    private PlayerController player;
    private GameObject keyObject;
    private GameObject exitObject;
    private List<GameObject> bonusObjects = new List<GameObject>();
    private List<EnemyPatrol> enemies = new List<EnemyPatrol>();
    private Transform entitiesParent;

    // Track bonus positions for quick lookup
    private Dictionary<Vector2Int, int> bonusPositions = new Dictionary<Vector2Int, int>();
    private Vector2Int keyPosition;
    private Vector2Int exitPosition;
    private bool keyCollectedThisLevel = false;

    public int TotalKeys => levels != null ? levels.Length : 0;
    public int KeysCollected => keysCollected;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (gridManager == null) gridManager = GetComponent<GridManager>();
        if (turnSystem == null) turnSystem = GetComponent<TurnSystem>();
        if (cameraController == null) cameraController = GetComponent<CameraController>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
    }

    private void Start()
    {
        turnSystem.OnMovesExhausted += OnPlayerDied;
        turnSystem.OnPlayerMoved += OnEnemiesTurn;

        keysCollected = 0;
        currentLevelIndex = 0;
        LoadLevel(currentLevelIndex, null);
    }

    private void OnDestroy()
    {
        if (turnSystem != null)
        {
            turnSystem.OnMovesExhausted -= OnPlayerDied;
            turnSystem.OnPlayerMoved -= OnEnemiesTurn;
        }
    }

    // ============ LEVEL LOADING ============

    public void LoadLevel(int levelIndex, Vector2Int? overrideSpawn)
    {
        if (levels == null || levelIndex < 0 || levelIndex >= levels.Length)
        {
            ShowVictory();
            return;
        }

        LevelData data = levels[levelIndex];
        currentLevelIndex = levelIndex;
        currentLevelHasKey = false;
        keyCollectedThisLevel = false;

        // Clear old entities
        ClearEntities();

        // Build grid
        gridManager.BuildMap(data);

        // Camera
        cameraController.FitToMap(data.width, data.height, gridManager.GetMapCenter());

        // Create entities parent
        entitiesParent = new GameObject("Entities").transform;

        Sprite sprite = gridManager.SharedSprite;

        // Spawn player — fallback if overrideSpawn is out of bounds or on a wall
        Vector2Int spawnPos = data.playerSpawn;
        if (overrideSpawn.HasValue && gridManager.IsWalkable(overrideSpawn.Value))
        {
            spawnPos = overrideSpawn.Value;
        }
        if (player == null)
        {
            GameObject playerObj = new GameObject("Player");
            player = playerObj.AddComponent<PlayerController>();
        }
        player.Revive();
        player.Initialize(this, gridManager, turnSystem, spawnPos, sprite);

        // Spawn key
        keyPosition = data.keyPosition;
        keyObject = CreateEntity("Key", keyPosition, sprite,
            new Color(0.96f, 0.65f, 0.14f), "K", 2);

        // Spawn exit (locked initially)
        exitPosition = data.exitPosition;
        exitObject = CreateEntity("Exit", exitPosition, sprite,
            new Color(0.82f, 0.01f, 0.11f), "X", 1);

        // Spawn bonus items
        bonusPositions.Clear();
        if (data.bonusItems != null)
        {
            foreach (var bonus in data.bonusItems)
            {
                GameObject obj = CreateEntity("Bonus", bonus.position, sprite,
                    new Color(0.31f, 0.89f, 0.76f), "+" + bonus.bonusAmount, 2);
                bonusObjects.Add(obj);
                bonusPositions[bonus.position] = bonus.bonusAmount;
            }
        }

        // Spawn enemies
        if (data.enemies != null)
        {
            foreach (var enemyData in data.enemies)
            {
                if (enemyData.patrolPath == null || enemyData.patrolPath.Length == 0)
                    continue;

                GameObject enemyObj = new GameObject("Enemy");
                enemyObj.transform.parent = entitiesParent;
                EnemyPatrol patrol = enemyObj.AddComponent<EnemyPatrol>();
                patrol.Initialize(enemyData.patrolPath, sprite);
                enemies.Add(patrol);
            }
        }

        // Reset moves
        turnSystem.ResetMoves(5);

        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateMoves(turnSystem.movesLeft);
            uiManager.UpdateKeys(keysCollected, TotalKeys);
            uiManager.HideGameOver();
        }
    }

    private void ClearEntities()
    {
        if (entitiesParent != null) Destroy(entitiesParent.gameObject);

        if (keyObject != null) Destroy(keyObject);
        if (exitObject != null) Destroy(exitObject);

        foreach (var obj in bonusObjects)
            if (obj != null) Destroy(obj);
        bonusObjects.Clear();

        enemies.Clear();
        bonusPositions.Clear();
    }

    private GameObject CreateEntity(string name, Vector2Int pos, Sprite sprite,
        Color color, string label, int sortOrder)
    {
        GameObject obj = new GameObject(name);
        if (entitiesParent != null) obj.transform.parent = entitiesParent;
        obj.transform.position = new Vector3(pos.x, pos.y, 0);
        obj.transform.localScale = Vector3.one * 0.75f;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = sortOrder;

        // Text label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(obj.transform, false);
        labelObj.transform.localPosition = Vector3.zero;
        var tmp = labelObj.AddComponent<TMPro.TextMeshPro>();
        tmp.text = label;
        tmp.fontSize = 3.5f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.sortingOrder = sortOrder + 1;
        tmp.rectTransform.sizeDelta = new Vector2(1.3f, 1.3f);

        return obj;
    }

    // ============ ITEM COLLECTION (called by PlayerController during slide) ============

    public void CollectItemsAt(Vector2Int pos)
    {
        // Key
        if (!keyCollectedThisLevel && pos == keyPosition && keyObject != null)
        {
            keyCollectedThisLevel = true;
            Destroy(keyObject);
            keyObject = null;

            // Unlock exit visual
            if (exitObject != null)
            {
                SpriteRenderer sr = exitObject.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(0.49f, 0.83f, 0.13f); // Green

                TMPro.TextMeshPro tmp = exitObject.GetComponentInChildren<TMPro.TextMeshPro>();
                if (tmp != null) tmp.text = "E";
            }
        }

        // Bonus
        if (bonusPositions.ContainsKey(pos))
        {
            int bonus = bonusPositions[pos];
            bonusPositions.Remove(pos);
            turnSystem.AddMoves(bonus);

            // Destroy the bonus visual
            for (int i = bonusObjects.Count - 1; i >= 0; i--)
            {
                if (bonusObjects[i] != null)
                {
                    Vector3 p = bonusObjects[i].transform.position;
                    if (Mathf.RoundToInt(p.x) == pos.x && Mathf.RoundToInt(p.y) == pos.y)
                    {
                        Destroy(bonusObjects[i]);
                        bonusObjects.RemoveAt(i);
                        break;
                    }
                }
            }

            if (uiManager != null) uiManager.UpdateMoves(turnSystem.movesLeft);
        }
    }

    // ============ EXIT CHECK (called by PlayerController after slide completes) ============

    public void CheckExitAt(Vector2Int pos)
    {
        if (pos == exitPosition && keyCollectedThisLevel)
        {
            keysCollected++;
            if (uiManager != null) uiManager.UpdateKeys(keysCollected, TotalKeys);

            // Next level: spawn at the same XY as this level's exit
            int nextIndex = currentLevelIndex + 1;
            if (nextIndex < levels.Length)
            {
                LoadLevel(nextIndex, exitPosition);
            }
            else
            {
                ShowVictory();
            }
        }
    }

    // ============ ENEMY QUERY ============

    public bool HasEnemyAt(Vector2Int pos)
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.GridPosition == pos)
                return true;
        }
        return false;
    }

    // ============ ENEMY TURN ============

    private void OnEnemiesTurn()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.MoveNext();

                // Check if enemy landed on player
                if (player != null && enemy.GridPosition == player.GridPosition)
                {
                    OnPlayerDied();
                    return;
                }
            }
        }
    }

    // ============ DEATH / GAME OVER ============

    private void OnPlayerDied()
    {
        if (player != null) player.Die();
        if (uiManager != null) uiManager.ShowGameOver();
    }

    public void RestartCurrentLevel()
    {
        // Reset key progress for this level only (don't lose previous keys)
        keyCollectedThisLevel = false;
        LoadLevel(currentLevelIndex, null);
    }

    // ============ VICTORY ============

    private void ShowVictory()
    {
        if (uiManager != null) uiManager.ShowVictory();
    }

    // ============ RESTART FULL GAME ============

    public void RestartGame()
    {
        keysCollected = 0;
        currentLevelIndex = 0;
        LoadLevel(0, null);
    }
}
