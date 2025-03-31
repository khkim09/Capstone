// 기존 코드 - 정상 작동 (버튼 드래그 앤 드랍)
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 방 설치, 회전, 삭제, 이동까지 담당하는 드래그 핸들러
/// </summary>
public class RoomDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static RoomDragHandler Instance;

    [Header("Room Info")]
    public RoomData roomData;
    public GameObject roomPrefab;
    public GameObject previewPrefab;

    [HideInInspector] public Vector2Int roomSize;
    [HideInInspector] public int rotation;
    [HideInInspector] public int roomLevel;

    [Header("Preview Settings")]
    public SpriteRenderer previewSR;
    private GameObject previewInstance;
    private bool isDragging = false;

    private Transform placedRoomParent;

    private void Start()
    {
        GameObject found = GameObject.Find("PlacedRooms");
        if (found != null)
            placedRoomParent = found.transform;
    }

    /// <summary>
    /// 드래그 중 우클릭 시 마다, 설치할 방의 실루엣을 시계 방향으로 90도 회전합니다.
    /// </summary>
    private void Update()
    {
        if (!isDragging || previewInstance == null)
            return;

        if (Input.GetMouseButtonDown(1)) // 우클릭 때마다 시계 방향으로 90도 회전
        {
            Debug.Log("회전 90도");
            rotation = (rotation + 90) % 360;
            previewInstance.transform.Rotate(0, 0, -90);
        }
    }

    /// <summary>
    /// 호출해 온 방 정보를 이용하여 방 설치에 필요한 정보들을 초기화합니다.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="level"></param>
    public void InitializeFromRoomData(RoomData data, int level)
    {
        roomData = data;
        roomLevel = level;

        var levelData = data.GetRoomData(level);
        if (levelData == null)
        {
            Debug.LogError("Invalid RoomData or Level");
            return;
        }

        roomSize = levelData.size;
        rotation = levelData.rotation;
        // roomPrefab = levelData.roomPrefab;
        // previewPrefab = levelData.previewPrefab;
    }

    /// <summary>
    /// 드래그를 시작할 때의 작업입니다.
    /// preview (실루엣)를 생성하고 alpha값을 0.5로 조정합니다. (SetAlpha() 호출)
    /// UI와 겹치지 않기 위해 z값을 조정합니다. (MoveToFront() 호출)
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        previewInstance = Instantiate(previewPrefab);
        previewSR = previewInstance.GetComponentInChildren<SpriteRenderer>();
        SetAlpha(previewInstance, 0.5f);
        MoveToFront(previewInstance);
    }

    /// <summary>
    /// 드래그 하는 동안의 작업입니다.
    /// 항상 n x n 사이즈 블록의 가장 좌측 하단의 블록을 기준점으로 설치하도록 합니다.
    /// 설치 가능 여부에 따라 (canPlace 값) preview (실루엣)의 색상 변동을 구현합니다.
    /// 드래그 하는 동안 우클릭 시, 시계 방향으로 90도 씩 회전합니다.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2Int gridPos = ShipGridManager.Instance.WorldToGridPosition(worldPos);

        bool canPlace = GridPlacer.Instance.CanPlaceRoom(gridPos, roomSize);
        Color targetColor = canPlace ? Color.green : Color.red;
        targetColor.a = 0.5f;
        previewSR.color = targetColor;

        Vector3 basePos = ShipGridManager.Instance.GridToWorldPosition(gridPos);

        // 좌측 하단 블록 중심으로 세팅
        float dx = roomSize.x / 2.0f - 0.5f;
        float dy = roomSize.y / 2.0f - 0.5f;
        previewInstance.transform.position = basePos + new Vector3(dx, dy, 0);

        /*
                if (Input.GetMouseButtonDown(1)) // 우클릭 90도 회전
                {
                    Debug.Log("회전 90도");
                    rotation = (rotation + 90) % 360; // 시계 방향으로 90도씩 돌리면 rotation + 90 이 맞는지 (왜냐면 방 정보 저장할 때 rotation값도 저장해줘야 함선 호출 시 제대로 배치 가능)
                    previewInstance.transform.Rotate(0, 0, -90);
                }
        */
    }

    /// <summary>
    /// 드래그 종료 후 작업입니다.
    /// 현재 마우스 위치에 방을 설치하고, preview (실루엣)을 삭제합니다.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2Int gridPos = ShipGridManager.Instance.WorldToGridPosition(worldPos);

        GameObject placedRoom = GridPlacer.Instance.PlaceRoom(gridPos, roomSize, roomPrefab, rotation);
        if (placedRoom != null)
        {
            var roomComp = placedRoom.GetComponent<PlacedRoomInteraction>();
            roomComp.roomData = roomData;
            roomComp.rotation = rotation;

            var sr = placedRoom.GetComponent<SpriteRenderer>();
            sr.sprite = roomData.GetRoomData(roomLevel).roomSprite;

            RoomsInventoryTooltipUI.Instance.RefreshInventory(); // 설치 후 인벤토리 갱신
        }
        Destroy(previewInstance);
    }

    /// <summary>
    /// alpha 값 조정
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="alpha"></param>
    private void SetAlpha(GameObject obj, float alpha)
    {
        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    /// <summary>
    /// UI와 겹치지 않기 위해 z값 수정
    /// </summary>
    /// <param name="obj"></param>
    private void MoveToFront(GameObject obj)
    {
        obj.transform.position += new Vector3(0, 0, -0.5f);
    }
}
