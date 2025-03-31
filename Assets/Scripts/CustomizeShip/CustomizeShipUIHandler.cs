using UnityEngine;
using UnityEngine.UI;

public class CustomizeShipUIHandler : MonoBehaviour
{
    [Header("UI")]
    public GameObject mainUI;
    public GameObject customizeUI;
    public RoomsInventoryTooltipUI inventoryTooltipUI;

    [Header("Parent Object")]
    public GameObject gridTiles;
    public GameObject placedRooms;

    [Header("Buttons")]
    public Button saveButton;
    public Button cancelButton;
    public Text feedbackText;

    [Header("Ship")]
    public Ship playerShip; // 플레이어 함선
    public ShipCustomizationManager customizationManager;
    public ShipLayoutValidator layoutValidator;

    /// <summary>
    /// 저장/취소 버튼 이벤트 처리, 피드백 UI
    /// </summary>
    private void OnEnable()
    {
        gridTiles.SetActive(true);
        placedRooms.SetActive(true);

        customizationManager.ClearAllModules();
        LoadShipLayout(playerShip);
        inventoryTooltipUI.RefreshInventory(); // 구매한 방 list 갱신
    }

    private void OnDisable()
    {
        gridTiles.SetActive(false);
        placedRooms.SetActive(false);
        customizationManager.ClearAllModules();
    }

    public void OnSaveClicked()
    {
        // 함선 저장 로직 연결 예정
        if (!layoutValidator.ValidateLayout())
        {
            Debug.LogWarning("모든 방이 연결되어 있어야 저장 가능");
            return;
        }

        SaveShipLayout(playerShip);
        Debug.Log("Ship Data 저장됨.");

        inventoryTooltipUI.RefreshInventory();

        customizeUI.SetActive(false);
        mainUI.SetActive(true);
    }

    public void OnCancelClicked()
    {
        customizationManager.ClearAllModules();

        customizeUI.SetActive(false);
        mainUI.SetActive(true);
    }

    /// <summary>
    /// 기존 저장한 함선 호출
    /// </summary>
    /// <param name="ship"></param>
    private void LoadShipLayout(Ship ship)
    {
        foreach (Room room in ship.allRooms)
            customizationManager.PlaceSavedRoom(room);
        Debug.Log($"{ship.allRooms}");
    }

    /// <summary>
    /// 함선 저장
    /// </summary>
    /// <param name="ship"></param>
    private void SaveShipLayout(Ship ship)
    {
        ship.allRooms.Clear();

        foreach (var kvp in customizationManager.PlacedModules)
        {
            GameObject obj = kvp.Value;
            Room room = obj.GetComponent<Room>();
            if (room != null)
            {
                room.position = kvp.Key;
                ship.allRooms.Add(room);
                Debug.Log("방 추가");
            }
        }
    }
}
