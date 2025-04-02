using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 설치된 방에 대한 상호작용을 처리하는 클래스.
/// 클릭으로 삭제하거나, 드래그하여 이동할 수 있으며, 수리 툴팁도 처리합니다.
/// </summary>
public class PlacedRoomInteraction : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// 방의 데이터 정보입니다.
    /// 삭제 후 인벤토리에 되돌릴 때 사용됩니다.
    /// </summary>
    public RoomData roomData;

    /// <summary>
    /// 현재 회전 상태입니다.
    /// </summary>
    public int rotation;

    /// <summary>
    /// 클릭 시 방 삭제 또는 수리 툴팁 표시를 처리합니다.
    /// 좌클릭 시 삭제, 우클릭 시 수리 툴팁 출력 (임시).
    /// </summary>
    /// <param name="eventData">클릭 이벤트 데이터.</param>
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
    /// 드래그 시작 시 호출됩니다.
    /// 방 이동을 위한 초기 동작을 수행합니다.
    /// </summary>
    /// <param name="eventData">드래그 이벤트 데이터.</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // transform.localScale *= 1.1f;
        Debug.Log("방 이동 시작");
    }

    /// <summary>
    /// 드래그 중 방의 위치를 마우스에 따라 실시간으로 이동시킵니다.
    /// </summary>
    /// <param name="eventData">드래그 이벤트 데이터.</param>
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(eventData.position);
        pos.z = 16;
        transform.position = pos;
    }

    /// <summary>
    /// 드래그 종료 시, 방을 그리드에 스냅하여 배치합니다.
    /// </summary>
    /// <param name="eventData">드래그 이벤트 데이터.</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2Int gridPos = ShipGridManager.Instance.WorldToGridPosition(transform.position);
        transform.position = ShipGridManager.Instance.GridToWorldPosition(gridPos);
        transform.localScale = Vector3.one;
        Debug.Log("방 이동 완료");
    }
}
