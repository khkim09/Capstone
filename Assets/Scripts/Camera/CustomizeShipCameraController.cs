using UnityEngine;

public class CustomizeShipCameraController : MonoBehaviour
{
    [Header("Camera Zoom Settings")]
    public float zoomSpeed = 1f;
    public float minZoom = 1f;
    public float maxZoom = 10f;

    [Header("UI")]
    public GameObject customizeShipUIScreen;

    void Update()
    {
        if (!customizeShipUIScreen.activeSelf) // customize ship ui 활성화 시에만 zoom in, out 가능
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
    }
}
