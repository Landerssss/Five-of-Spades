using UnityEngine;

/// <summary>
/// Optional convenience script. Attach to a single GameObject in your scene.
/// It ensures all required manager components exist on this GameObject.
/// You still need to assign SpriteConfig and LevelData in the GameManager Inspector.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GetOrAdd<GridManager>();
        GetOrAdd<TurnSystem>();
        GetOrAdd<CameraController>();
        GetOrAdd<UIManager>();
        GetOrAdd<GameManager>();
    }

    private T GetOrAdd<T>() where T : Component
    {
        T comp = GetComponent<T>();
        if (comp == null) comp = gameObject.AddComponent<T>();
        return comp;
    }
}
