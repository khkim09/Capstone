using System.Collections.Generic;
using System.Linq;
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
    /// 저장 또는 유효성 검사 결과를 표시할 텍스트입니다.
    /// </summary>
    public TMP_Text feedbackText;

    /// <summary>
    /// 현재 보유 재화량
    /// </summary>
    public TMP_Text currency;

    /// <summary>
    /// 기존 함선 가격
    /// </summary>
    public TMP_Text originShipCost;

    /// <summary>
    /// 총 설계도 가격을 표시할 텍스트 UI.
    /// </summary>
    public TMP_Text totalCostText;

    /// <summary>
    /// 현재 커스터마이징 중인 플레이어 함선입니다.
    /// </summary>
    [Header("Ship")] public Ship playerShip;

    /// <summary>
    /// 그리드 타일 배치 작업을 위한 오브젝트
    /// </summary>
    public GridPlacer gridPlacer;

    /// <summary>
    /// 제작한 설계도
    /// </summary>
    public BlueprintShip targetBlueprintShip;

    /// <summary>
    /// 기존 사용 중인 함선 가격
    /// </summary>
    private int originalShipCost = 0;

    /// <summary>
    /// 유효성 검사 결과
    /// </summary>
    private ValidationResult validationResult;

    /// <summary>
    /// 설계도 함선 가격
    /// </summary>
    private int totalBPCost = 0;

    public List<BackupCrewData> backupCrewDatas = new();

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
            totalBPCost = targetBlueprintShip.GetTotalBPCost(); // 설계도 가격
            originalShipCost = playerShip.GetTotalShipValue(); // 사용중인 함선 가격
            int currentCurrency = (int)ResourceManager.Instance.GetResource(ResourceType.COMA); // 보유 재화량

            currency.text = $"Currency : {currentCurrency}";
            totalCostText.text = $"Blueprint Cost : {totalBPCost}";
            originShipCost.text = $"Origin Ship Cost : {originalShipCost}";

            // 1. 조건 : 자산
            bool hasEnoughMoney = totalBPCost - originalShipCost <= currentCurrency;

            // 2. 기존 함선 모든 방 내구도 100%
            bool shipFullyRepaired = playerShip.IsFullHitPoint();

            // tooltip에 띄울 피드백
            if (validationResult != null)
                feedbackText.text = $"{validationResult.Message}";
            else
                feedbackText.text = "";

            // 조건 체크
            if (hasEnoughMoney && shipFullyRepaired)
                buildButton.interactable = true;
            else
                buildButton.interactable = false;
        }

        // 설계도 작업 시 각 방 collider 임시 비활성화
        if (customizeUI.activeInHierarchy)
            SetPlayerShipCollidersActive(false);
        else
            SetPlayerShipCollidersActive(true);
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

        GetSavedWeaponRooms();


        // 카메라 - 설계도 함선 기준으로 세팅
        CenterCameraToBP();

        // 선원 임시 비활성화
        DisableCrews();

        playerShip.ClearExistingHulls();

        // 함선의 방 collider 비활성화 (RTS를 위한 collider와 겹침 방지)
        SetPlayerShipCollidersActive(false);
    }

    /// <summary>
    /// CustomizeShipUI 비활성화 시 아래 작업 수행 :
    /// 그리드 저장 오브젝트 비활성화
    /// 설계도 설치한 방 데이터 모두 저장 후 제거
    /// </summary>
    private void OnDisable()
    {
        if (gridTiles.activeInHierarchy)
            gridTiles.SetActive(false);

        // 설계도 설치한 방 데이터 모두 저장 후 제거
        SaveBPRoomsandDestroy();

        // 카메라 - 기존 함선 기준으로 복구
        ResetCameraToOriginShip();

        // 선원 활성화
        EnableCrews();

        // 함선 방 collider 활성화
        SetPlayerShipCollidersActive(true);

        playerShip.UpdateOuterHullVisuals();
    }

    /// <summary>
    /// 커스터마이징 UI 진입 시 선원 임시 비활성화
    /// </summary>
    private void DisableCrews()
    {
        List<CrewBase> crews = playerShip.GetAllCrew();

        foreach (CrewBase crew in crews)
            if (crew is CrewMember crewMember)
                crewMember.gameObject.SetActive(false);
    }

    /// <summary>
    /// 커스터마이징 UI 탈출 시 선원 재활성화
    /// </summary>
    private void EnableCrews()
    {
        List<CrewBase> crews = playerShip.GetAllCrew();

        foreach (CrewBase crew in crews)
            if (crew is CrewMember crewMember)
                crewMember.gameObject.SetActive(true);
    }

    /// <summary>
    /// 설계도 UI 호출 시 카메라 중앙값 보정
    /// </summary>
    private void CenterCameraToBP()
    {
        Vector3 startPos = gridPlacer.GetCameraStartPositionToBP();
        Camera.main.transform.position = new Vector3(startPos.x, startPos.y, Camera.main.transform.position.z);

        CameraZoomController cameraController = Camera.main.GetComponent<CameraZoomController>();
        Camera.main.orthographicSize = cameraController.lastZoomSize;
    }

    /// <summary>
    /// 카메라 기본 세팅으로 복구
    /// </summary>
    private void ResetCameraToOriginShip()
    {
        Vector3 startPos = gridPlacer.GetCameraStartPositionToOriginShip();
        Camera.main.transform.position = new Vector3(startPos.x, startPos.y, Camera.main.transform.position.z);
        Camera.main.orthographicSize = 5;
    }

    /// <summary>
    /// 기존에 작업중이던 설계도의 모든 방 데이터 호출 및 배치
    /// </summary>
    private void GetSavedBPRooms()
    {
        List<BlueprintRoomSaveData> layout = BlueprintLayoutSaver.LoadRoomLayout();

        foreach (BlueprintRoomSaveData saved in layout)
            gridPlacer.PlaceRoom(saved.bpRoomData, saved.bpLevelIndex, saved.bpPosition, saved.bpRotation);
    }

    private void GetSavedWeaponRooms()
    {
        List<BlueprintWeaponSaveData> layout = BlueprintLayoutSaver.LoadWeaponLayout();

        foreach (BlueprintWeaponSaveData saved in layout)
        {
            BlueprintWeapon bw = gridPlacer.PlaceWeapon(saved.bpWeaponData, saved.bpPosition, saved.bpDirection);
            bw.ApplyAttachedDirectionSprite();
        }
    }

    /// <summary>
    /// 작업중이던 설계도 저장 및 방 제거
    /// </summary>
    private void SaveBPRoomsandDestroy()
    {
        BlueprintRoom[] bpRooms = targetBlueprintShip.GetComponentsInChildren<BlueprintRoom>();
        BlueprintLayoutSaver.SaveRoomLayout(bpRooms);

        BlueprintWeapon[] bpWeapons = targetBlueprintShip.GetComponentsInChildren<BlueprintWeapon>();
        BlueprintLayoutSaver.SaveWeaponLayout(bpWeapons);

        // 설치했던 모든 설계도 방 제거
        foreach (BlueprintRoom r in bpRooms)
            Destroy(r.gameObject);

        foreach (BlueprintWeapon w in bpWeapons) Destroy(w.gameObject);

        targetBlueprintShip.ClearRooms();
    }

    /// <summary>
    /// 함선의 모든 방에 대해 collider 활성화/비활성화
    /// </summary>
    /// <param name="active"></param>
    private void SetPlayerShipCollidersActive(bool active)
    {
        if (playerShip == null)
            return;

        foreach (Room room in playerShip.GetAllRooms())
        {
            BoxCollider2D collider = room.GetComponent<BoxCollider2D>();
            if (collider != null)
                collider.enabled = active;
        }
    }

    /// <summary>
    /// 선원 데이터 백업 작업
    /// </summary>
    private void BackUpCrewDatas()
    {
        backupCrewDatas.Clear();

        foreach (CrewMember cm in playerShip.CrewSystem.GetCrews())
            backupCrewDatas.Add(new BackupCrewData
            {
                crew = cm, position = cm.GetCurrentTile(), currentRoom = cm.currentRoom
            });
    }

    /// <summary>
    /// '함선 제작' 버튼 클릭 시 호출.
    /// 조건 만족 시 기존 함선을 설계도의 함선으로 교체합니다.
    /// 이후 방, 문의 연결 유효성 검사 수행 후 만족 여부에 따라 실제 함선으로 교체, 다시 설계도로 복구를 수행
    /// </summary>
    public void OnClickBuild()
    {
        // 1. 기존 함선 가격 저장
        originalShipCost = playerShip.GetTotalShipValue();

        // 2. 기존 선원, 함선 백업
        BackUpCrewDatas();
        playerShip.BackupCurrentShip();
        playerShip.Initialize();
        // 3. 설계도 -> 실제 함선 (bpRoom -> Room) 변환
        playerShip.ReplaceShipFromBlueprint(targetBlueprintShip);

        // 4. 실제 Ship 상태 기준 유효성 검사 수행
        validationResult = new ShipValidationHelper().ValidateShipLayout(playerShip);

        // feedbackText.text = $"{validationResult.Message}";
        // 5. 유효성 검사
        if (!validationResult.IsValid)
        {
            Debug.Log("유효 X");

            // 실패
            feedbackText.text = $"X {validationResult.Message}";

            // 기존 함선으로 복원
            playerShip.RevertToOriginalShip();

            // 기존 선원 복원
            playerShip.CrewSystem.RevertOriginalCrews(backupCrewDatas);
        }
        else
        {
            Debug.Log("유효 O");

            // 성공 -> 함선 교체
            feedbackText.text = $"O Ship updated successfully\n{validationResult.Message}!";

            // 기존 함선 판매
            ResourceManager.Instance.ChangeResource(ResourceType.COMA, originalShipCost);

            // 설계도 함선 구매 (재화량 차감)
            ResourceManager.Instance.ChangeResource(ResourceType.COMA, -targetBlueprintShip.GetTotalBPCost());

            // RTS 이동을 위한 data 업데이트
            RTSSelectionManager.Instance.RefreshMovementData();

            // 1) 모든 방의 타일 - 선원 점유 상태 초기화
            Debug.LogError($"빌드 후 전체 방 수 : {playerShip.GetAllRooms().Count}");
            foreach (Room room in playerShip.GetAllRooms())
                room.ClearCrewOccupancy();

            // 2) 전역 타일 점유 상태 초기화 (ship 단위)
            playerShip.ClearAllCrewTileOccupancy();

            // 3) 기존 선원 복구 및 랜덤 배치
            playerShip.CrewSystem.RestoreCrewAfterBuild(backupCrewDatas);
        }

        // 함선 스텟 다시 계산
        playerShip.RecalculateAllStats();
    }

    /// <summary>
    /// 설계도 초기화 및 점유 타일 초기화
    /// </summary>
    public void OnClickClear()
    {
        targetBlueprintShip.ClearRooms();
        gridPlacer.occupiedGridTiles = new HashSet<Vector2Int>();
    }
}
