using UnityEngine;

/// <summary>
/// Attach this to an empty GameObject in your scene. It auto-wires all managers.
/// Just assign LevelData assets to the GameManager's levels array in Inspector.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameBootstrap : MonoBehaviour
{
    [Header("Level Data — Assign your LevelData assets here")]
    [SerializeField] private LevelData[] levels;

    private void Awake()
    {
        // Ensure all required components exist on this GameObject
        GridManager gridManager = GetOrAdd<GridManager>();
        TurnSystem turnSystem = GetOrAdd<TurnSystem>();
        CameraController cameraController = GetOrAdd<CameraController>();
        UIManager uiManager = GetOrAdd<UIManager>();
        GameManager gameManager = GetOrAdd<GameManager>();

        // Inject levels into GameManager via reflection (since we can't access private serialized field)
        // Actually, let's use a public setup method
    }

    private T GetOrAdd<T>() where T : Component
    {
        T comp = GetComponent<T>();
        if (comp == null) comp = gameObject.AddComponent<T>();
        return comp;
    }
}
