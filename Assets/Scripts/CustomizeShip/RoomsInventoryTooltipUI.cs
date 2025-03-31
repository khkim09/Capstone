using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RoomInventoryTooltip 의 Scroll View 관리하며,
/// Ship.cs의 allRooms에 저장된 보유 방들을 기반으로 UI를 동적으로 생성하고 Content 크기를 조정합니다.
/// </summary>
public class RoomsInventoryTooltipUI : MonoBehaviour
{
    public static RoomsInventoryTooltipUI Instance;

    [Header("Inventory References")]
    public GameObject roomButtonPrefab;         // 드래그 가능한 룸 UI 프리팹
    public Transform contentRoot;               // ScrollView > Viewport > Content
    public Ship playerShip;                     // 현재 플레이어가 작업 중인 Ship

    private const float unitSize = 40f;         // 타일 1칸에 해당하는 버튼 높이(px)
    private const float verticalSpacing = 20f;  // VerticalLayoutGroup의 spacing

    /// <summary>
    /// 인벤토리를 갱신하여 현재 보유한 방들을 UI로 표시합니다.
    /// 방 설치, 해제 완료 시 갱신 추가 필요 (구현 필요)
    /// </summary>
    public void RefreshInventory()
    {
        if (playerShip == null || playerShip.allRooms == null)
        {
            Debug.LogWarning("Ship or room list is null");
            return;
        }

        // 기존 항목 제거
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        float totalHeight = 0f;

        foreach (Room room in playerShip.allRooms)
        {
            Debug.Log($"{room.roomType}");
            RoomData data = room.roomData;
            var roomLevelData = data.GetRoomData(room.GetCurrentLevel());
            if (roomLevelData == null)
                continue;

            GameObject btn = Instantiate(roomButtonPrefab, contentRoot); // button prefab이라 draghandler가 붙질 않는듯

            // 크기 조정
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(roomLevelData.size.x * unitSize, roomLevelData.size.y * unitSize); // room 크기 만큼

            // 이미지 설정
            Image img = btn.GetComponent<Image>();
            img.sprite = roomLevelData.roomSprite;

            // 드래그 설정
            RoomDragHandler dragHandler = btn.GetComponent<RoomDragHandler>();
            dragHandler.InitializeFromRoomData(data, room.GetCurrentLevel());

            totalHeight += rt.sizeDelta.y + verticalSpacing;
        }

        // Content의 최소 높이 보장
        float minHeight = 500f;
        float finalHeight = Mathf.Max(minHeight, totalHeight);
        RectTransform contentRT = contentRoot.GetComponent<RectTransform>();
        contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, finalHeight);

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
    }

    /*
        public void AddRoom(RoomData data, int level)
        {
            GameObject btn = Instantiate(roomButtonPrefab, contentRoot);
            RoomDragHandler dragHandler = btn.GetComponent<RoomDragHandler>();
            dragHandler.InitializeFromRoomData(data, level);

            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(dragHandler.roomSize.x * unitSize, dragHandler.roomSize.y * unitSize);
            Image img = btn.GetComponent<Image>();
            img.sprite = dragHandler.roomSprite;

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
        }
    */
}
