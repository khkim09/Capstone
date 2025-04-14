using System.Collections.Generic;
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

    // 드래그 가능 여부를 제어하는 플래그
    private bool isPanningEnabled = true;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        HandleZoom();
        if (isPanningEnabled) // 카메라 이동이 활성화된 경우에만 실행
            HandlePan();
    }

    /// <summary>
    /// 화면을 확대/축소합니다.
    /// 마우스 휠 스크롤을 이용하여 확대/축소 가능하며 최대 확대 거리 : 4.5, 최대 축소 거리 : 15 입니다.
    /// </summary>
    private void HandleZoom()
    {
        // UI 위에 있을 때는 줌 비활성화하지만, 창고만 있는 경우에는 줌 허용
        if ((IsMouseOverUI() && !IsMouseOverStorageOnly()) || !IsMouseInGameView())
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minSize, maxSize);
    }

    /// <summary>
    /// 우클릭을 이용하여 드래그 시 화면 이동을 구현합니다.
    /// 이동 가능 영역은 CheckBounds()를 통해 확인합니다.
    /// </summary>
    private void HandlePan()
    {
        // 드래그 중인 아이템이 있을 때는 카메라 이동 비활성화
        if (!isPanningEnabled)
            return;

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

    /// <summary>
    /// 유저의 마우스가 UI위에 있는지 여부를 확인합니다.
    /// </summary>
    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    // 마우스가 UI가 아닌 창고 위에만 있는지 확인
    private bool IsMouseOverStorageOnly()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
            return false; // UI가 아니면 false

        // 마우스 위치에서 레이캐스트
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        // 창고에 맞히고 다른 UI가 없는지 확인
        if (hit.collider != null && hit.collider.GetComponentInParent<StorageRoomBase>() != null)
        {
            // 추가 검사: UI 캔버스에 맞히는지 확인
            PointerEventData eventData = new(EventSystem.current);
            eventData.position = Input.mousePosition;

            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(eventData, results);

            // UI 요소가 있는지 확인
            bool hasUIElement = false;
            foreach (RaycastResult result in results)
                // Canvas의 자식이거나 UI 레이어인 경우 UI 요소로 간주
                if (result.gameObject.GetComponentInParent<Canvas>() != null ||
                    result.gameObject.layer == LayerMask.NameToLayer("UI"))
                {
                    hasUIElement = true;
                    break;
                }

            // UI 요소가 없고 창고에만 맞힌 경우 true 반환
            return !hasUIElement;
        }

        return false;
    }

    /// <summary>
    /// 유저의 마우스가 GameView 내에 위치한지 확인합니다.
    /// </summary>
    private bool IsMouseInGameView()
    {
        Vector3 mousePos = Input.mousePosition;
        return mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height;
    }

    /// <summary>
    /// 카메라 화면 이동 가능 영역을 제한합니다.
    /// 가로 축 : -28 ~ 25, 세로 축 : -28 ~ 28 까지 이동 가능합니다.
    /// </summary>
    private bool CheckBounds(Vector3 simulatePos)
    {
        float minX = 0f;
        float maxX = 58f;
        float minY = 0f;
        float maxY = 59f;

        return minX <= simulatePos.x && simulatePos.x <= maxX && minY <= simulatePos.y && simulatePos.y <= maxY;
    }

    /// <summary>
    /// 드래그 위치 초기화 함수
    /// </summary>
    public void StartPanFrom(Vector3 mouseScreenPos)
    {
        lastMousePos = mouseScreenPos;
        isDragging = false;
    }

    /// <summary>
    /// 카메라 이동을 비활성화합니다.
    /// 아이템 드래그 중일 때 호출됩니다.
    /// </summary>
    public void DisablePanning()
    {
        isPanningEnabled = false;
        isDragging = false; // 진행 중이던 카메라 드래그도 중단
    }

    /// <summary>
    /// 카메라 이동을 활성화합니다.
    /// 아이템 드래그가 끝났을 때 호출됩니다.
    /// </summary>
    public void EnablePanning()
    {
        isPanningEnabled = true;
    }
}
