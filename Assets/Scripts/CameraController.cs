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

        float screenAspect = (float)Screen.width / Screen.height;
        
        // 1. 计算适配地图所需的 Size (确保高度和宽度都能装下，并加上边距)
        float halfHeight = (mapHeight * 0.5f) + padding;
        float halfWidth = (mapWidth * 0.5f) + padding;
        float requiredSize = Mathf.Max(halfHeight, halfWidth / screenAspect);
        cam.orthographicSize = requiredSize;

        // 2. 锚点对齐左下角逻辑：
        // 地图左下角物体的中心点坐标是 (0, 0)，边缘是 (-0.5, -0.5)
        // 我们让摄像机视口的左边缘和下边缘正好等于 -0.5，这样地图就紧贴角落了
        
        float targetX = (cam.orthographicSize * screenAspect) - 0.5f;
        float targetY = cam.orthographicSize - 0.5f;

        cam.transform.position = new Vector3(targetX, targetY, -10f);
    }
}
