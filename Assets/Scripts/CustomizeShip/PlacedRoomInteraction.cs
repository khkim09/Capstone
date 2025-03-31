using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 설치된 방의 상호작용을 처리 (삭제, 이동, 수리툴팁)
/// </summary>
public class PlacedRoomInteraction : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RoomData roomData;
    public int rotation;

    /// <summary>
    /// 방 삭제, 수리 툴팁 생성
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 삭제
            Debug.Log("방 삭제");
            Destroy(gameObject);
            RoomsInventoryTooltipUI.Instance.AddRoom(roomData); // 인벤토리로 되돌리기
            RoomsInventoryTooltipUI.Instance.RefreshInventory(); // 인벤토리 갱신
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 수리 툴팁 띄우기 (임시)
            Debug.Log($"Repair Tooltip for {roomData.name}");
        }
    }

    /// <summary>
    /// 방 이동 시작
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // transform.localScale *= 1.1f;
        Debug.Log("방 이동 시작");
    }

    /// <summary>
    /// 방 이동 중중
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(eventData.position);
        pos.z = 16;
        transform.position = pos;
    }

    /// <summary>
    /// 방 이동 완료
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2Int gridPos = ShipGridManager.Instance.WorldToGridPosition(transform.position);
        transform.position = ShipGridManager.Instance.GridToWorldPosition(gridPos);
        transform.localScale = Vector3.one;
        Debug.Log("방 이동 완료");
    }
}
