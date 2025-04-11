using System.Collections.Generic;
using TMPro;
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
    /// 그리드 타일이 배치되는 부모 오브젝트입니다.
    /// </summary>
    [Header("Parent Object")] public GameObject gridTiles;

    /// <summary>
    /// 설계도 -> 실제 함선으로 교체 (bpRoom -> room)
    /// </summary>
    public Button buildButton;

    /// <summary>
    /// 저장 버튼입니다.
    /// </summary>
    [Header("Buttons")]
    public Button saveButton;

    /// <summary>
    /// 취소 버튼입니다.
    /// </summary>
    public Button cancelButton;

    /// <summary>
    /// 저장 또는 유효성 검사 결과를 표시할 텍스트입니다.
    /// </summary>
    public Text feedbackText;

    /// <summary>
    /// 총 설계도 가격을 표시할 텍스트 UI.
    /// </summary>
    public TMP_Text totalCostText;

    /// <summary>
    /// 현재 커스터마이징 중인 플레이어 함선입니다.
    /// </summary>
    [Header("Ship")]
    public Ship playerShip;

    /// <summary>
    /// 그리드 타일 배치 작업을 위한 오브젝트
    /// </summary>
    public GridPlacer gridPlacer;

    /// <summary>
    /// 제작한 설계도
    /// </summary>
    public BlueprintShip targetBlueprintShip;

    /// <summary>
    /// 배치된 레이아웃의 유효성을 검사하는 유효성 검사기입니다.
    /// </summary>
    public ShipValidationHelper ValidatonHelper;

    /// <summary>
    /// 현재 설계도의 가격 지속 갱신
    /// 설계도로 함선 교체 가능 조건
    /// 1. (설계도 가격 - 기존 함선 가격) <= 보유 재화량
    /// 2. 기존 함선 모든 방 내구도 100%
    /// </summary>
    private void Update()
    {
        if (targetBlueprintShip != null && playerShip != null)
        {
            int totalBPCost = targetBlueprintShip.totalBlueprintCost; // 설계도 가격
            int currentShipCost = playerShip.GetTotalShipValue();// 기존 함선 가격
            int currentCurrency = (int)ResourceManager.Instance.GetResource(ResourceType.COMA); // 보유 재화량

            totalCostText.text = $"Blueprint Cost: {totalBPCost}";

            // 조건 체크
            if (totalBPCost - currentShipCost <= currentCurrency && playerShip.IsFullHitPoint())
                buildButton.interactable = true;
            else
                buildButton.interactable = false;
        }
    }

    /// <summary>
    /// CustomizeShipUI 활성화 시 아래 작업 수행 :
    /// 그리드 생성 및 그리드 저장 오브젝트 활성화
    /// 작업하던 설계도 방 호출, 배치
    /// 카메라 중앙값 세팅
    /// </summary>
    private void OnEnable()
    {
        gridTiles.SetActive(true);

        // 설계도 방 호출, 배치
        GetSavedBPRooms();

        // 카메라 세팅
        CenterCamera();
    }

    /// <summary>
    /// CustomizeShipUI 비활성화 시 아래 작업 수행 :
    /// 그리드 저장 오브젝트 비활성화
    /// 설계도 설치한 방 데이터 모두 저장 후 제거
    /// </summary>
    private void OnDisable()
    {
        gridTiles.SetActive(false);

        // 설계도 설치한 방 데이터 모두 저장 후 제거
        SaveBPRoomsandDestroy();
    }

    /// <summary>
    /// 설계도도 UI 호출 시 카메라 중앙값 보정
    /// </summary>
    private void CenterCamera()
    {
        Vector3 startPos = gridPlacer.GetCameraStartPosition();
        Camera.main.transform.position = new Vector3(startPos.x, startPos.y, Camera.main.transform.position.z);

        CameraZoomController cameraDrag = Camera.main.GetComponent<CameraZoomController>();
        if (cameraDrag != null)
            cameraDrag.StartPanFrom(Input.mousePosition); // 현재 마우스 위치 기준으로 초기화
    }

    /// <summary>
    /// 기존에 작업중이던 설계도의 모든 방 데이터 호출 및 배치
    /// </summary>
    private void GetSavedBPRooms()
    {
        List<BlueprintRoomSaveData> layout = BlueprintLayoutSaver.LoadLayout();

        foreach (BlueprintRoomSaveData saved in layout)
            gridPlacer.PlaceRoom(saved.bpRoomData, saved.bpLevelIndex, saved.bpPosition, saved.bpRotation);
    }

    /// <summary>
    /// 작업중이던 설계도 저장 및 방 제거
    /// </summary>
    private void SaveBPRoomsandDestroy()
    {
        BlueprintRoom[] bpRooms = targetBlueprintShip.GetComponentsInChildren<BlueprintRoom>();
        BlueprintLayoutSaver.SaveLayout(bpRooms);

        // 설치했던 모든 설계도 방 제거
        foreach (BlueprintRoom r in bpRooms)
            Destroy(r.gameObject);
    }

    /// <summary>
    /// '함선 제작' 버튼 클릭 시 호출.
    /// 조건 만족 시 기존 함선을 설계도의 함선으로 교체합니다.
    /// </summary>
    public void OnClickBuild()
    {
        playerShip.ReplaceShipWithBlueprint(targetBlueprintShip);
    }
}
