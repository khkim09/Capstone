using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// 함선 커스터마이징 시 camera의 zoom-in, zoom-out과 우클릭으로의 화면 이동을 관리합니다.
/// </summary>
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

    /// <summary>
    /// 화면을 확대/축소합니다.
    /// 마우스 휠 스크롤을 이용하여 확대/축소 가능하며 최대 확대 거리 : 4.5, 최대 축소 거리 : 15 입니다.
    /// </summary>
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

    /// <summary>
    /// 우클릭을 이용하여 드래그 시 화면 이동을 구현합니다.
    /// 이동 가능 영역은 CheckBounds()를 통해 확인합니다.
    /// </summary>
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
            Debug.Log("드래그 시작");
            Vector3 delta = cam.ScreenToWorldPoint(lastMousePos) - cam.ScreenToWorldPoint(Input.mousePosition);
            if (CheckBounds(cam.transform.position + delta))
            {
                cam.transform.position += delta;
                lastMousePos = Input.mousePosition;
            }
        }
    }

    /// <summary>
    /// 유저의 마우스가 UI위에 있는지 여부를 확인합니다.
    /// </summary>
    /// <returns></returns>
    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// 유저의 마우스가 GameView 내에 위치한지 확인합니다.
    /// </summary>
    /// <returns></returns>
    private bool IsMouseInGameView()
    {
        Vector3 mousePos = Input.mousePosition;
        return mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height;
    }

    /// <summary>
    /// 카메라 화면 이동 가능 영역을 제한합니다.
    /// 가로 축 : -28 ~ 25, 세로 축 : -28 ~ 28 까지 이동 가능합니다.
    /// </summary>
    /// <param name="simulatePos"></param>
    /// <returns></returns>
    private bool CheckBounds(Vector3 simulatePos)
    {
        float minX = 0f;
        float maxX = 58f;
        float minY = 0f;
        float maxY = 59f;

        if (minX <= simulatePos.x && simulatePos.x <= maxX && minY <= simulatePos.y && simulatePos.y <= maxY)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 드래그 위치 초기화 함수
    /// </summary>
    /// <param name="mouseScreenPos"></param>
    public void StartPanFrom(Vector3 mouseScreenPos)
    {
        lastMousePos = mouseScreenPos;
        isDragging = false;
    }
}
