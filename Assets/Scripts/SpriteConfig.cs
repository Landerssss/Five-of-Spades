using UnityEngine;

/// <summary>
/// Holds all visual configuration: strictly prefabs for everything.
/// </summary>
[CreateAssetMenu(fileName = "SpriteConfig", menuName = "Game/Sprite Config")]
public class SpriteConfig : ScriptableObject
{
    [Header("=== Tile Prefabs ===")]
    [Tooltip("Wall prefab.")]
    public GameObject wallPrefab;

    [Tooltip("Floor prefab.")]
    public GameObject floorPrefab;

    [Tooltip("Slime prefab. Player stops upon hitting this tile.")]
    public GameObject slimePrefab;

    [Header("=== Entity Prefabs ===")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject keyPrefab;
    public GameObject exitLockedPrefab;
    public GameObject exitUnlockedPrefab;
    public GameObject[] bonusPrefabs;

    [Header("=== UI Icons (Drag Sprites/Images here) ===")]
    [Tooltip("Sprite for a locked key slot.")]
    public Sprite uiLockIcon;
    [Tooltip("Sprite for an unlocked key slot.")]
    public Sprite uiKeyIcon;

    // ============ HELPER METHODS ============
    public GameObject GetBonusPrefab(int index)
    {
        if (bonusPrefabs == null || bonusPrefabs.Length == 0) return null;
        return bonusPrefabs[index % bonusPrefabs.Length];
    }
}
