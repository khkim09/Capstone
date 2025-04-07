using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 각 룸 항목에 대한 UI 버튼.
/// </summary>
public class RoomInventoryButton : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image icon;
    public TMP_Text label;

    private RoomData roomData;
    private int level;
    private BlueprintRoomDragHandler dragHandler;

    private bool isDragging = false;

    public void Initialize(RoomData data, int lvl, RoomData.RoomLevel levelData, int levelsCount, BlueprintRoomDragHandler handler)
    {
        roomData = data;
        level = lvl;
        dragHandler = handler;

        if (label != null)
        {
            if (levelsCount != 1)
                label.text = $"{levelData.roomName}\n(Lv.{levelData.level})";
            else
                label.text = $"{levelData.roomName}";
        }

        if (icon != null)
            icon.sprite = levelData.roomSprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 클릭 시 바로 드래그 시작 준비
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isDragging = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDragging || dragHandler == null)
            return;

        dragHandler.StartDragging(roomData, level);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // RoomDragHandler가 업데이트에서 처리함
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }
}
