using System;
using TMPro;
using Unity.VisualScripting;
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

    [Header("button size")]
    public float baseWidth = 130f;
    public float baseHeight = 130f;

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

        // 인벤토리 버튼 너비 비율 조정
        Vector2Int roomSize = levelData.size;
        float maxRoomSize = 4f;
        float widthRatio = Mathf.Clamp(roomSize.x / maxRoomSize, 0.5f, 1.0f);
        float heightRatio = Mathf.Clamp(roomSize.y / maxRoomSize, 0.5f, 1.0f);

        // 정규화 보정
        switch (roomSize.x)
        {
            case 1:
                widthRatio = 0.5f;
                break;
            case 2:
                widthRatio = 0.666f;
                break;
            case 3:
                widthRatio = 0.833f;
                break;
            case 4:
                widthRatio = 1.0f;
                break;
        }

        switch (roomSize.y)
        {
            case 1:
                heightRatio = 0.5f;
                break;
            case 2:
                heightRatio = 0.666f;
                break;
            case 3:
                heightRatio = 0.833f;
                break;
            case 4:
                heightRatio = 1.0f;
                break;
        }

        // 버튼 크기 조정
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(baseWidth * widthRatio, baseHeight * heightRatio);
        }
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
