using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 함선 커스터마이징 시 camera의 zoom-in, zoom-out과 우클릭으로의 화면 이동을 관리합니다.
/// </summary>
public class CameraZoomController : MonoBehaviour
{
    /// <summary>
    /// 카메라 줌 속도
    /// </summary>
    private float zoomSpeed = 2f;

    /// <summary>
    /// 카메라 최대 확대 줌인
    /// </summary>
    private float minSize = 4.5f;

    /// <summary>
    /// 카메라 최소치 줌 아웃
    /// </summary>
    private float maxSize = 15f;

    /// <summary>
    /// 설계도 화면
    /// </summary>
    public GameObject customizeUI;

    /// <summary>
    /// 조정할 메인 카메라
    /// </summary>
    private Camera cam;

    /// <summary>
    /// 가장 최신 줌 사이즈
    /// </summary>
    [HideInInspector] public float lastZoomSize = 5f;

    private void Start()
    {
        cam = Camera.main;
        Camera.main.transform.position = new Vector3(0, 0, Camera.main.transform.position.z);
    }

    /// <summary>
    /// 설계도 작업 시에만 카메라 컨트롤 적용
    /// </summary>
    private void Update()
    {
        if (CrewUIHandler.Instance.GetCurrentActiveUI() == customizeUI)
        {
            HandleZoom();
            CameraMove();
        }
    }

    /// <summary>
    /// 화면을 확대/축소합니다.
    /// 마우스 휠 스크롤을 이용하여 확대/축소 가능하며 최대 확대 거리 : 4.5, 최대 축소 거리 : 15 입니다.
    /// </summary>
    private void HandleZoom()
    {
        // 마우스가 게임 뷰 위에 있을 때만 줌 활성화
        if (!IsMouseInGameView())
            return;

        // 방이 아닌 UI 위에서 줌 금지
        if (EventSystem.current.IsPointerOverGameObject() && !isMouseOverRoomOrGrid())
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minSize, maxSize);

        lastZoomSize = cam.orthographicSize;
    }

    /// <summary>
    /// 카메라 이동 구현 및 이동 제한
    /// </summary>
    private void CameraMove()
    {
        Vector3 moveDir = Vector3.zero;
        int stepSize = (int)cam.orthographicSize; // 줌 레벨 기준 이동 거리
        KeyCode lastInputKey = KeyCode.None;

        if (Input.GetKeyDown(KeyCode.W))
        {
            moveDir += Vector3.up;
            lastInputKey = KeyCode.W;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            moveDir += Vector3.down;
            lastInputKey = KeyCode.S;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            moveDir += Vector3.left;
            lastInputKey = KeyCode.A;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            moveDir += Vector3.right;
            lastInputKey = KeyCode.D;
        }

        if (moveDir != Vector3.zero)
        {
            Vector3 newPos = cam.transform.position + moveDir * stepSize;
            newPos.z = cam.transform.position.z; // z는 그대로 유지

            if (CheckBounds(newPos))
                cam.transform.position = newPos;
            else
            {
                switch (lastInputKey)
                {
                    case KeyCode.W:
                        cam.transform.position = new Vector3(newPos.x, 58.5f, newPos.z);
                        break;
                    case KeyCode.S:
                        cam.transform.position = new Vector3(newPos.x, 1.5f, newPos.z);
                        break;
                    case KeyCode.A:
                        cam.transform.position = new Vector3(4.5f, newPos.y, newPos.z);
                        break;
                    case KeyCode.D:
                        cam.transform.position = new Vector3(55.5f, newPos.y, newPos.z);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /* 마우스 창고 위
    /// <summary>
    /// 마우스가 UI가 아닌 창고 위에만 있는지 확인
    /// </summary>
    /// <returns></returns>
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
    */

    /// <summary>
    /// 유저의 마우스가 GameView 내에 위치한지 확인합니다.
    /// </summary>
    private bool IsMouseInGameView()
    {
        Vector3 mousePos = Input.mousePosition;
        return mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height;
    }

    /// <summary>
    /// 방 ui 위에 있을 때는 줌 가능
    /// </summary>
    /// <returns></returns>
    private bool isMouseOverRoomOrGrid()
    {
        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
            return true;

        return false;
    }

    /// <summary>
    /// 카메라 화면 이동 가능 영역을 제한합니다.
    /// </summary>
    private bool CheckBounds(Vector3 simulatePos)
    {
        // 수정 필요
        float minX = 4.5f;
        float maxX = 55.5f;
        float minY = 1.5f;
        float maxY = 58.5f;

        return minX <= simulatePos.x && simulatePos.x <= maxX && minY <= simulatePos.y && simulatePos.y <= maxY;
    }
}
