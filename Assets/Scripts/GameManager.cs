using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Visual Config (assign SpriteConfig asset)")]
    [SerializeField] private SpriteConfig spriteConfig;

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
    private bool keyCollectedThisLevel = false;

    // Runtime objects
    private PlayerController player;
    private GameObject playerObj;
    private GameObject keyObject;
    private GameObject exitObject;
    private List<GameObject> bonusObjects = new List<GameObject>();
    private List<EnemyPatrol> enemies = new List<EnemyPatrol>();
    private Transform entitiesParent;

    // Track bonus positions for quick lookup
    private Dictionary<Vector2Int, int> bonusPositions = new Dictionary<Vector2Int, int>();
    private Vector2Int keyPosition;
    private Vector2Int exitPosition;

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
        keyCollectedThisLevel = false;

        // Clear old entities
        ClearEntities();

        // Build grid (pass spriteConfig for wall/floor variants)
        gridManager.BuildMap(data, spriteConfig);

        // Camera
        cameraController.FitToMap(data.width, data.height, gridManager.GetMapCenter());

        // Create entities parent
        entitiesParent = new GameObject("Entities").transform;

        // ---- Spawn player ----
        Vector2Int spawnPos = data.playerSpawn;
        if (overrideSpawn.HasValue && gridManager.IsWalkable(overrideSpawn.Value))
        {
            spawnPos = overrideSpawn.Value;
        }

        if (playerObj == null)
        {
            playerObj = SpawnPrefabOrFallback(
                spriteConfig != null ? spriteConfig.playerPrefab : null,
                "Player", spawnPos, 5, Color.blue);
            player = playerObj.GetComponent<PlayerController>();
            if (player == null) player = playerObj.AddComponent<PlayerController>();
        }
        player.Revive();
        player.Initialize(this, gridManager, turnSystem, spawnPos);

        // ---- Spawn key ----
        keyPosition = data.keyPosition;
        keyObject = SpawnPrefabOrFallback(
            spriteConfig != null ? spriteConfig.keyPrefab : null,
            "Key", keyPosition, 2, new Color(0.96f, 0.65f, 0.14f));
        keyObject.transform.SetParent(entitiesParent);

        // ---- Spawn exit (locked) ----
        exitPosition = data.exitPosition;
        exitObject = SpawnPrefabOrFallback(
            spriteConfig != null ? spriteConfig.exitLockedPrefab : null,
            "Exit", exitPosition, 1, new Color(0.82f, 0.01f, 0.11f));
        exitObject.transform.SetParent(entitiesParent);

        // ---- Spawn bonus items ----
        bonusPositions.Clear();
        if (data.bonusItems != null)
        {
            for (int i = 0; i < data.bonusItems.Length; i++)
            {
                var bonus = data.bonusItems[i];
                GameObject prefab = (spriteConfig != null) ? spriteConfig.GetBonusPrefab(i) : null;
                GameObject obj = SpawnPrefabOrFallback(
                    prefab, "Bonus", bonus.position, 2, new Color(0.31f, 0.89f, 0.76f));
                obj.transform.SetParent(entitiesParent);
                bonusObjects.Add(obj);
                bonusPositions[bonus.position] = bonus.bonusAmount;
            }
        }

        // ---- Spawn enemies ----
        if (data.enemies != null)
        {
            foreach (var enemyData in data.enemies)
            {
                if (enemyData.patrolPath == null || enemyData.patrolPath.Length == 0)
                    continue;

                GameObject prefab = (spriteConfig != null) ? spriteConfig.enemyPrefab : null;
                Vector2Int startPos = enemyData.patrolPath[0];
                GameObject enemyObj = SpawnPrefabOrFallback(
                    prefab, "Enemy", startPos, 3, new Color(0.56f, 0.07f, 1f));
                enemyObj.transform.SetParent(entitiesParent);

                EnemyPatrol patrol = enemyObj.GetComponent<EnemyPatrol>();
                if (patrol == null) patrol = enemyObj.AddComponent<EnemyPatrol>();
                patrol.Initialize(enemyData.patrolPath);
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

    /// <summary>
    /// If prefab is assigned, instantiate it. Otherwise create a fallback colored square.
    /// </summary>
    private GameObject SpawnPrefabOrFallback(GameObject prefab, string name,
        Vector2Int gridPos, int sortOrder, Color fallbackColor)
    {
        GameObject obj;

        if (prefab != null)
        {
            obj = Instantiate(prefab);
            obj.name = name;
        }
        else
        {
            // Fallback: create a colored square (same as old behavior)
            obj = new GameObject(name);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateFallbackSprite();
            sr.color = fallbackColor;
            sr.sortingOrder = sortOrder;
            obj.transform.localScale = Vector3.one * 0.75f;
        }

        obj.transform.position = new Vector3(gridPos.x, gridPos.y, 0);

        // Ensure SpriteRenderer sorting order is set for prefabs too
        SpriteRenderer prefabSR = obj.GetComponent<SpriteRenderer>();
        if (prefabSR != null)
        {
            prefabSR.sortingOrder = sortOrder;
        }

        return obj;
    }

    private Sprite _fallbackSprite;
    private Sprite CreateFallbackSprite()
    {
        if (_fallbackSprite != null) return _fallbackSprite;
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] px = new Color[size * size];
        for (int i = 0; i < px.Length; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        _fallbackSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size);
        return _fallbackSprite;
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

    // ============ ITEM COLLECTION (called by PlayerController during slide) ============

    public void CollectItemsAt(Vector2Int pos)
    {
        // Key
        if (!keyCollectedThisLevel && pos == keyPosition && keyObject != null)
        {
            keyCollectedThisLevel = true;
            Destroy(keyObject);
            keyObject = null;

            // Swap exit to unlocked visual
            SwapExitToUnlocked();
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

    /// <summary>
    /// Swap exit object to the unlocked prefab, or just recolor if no prefab.
    /// </summary>
    private void SwapExitToUnlocked()
    {
        if (exitObject == null) return;

        if (spriteConfig != null && spriteConfig.exitUnlockedPrefab != null)
        {
            // Replace with unlocked prefab
            Vector3 pos = exitObject.transform.position;
            Transform parent = exitObject.transform.parent;
            int sortOrder = 1;
            SpriteRenderer oldSR = exitObject.GetComponent<SpriteRenderer>();
            if (oldSR != null) sortOrder = oldSR.sortingOrder;

            Destroy(exitObject);
            exitObject = Instantiate(spriteConfig.exitUnlockedPrefab);
            exitObject.name = "Exit_Unlocked";
            exitObject.transform.position = pos;
            exitObject.transform.SetParent(parent);

            SpriteRenderer newSR = exitObject.GetComponent<SpriteRenderer>();
            if (newSR != null) newSR.sortingOrder = sortOrder;
        }
        else
        {
            // Fallback: just recolor
            SpriteRenderer sr = exitObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.49f, 0.83f, 0.13f);

            TMPro.TextMeshPro tmp = exitObject.GetComponentInChildren<TMPro.TextMeshPro>();
            if (tmp != null) tmp.text = "E";
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
