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
    /// <summary>
    /// 인벤토리의 방 이미지
    /// </summary>
    [Header("UI")] public Image icon;

    /// <summary>
    /// 방 이름 라벨
    /// </summary>
    public TMP_Text label;

    /// <summary>
    /// 방 정보
    /// </summary>
    private RoomData roomData;

    /// <summary>
    /// 방 레벨
    /// </summary>
    private int level;

    /// <summary>
    /// 방 드래그를 관리하는 handler
    /// </summary>
    private BlueprintRoomDragHandler dragHandler;

    /// <summary>
    /// 드래그 중인지 여부
    /// </summary>
    private bool isDragging = false;

    /// <summary>
    /// 방 기본 크기 - 가로
    /// </summary>
    [Header("button size")] private float baseWidth = 130f;

    // 방 기본 크기 - 세로로
    private float baseHeight = 130f;

    /// <summary>
    /// 방 정보에 따른 방 초기화 작업
    /// </summary>
    /// <param name="data">방 정보</param>
    /// <param name="lvl">방 레벨</param>
    /// <param name="levelData">레벨에 따른 방 세부 데이터</param>
    /// <param name="levelsCount">레벨 개수</param>
    /// <param name="handler">드래그 핸들러</param>
    public void Initialize(RoomData data, int lvl, RoomData.RoomLevel levelData, int levelsCount,
        BlueprintRoomDragHandler handler)
    {
        roomData = data;
        level = lvl;
        dragHandler = handler;

        if (label != null)
        {
            if (levelsCount != 1)
                label.text = $"{levelData.roomName.Localize()}\n(Lv.{levelData.level})";
            else
                label.text = $"{levelData.roomName.Localize()}";
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
        if (rt != null) rt.sizeDelta = new Vector2(baseWidth * widthRatio, baseHeight * heightRatio);
    }

    /// <summary>
    /// 클릭 시 바로 드래그 시작 준비
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            isDragging = true;
    }

    /// <summary>
    /// 드래그 시작
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDragging || dragHandler == null)
            return;

        dragHandler.StartDragging(roomData, level);
    }

    /// <summary>
    /// 드래그 중 (RoomDragHandler가 업데이트에서 처리)
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        // RoomDragHandler가 업데이트에서 처리함
    }

    /// <summary>
    /// 드래그 종료
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }
}
