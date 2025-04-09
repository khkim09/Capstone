using System.Collections.Generic;
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
    public int bpLevelIndex;

    /// <summary>회전 각</summary>
    public int bpRotation;

    /// <summary>배치 위치</summary>
    public Vector2Int bpPosition;

    /// <summary>
    /// 설계도 함선
    /// </summary>
    private BlueprintShip blueprintShip;

    /// <summary>
    /// 드래그 중인지 여부
    /// </summary>
    private bool isDragging = false;

    /// <summary>해당 레벨의 설치 비용</summary>
    public int bpRoomCost;

    /// <summary>해당 레벨의 크기</summary>
    public Vector2Int bpRoomSize;

    /// <summary>
    /// 실제 점유 타일
    /// </summary>
    public List<Vector2Int> occupiedTiles = new();

    /// <summary>
    /// 설치 시 초기화
    /// </summary>
    public void Initialize(RoomData data, int level, Vector2Int pos, int rot)
    {
        roomData = data;
        bpLevelIndex = level;
        bpPosition = pos;
        bpRotation = rot;

        RoomData.RoomLevel levelData = data.GetRoomDataByLevel(level);
        bpRoomCost = levelData.cost;
        bpRoomSize = levelData.size;

        bpRoomSize = RoomRotationUtility.GetRotatedSize(bpRoomSize, bpRotation);
        occupiedTiles = RoomRotationUtility.GetOccupiedGridPositions(bpPosition, bpRoomSize, bpRotation);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = levelData.roomSprite;

        transform.rotation = Quaternion.Euler(0, 0, -bpRotation);
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
        bpPosition = newPos;
        transform.position = new Vector3(newPos.x, newPos.y, 0);
        blueprintShip.AddRoom(this);
    }
}
