using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 UI: RoomData에서 모든 RoomLevel을 불러와 UI로 나열.
/// </summary>
public class BlueprintInventoryUI : MonoBehaviour
{
    [Header("RoomData 직접 등록")]
    public List<RoomData> allRooms;

    [Header("UI Reference")]
    public Transform contentRoot; // ScrollView > Viewport > Content
    public GameObject roomButtonPrefab;
    public BlueprintRoomDragHandler dragHandler;

    private void Start()
    {
        LoadInventory();
    }

    /// <summary>
    ///
    /// </summary>
    private void LoadInventory()
    {
        foreach (RoomData data in allRooms)
        {
            List<RoomData.RoomLevel> levels = data.GetAllLevels();
            for (int i = 0; i < levels.Count; i++)
            {
                RoomData.RoomLevel levelData = levels[i];
                GameObject btnGO = Instantiate(roomButtonPrefab, contentRoot);
                var btn = btnGO.GetComponent<RoomInventoryButton>();
                btn.Initialize(data, i + 1, levelData, levels.Count, dragHandler); // 1 - based index
            }
        }
    }
}
