using System.Collections;
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
    /// 조정할 메인 카메라
    /// </summary>
    private Camera cam;

    [SerializeField] public Ship targetShip;

    /// <summary>
    /// 가장 최신 줌 사이즈
    /// </summary>
    [HideInInspector] public float lastZoomSize = 5f;

    /// <summary>
    /// 함선 위치로 카메라 세팅, 줌 사이즈 = 5
    /// </summary>
    private void OnEnable()
    {
        cam = Camera.main;

        StartCoroutine(CameraCoroutine());
    }

    private IEnumerator CameraCoroutine()
    {
        yield return null;

        targetShip = GameManager.Instance.playerShip;

        Vector3 startPos = GetCameraStartPositionToOriginShip();
        Camera.main.transform.position = new Vector3(startPos.x, startPos.y, Camera.main.transform.position.z);
        Camera.main.orthographicSize = (maxSize + minSize)/2;
    }


    /// <summary>
    /// 설계도 작업 시에만 카메라 컨트롤 적용
    /// </summary>
    private void Update()
    {
        // tag로 zoom 가능한 곳 지정 후 검색해서 이 CanControlCamera 태그에서만 가능하도록
        HandleZoom();
        CameraMove();
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

    /// <summary>
    /// 기존 소유 함선을 이루는 방들의 중심으로 카메라 시작 위치 보정
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCameraStartPositionToOriginShip()
    {
        List<Room> allRooms = targetShip.GetAllRooms();
        List<ShipWeapon> allWeapons = targetShip.GetAllWeapons(); // 함선의 모든 무기 가져오기 (구현 필요)

        // 배치된 방 없으면 그리드 중앙
        if ((allRooms == null || allRooms.Count == 0) && (allWeapons == null || allWeapons.Count == 0))
            return targetShip.GetWorldPositionFromGrid(targetShip.GetGridSize() / 2);

        // 전체 타일 평균 위치 계산
        List<Vector2Int> allTiles = new();

        foreach (Room room in allRooms)
            allTiles.AddRange(room.GetOccupiedTiles());

        foreach (ShipWeapon weapon in allWeapons)
        {
            // 무기가 점유하는 타일 추가 (구현 필요)
            Vector2Int pos = weapon.GetGridPosition();
            allTiles.Add(pos);
            allTiles.Add(new Vector2Int(pos.x + 1, pos.y));
        }

        Vector2 average = Vector2.zero;
        foreach (Vector2Int tile in allTiles)
            average += (Vector2)tile;

        average /= allTiles.Count;

        return targetShip.GetWorldPositionFromGrid(Vector2Int.RoundToInt(average));
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
