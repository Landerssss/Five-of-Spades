using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float padding = 1.5f;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    /// <summary>
    /// Adjust camera to fit the map with some padding. Call after BuildMap.
    /// </summary>
    public void FitToMap(int mapWidth, int mapHeight, Vector3 center)
    {
        if (cam == null) cam = Camera.main;

        cam.transform.position = new Vector3(center.x, center.y, -10f);

        float halfHeight = (mapHeight * 0.5f) + padding;
        float halfWidth = (mapWidth * 0.5f) + padding;
        float screenAspect = (float)Screen.width / Screen.height;
        float requiredSize = Mathf.Max(halfHeight, halfWidth / screenAspect);

        cam.orthographicSize = requiredSize;
    }
}
