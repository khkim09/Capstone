using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;

public class CameraZoomController : MonoBehaviour
{
    public float zoomSpeed = 2f;
    public float moveSpeed = 0.1f;
    public float minSize = 4.5f;
    public float maxSize = 15f;

    private Camera cam;
    private Vector3 lastMousePos;
    private bool isDragging = false;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        HandleZoom();
        HandlePan();
    }

    private void HandleZoom()
    {
        if (IsMouseOverUI() || !IsMouseInGameView())
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minSize, maxSize);
        }
    }

    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(1)) // 우클릭
        {
            lastMousePos = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(1))
            isDragging = false;

        if (isDragging)
        {
            Vector3 delta = cam.ScreenToWorldPoint(lastMousePos) - cam.ScreenToWorldPoint(Input.mousePosition);
            if (CheckBounds(cam.transform.position + delta))
            {
                cam.transform.position += delta;
                lastMousePos = Input.mousePosition;
            }
        }
    }

    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsMouseInGameView()
    {
        Vector3 mousePos = Input.mousePosition;
        return (mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height);
    }

    private bool CheckBounds(Vector3 simulatePos)
    {
        float minX = -28f;
        float maxX = 25f;
        float minY = -28f;
        float maxY = 28f;

        if (minX <= simulatePos.x && simulatePos.x <= maxX && minY <= simulatePos.y && simulatePos.y <= maxY)
            return true;
        else
            return false;
    }
}
