using UnityEngine;

/// <summary>
/// Holds all visual configuration: sprite variants for tiles, prefabs for entities.
/// Create via: Project → Right-Click → Create → Game → Sprite Config
/// </summary>
[CreateAssetMenu(fileName = "SpriteConfig", menuName = "Game/Sprite Config")]
public class SpriteConfig : ScriptableObject
{
    [Header("=== Tile Sprites (support multiple variants) ===")]

    [Tooltip("Wall sprite variants. A random one is picked per wall cell.")]
    public Sprite[] wallSprites;

    [Tooltip("Floor sprite variants. A random one is picked per floor cell.")]
    public Sprite[] floorSprites;

    [Header("=== Entity Prefabs (drag from Project) ===")]

    [Tooltip("Player prefab. Must have SpriteRenderer. PlayerController will be added at runtime if missing.")]
    public GameObject playerPrefab;

    [Tooltip("Enemy prefab. Must have SpriteRenderer. EnemyPatrol will be added at runtime if missing.")]
    public GameObject enemyPrefab;

    [Tooltip("Key prefab. Must have SpriteRenderer.")]
    public GameObject keyPrefab;

    [Tooltip("Exit (locked) prefab. Must have SpriteRenderer.")]
    public GameObject exitLockedPrefab;

    [Tooltip("Exit (unlocked) prefab. Must have SpriteRenderer. Swapped in when key is collected.")]
    public GameObject exitUnlockedPrefab;

    [Tooltip("Bonus item prefab variants. If multiple, picked per bonus item in order or cycled.")]
    public GameObject[] bonusPrefabs;

    // ============ HELPER METHODS ============

    /// <summary>
    /// Returns a random wall sprite, or null if list is empty.
    /// </summary>
    public Sprite GetRandomWallSprite()
    {
        if (wallSprites == null || wallSprites.Length == 0) return null;
        return wallSprites[Random.Range(0, wallSprites.Length)];
    }

    /// <summary>
    /// Returns a random floor sprite, or null if list is empty.
    /// </summary>
    public Sprite GetRandomFloorSprite()
    {
        if (floorSprites == null || floorSprites.Length == 0) return null;
        return floorSprites[Random.Range(0, floorSprites.Length)];
    }

    /// <summary>
    /// Returns a bonus prefab. Cycles through the list if multiple.
    /// </summary>
    public GameObject GetBonusPrefab(int index)
    {
        if (bonusPrefabs == null || bonusPrefabs.Length == 0) return null;
        return bonusPrefabs[index % bonusPrefabs.Length];
    }
}
