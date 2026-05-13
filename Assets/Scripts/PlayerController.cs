using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Player controller. Attach to the Player prefab which should have its own SpriteRenderer.
/// If no SpriteRenderer exists, a fallback is created.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float slideSpeed = 15f;

    public Vector2Int GridPosition { get; private set; }

    private bool isSliding = false;
    private bool isDead = false;

    private GameManager gameManager;
    private GridManager gridManager;
    private TurnSystem turnSystem;
    private SpriteRenderer spriteRenderer;

    // Cached original color from the prefab's SpriteRenderer
    private Color originalColor;

    /// <summary>
    /// Called by GameManager each time a level loads.
    /// No longer needs a Sprite parameter — the prefab already has its sprite.
    /// </summary>
    public void Initialize(GameManager gm, GridManager grid, TurnSystem turn, Vector2Int startPos)
    {
        gameManager = gm;
        gridManager = grid;
        turnSystem = turn;
        GridPosition = startPos;
        transform.position = new Vector3(startPos.x, startPos.y, 0);
        isDead = false;
        isSliding = false;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Cache original color from prefab
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Update()
    {
        if (isSliding || isDead) return;

        Vector2Int dir = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            dir = Vector2Int.right;

        if (dir != Vector2Int.zero)
        {
            Vector2Int target = CalculateSlideTarget(GridPosition, dir);
            if (target != GridPosition)
            {
                StartCoroutine(SlideCoroutine(dir, target));
            }
        }
    }

    /// <summary>
    /// Slide from start in direction until hitting wall/boundary/enemy. Returns final position.
    /// </summary>
    private Vector2Int CalculateSlideTarget(Vector2Int start, Vector2Int direction)
    {
        Vector2Int current = start;
        while (true)
        {
            Vector2Int next = current + direction;
            if (!gridManager.IsWalkable(next)) break;
            if (gameManager.HasEnemyAt(next)) break;
            current = next;
        }
        return current;
    }

    private IEnumerator SlideCoroutine(Vector2Int direction, Vector2Int target)
    {
        isSliding = true;

        // Build path cell by cell
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = GridPosition;
        while (current != target)
        {
            current += direction;
            path.Add(current);
        }

        // Animate through each cell
        foreach (Vector2Int cell in path)
        {
            Vector3 worldTarget = new Vector3(cell.x, cell.y, 0);
            while (Vector3.Distance(transform.position, worldTarget) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, worldTarget, slideSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = worldTarget;
            GridPosition = cell;

            // Collect items at each cell passed through
            gameManager.CollectItemsAt(cell);
        }

        isSliding = false;

        // Consume one action point
        turnSystem.ConsumeMove();

        // Check exit only at final landing position & only if still alive
        if (turnSystem.movesLeft > 0)
        {
            gameManager.CheckExitAt(GridPosition);
        }
    }

    public void SetPosition(Vector2Int pos)
    {
        GridPosition = pos;
        transform.position = new Vector3(pos.x, pos.y, 0);
    }

    public void Die()
    {
        isDead = true;
        StopAllCoroutines();
        isSliding = false;
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 0.5f);
    }

    public void Revive()
    {
        isDead = false;
        isSliding = false;
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}
