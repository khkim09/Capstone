using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 함선 커스터마이징 UI를 제어하는 핸들러.
/// UI 전환, 모듈 배치, 저장 및 취소 기능을 관리합니다.
/// </summary>
public class CustomizeShipUIHandler : MonoBehaviour
{
    /// <summary>
    /// 메인 화면 UI입니다.
    /// </summary>
    [Header("UI")] public GameObject mainUI;

    /// <summary>
    /// 커스터마이즈 UI 화면입니다.
    /// </summary>
    public GameObject customizeUI;

    /// <summary>
    /// 보유한 방 목록 툴팁 UI입니다.
    /// </summary>
    public RoomsInventoryTooltipUI inventoryTooltipUI;

    /// <summary>
    /// 그리드 타일이 배치되는 부모 오브젝트입니다.
    /// </summary>
    [Header("Parent Object")] public GameObject gridTiles;

    /// <summary>
    /// 배치된 방을 담는 부모 오브젝트입니다.
    /// </summary>
    public GameObject placedRooms;

    /// <summary>
    /// 저장 버튼입니다.
    /// </summary>
    [Header("Buttons")] public Button saveButton;

    /// <summary>
    /// 취소 버튼입니다.
    /// </summary>
    public Button cancelButton;

    /// <summary>
    /// 저장 또는 유효성 검사 결과를 표시할 텍스트입니다.
    /// </summary>
    public Text feedbackText;

    /// <summary>
    /// 현재 커스터마이징 중인 플레이어 함선입니다.
    /// </summary>
    [Header("Ship")] public Ship playerShip; // 플레이어 함선

    /// <summary>
    /// 함선 커스터마이징 로직을 담당하는 매니저입니다.
    /// </summary>
    public ShipCustomizationManager customizationManager;

    /// <summary>
    /// 배치된 레이아웃의 유효성을 검사하는 유효성 검사기입니다.
    /// </summary>
    public ShipValidationHelper ValidatonHelper;

    /// <summary>
    /// CustomizeShipUI 활성화 시 아래 작업 수행 :
    /// 그리드 생성 및 그리드 저장 오브젝트 활성화
    /// 방 저장 오브젝트 활성화
    /// 기존에 있던 모든 모듈 제거
    /// 마지막 저장된 함선 정보 호출
    /// 소유한 방 (커스텀하기 위해 구매한 모든 방) 목록 최신화
    /// </summary>
    private void OnEnable()
    {
        gridTiles.SetActive(true);
        placedRooms.SetActive(true);

        customizationManager.ClearAllModules();
        LoadShipLayout(playerShip);
        inventoryTooltipUI.allOwnedRoomData = playerShip.GetInstalledRoomDataList(); // GetInstalledRoom()아님 -> 이건 이미 설치된 방임
        // Debug.Log($"가져온 방 개수 : {inventoryTooltipUI.allOwnedRoomData.Count}");
        inventoryTooltipUI.RefreshInventory(); // 구매한 방 list 갱신
    }

    /// <summary>
    /// CustomizeShipUI 비활성화 시 아래 작업 수행 :
    /// 그리드 저장 오브젝트, 방 저장 오브젝트 비활성화
    /// 모든 모듈 제거
    /// </summary>
    private void OnDisable()
    {
        gridTiles.SetActive(false);
        placedRooms.SetActive(false);
        customizationManager.ClearAllModules();
    }

    /// <summary>
    /// 함선 커스터마이징 마친 후 save 버튼 클릭 시 함선 저장장
    /// </summary>
    public void OnSaveClicked()
    {
        // TODO: ValidatonHelper 완성 후에 규격에 맞게 매개변수 넣어야함
        // 함선 저장 로직 연결 예정
        // if (!ValidatonHelper.ValidateLayout())
        // {
        //     Debug.LogWarning("모든 방이 연결되어 있어야 저장 가능");
        //     return;
        // }

        SaveShipLayout(playerShip);
        Debug.Log("Ship Data 저장됨.");

        inventoryTooltipUI.RefreshInventory();

        customizeUI.SetActive(false);
        mainUI.SetActive(true);
    }

    /// <summary>
    /// 함선 커스터마이징 도중 cancel 버튼 클릭 시 이전 mainUI 화면으로 복귀
    /// 이 때, 저장하지 않은 모든 작업은 사라집니다.
    /// </summary>
    public void OnCancelClicked()
    {
        customizationManager.ClearAllModules();

        customizeUI.SetActive(false);
        mainUI.SetActive(true);
    }

    /// <summary>
    /// 가장 최신 함선 정보 호출
    /// </summary>
    /// <param name="ship"></param>
    private void LoadShipLayout(Ship ship)
    {
        foreach (Room room in ship.allRooms)
            customizationManager.PlaceSavedRoom(room);
        Debug.Log($"{ship.allRooms}");
    }

    /// <summary>
    /// 함선 layout 저장
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
