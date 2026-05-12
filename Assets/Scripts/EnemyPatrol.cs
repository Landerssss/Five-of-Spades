using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    private Vector2Int[] patrolPath;
    private int pathIndex = 0;

    public Vector2Int GridPosition { get; private set; }

    public void Initialize(Vector2Int[] path, Sprite sprite)
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
        transform.localScale = Vector3.one * 0.8f;

        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.56f, 0.07f, 1f); // Purple
        sr.sortingOrder = 3;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(transform, false);
        labelObj.transform.localPosition = Vector3.zero;
        var tmp = labelObj.AddComponent<TMPro.TextMeshPro>();
        tmp.text = "!";
        tmp.fontSize = 4;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.sortingOrder = 4;
        tmp.rectTransform.sizeDelta = new Vector2(1f, 1f);
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
