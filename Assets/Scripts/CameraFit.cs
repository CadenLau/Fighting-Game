using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    [SerializeField] private float mapWidth = 32f;   // total width of map in world units
    [SerializeField] private float mapHeight = 18f;  // total height of map in world units
    private int lastWidth, lastHeight;

    private Camera cam;


    private void Start()
    {
        cam = GetComponent<Camera>();
        FitCameraToMap();
    }

    void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            FitCameraToMap();
        }
    }

    private void FitCameraToMap()
    {
        float targetAspect = mapWidth / mapHeight;
        float windowAspect = (float)Screen.width / Screen.height;

        if (windowAspect >= targetAspect)
        {
            // if window is wider than map then fit height
            cam.orthographicSize = mapHeight / 2f;
        }
        else
        {
            // if window is narrower than map then fit width
            float scale = targetAspect / windowAspect;
            cam.orthographicSize = mapHeight / 2f * scale;
        }

        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }
}
