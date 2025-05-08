using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 UI: RoomData에서 모든 RoomLevel을 불러와 UI로 나열.
/// </summary>
public class BlueprintInventoryUI : MonoBehaviour
{
    /// <summary>
    /// 모든 방 종류
    /// </summary>
    [Header("RoomData 직접 등록")] public List<RoomData> allRooms;

    [Header("UI Reference")]
    /// <summary>
    /// 모든 방 콘텐츠 넣을 공간
    /// </summary>
    public Transform contentRoot; // ScrollView > Viewport > Content

    /// <summary>
    /// 설치를 위한 방 버튼 prefab
    /// </summary>
    public GameObject roomButtonPrefab;

    /// <summary>
    /// 방 드래그 관리 handler
    /// </summary>
    public BlueprintRoomDragHandler dragHandler;

    /// <summary>
    /// 카테고리 분류 handler
    /// </summary>
    public BlueprintCategoryButtonHandler categoryButtonHandler;

    /// <summary>
    /// 방 UI에 대해 항상 Essential 카테고리로 초기화
    /// </summary>
    private void Start()
    {
        FilterByCategory(RoomCategory.Essential);
        categoryButtonHandler.SetButtonState(RoomCategory.Essential);
        gameObject.AddComponent<SpriteRenderer>();
        //GetComponent<SpriteRenderer>().sortingOrder = SortingOrderConstants.UI;
    }

    /// <summary>
    /// 카테고리 별 분류 후 인벤토리에 방 호출
    /// </summary>
    /// <param name="category"></param>
    public void FilterByCategory(RoomCategory category)
    {
        // 기존 방 버튼 모두 제거
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        // 카테고리에 맞는 RoomLevel만 필터링해서 버튼 생성
        foreach (RoomData data in allRooms)
        {
            List<RoomData.RoomLevel> levels = data.GetAllLevels();

            for (int i = 0; i < levels.Count; i++)
            {
                RoomData.RoomLevel levelData = levels[i];
                if (levelData.category != category)
                    continue;

                GameObject btnGO = Instantiate(roomButtonPrefab, contentRoot);
                RoomInventoryButton btn = btnGO.GetComponent<RoomInventoryButton>();
                btn.Initialize(data, i + 1, levelData, levels.Count, dragHandler);
            }
        }
    }
}
