using UnityEngine;

/// <summary>
/// Enemy patrol behavior. Attach to the Enemy prefab which should have its own SpriteRenderer.
/// Moves one step along patrolPath each player turn.
/// </summary>
public class EnemyPatrol : MonoBehaviour
{
    private Vector2Int[] patrolPath;
    private int pathIndex = 0;

    public Vector2Int GridPosition { get; private set; }

    /// <summary>
    /// Called by GameManager. Only sets up patrol data — visual is handled by the prefab.
    /// </summary>
    public void Initialize(Vector2Int[] path)
    {
        patrolPath = path;
        pathIndex = 0;

        if (path == null || path.Length == 0)
        {
            Debug.LogError("EnemyPatrol: empty patrol path!");
            return;
        }

        GridPosition = patrolPath[0];
        transform.position = new Vector3(GridPosition.x, GridPosition.y, 0);
    }

    /// <summary>
    /// Advance one step along patrol path. Called each player turn.
    /// </summary>
    public void MoveNext()
    {
        if (patrolPath == null || patrolPath.Length <= 1) return;

        pathIndex = (pathIndex + 1) % patrolPath.Length;
        GridPosition = patrolPath[pathIndex];
        transform.position = new Vector3(GridPosition.x, GridPosition.y, 0);
    }

    /// <summary>
    /// Reset to first patrol point.
    /// </summary>
    public void ResetPatrol()
    {
        pathIndex = 0;
        if (patrolPath != null && patrolPath.Length > 0)
        {
            GridPosition = patrolPath[0];
            transform.position = new Vector3(GridPosition.x, GridPosition.y, 0);
        }
    }
}
