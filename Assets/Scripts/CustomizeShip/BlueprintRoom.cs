using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// 설계도에 배치된 방 정보.
/// </summary>
public class BlueprintRoom : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>RoomData 참조</summary>
    public RoomData roomData;

    /// <summary>선택된 레벨 인덱스 (0~2)</summary>
    public int levelIndex;

    /// <summary>배치 위치</summary>
    public Vector2Int position;

    /// <summary>
    /// 회전 각
    /// </summary>
    public int rotation;

    /// <summary>
    /// 설계도 함선
    /// </summary>
    private BlueprintShip blueprintShip;

    /// <summary>
    /// 드래그 중인지 여부
    /// </summary>
    private bool isDragging = false;

    /// <summary>레벨별 데이터 접근자</summary>
    public RoomData.RoomLevel levelData => roomData.GetRoomData(levelIndex);

    /// <summary>해당 레벨의 설치 비용</summary>
    public int roomCost => levelData.cost;

    /// <summary>해당 레벨의 크기</summary>
    public Vector2Int roomSize => levelData.size;

    /// <summary>
    /// 설치 시 초기화
    /// </summary>
    public void Initialize(RoomData data, int level, Vector2Int pos, int rot)
    {
        roomData = data;
        levelIndex = level;
        position = pos;
        rotation = rot;

        RoomData.RoomLevel levelData = data.GetRoomData(level);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = levelData.roomSprite;

        // transform.position = new Vector3(pos.x, pos.y, 0);
        transform.rotation = Quaternion.Euler(0, 0, -rotation);
    }

    public void SetBlueprint(BlueprintShip bpShip)
    {
        blueprintShip = bpShip;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            blueprintShip.RemoveRoom(this);
            Destroy(gameObject);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(eventData.position);
        transform.position = mouseWorld;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2Int newPos = new(Mathf.FloorToInt(mouseWorld.x), Mathf.FloorToInt(mouseWorld.y));

        blueprintShip.RemoveRoom(this);
        position = newPos;
        transform.position = new Vector3(newPos.x, newPos.y, 0);
        blueprintShip.AddRoom(this);
    }
}
