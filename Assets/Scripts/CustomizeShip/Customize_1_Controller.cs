using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 함선 커스터마이징 UI를 제어하는 핸들러.
/// UI 전환, 모듈 배치, 저장 및 취소 기능을 관리합니다.
/// </summary>
public class Customize_1_Controller : MonoBehaviour
{
    [Header("Customize UI Panel")]
    public GameObject customize0Panel;
    public GameObject customize1Panel;
    public GameObject customize2Panel;
    [SerializeField] BlueprintCategoryButtonHandler bcbHandler;

    /// <summary>
    /// 그리드 타일이 배치되는 부모 오브젝트입니다.
    /// </summary>
    [Header("배치에 사용될 필드 값")]
    public GameObject gridTiles;

    /// <summary>
    /// 총 설계도 가격을 표시할 텍스트 UI.
    /// </summary>
    public TMP_Text bpCostText;

    /// <summary>
    /// 저장 또는 유효성 검사 결과를 표시할 텍스트입니다.
    /// </summary>
    public TMP_Text feedbackText;

    /// <summary>
    /// 그리드 타일 배치 작업을 위한 오브젝트
    /// </summary>
    public GridPlacer gridPlacer;

    /// <summary>
    /// 유저 함선
    /// </summary>
    public Ship playerShip;

    /// <summary>
    /// 제작한 설계도
    /// </summary>
    public BlueprintShip targetBlueprintShip;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button saveButton;

    [SerializeField] private Button essentialButton;
    [SerializeField] private Button auxiliaryButton;
    [SerializeField] private Button livingButton;
    [SerializeField] private Button storageButton;
    [SerializeField] private Button corridorButton;
    [SerializeField] private Button weaponButton;

    [Header("유효성 검사 결과")]
    /// <summary>
    /// 유효성 검사 결과
    /// </summary>
    private ValidationResult validationResult;

    /// <summary>
    /// 설계도 함선 가격
    /// </summary>
    private int totalBPCost = 0;

    [Header("Save Button 색 처리")]
    /// <summary>
    /// build 빨간색 처리 코루틴
    /// </summary>
    private Coroutine flashCoroutine;

    /// <summary>
    /// 버튼 기본 색상 저장
    /// </summary>
    private Color? originButtonColor = null;

    [SerializeField] BlueprintInventoryUI blueprintInventoryUI;

    /// <summary>
    /// button 클릭 함수 연결
    /// </summary>
    private void Start()
    {
        backButton.onClick.AddListener(() => { OnClickBack(); });
        clearButton.onClick.AddListener(() => { OnClickClear(); });
        saveButton.onClick.AddListener(() => { OnClickSave(); });

        essentialButton.onClick.AddListener(() => { bcbHandler.ShowEssential(); });
        auxiliaryButton.onClick.AddListener(() => { bcbHandler.ShowAuxiliary(); });
        livingButton.onClick.AddListener(() => { bcbHandler.ShowLiving(); });
        storageButton.onClick.AddListener(() => { bcbHandler.ShowStorage(); });
        corridorButton.onClick.AddListener(() => { bcbHandler.ShowEtc(); });
        weaponButton.onClick.AddListener(() => { bcbHandler.ShowWeapons(); });
    }

    /// <summary>
    /// 현재 설계도의 가격 지속 갱신
    /// 설계도로 함선 교체 가능 조건
    /// 1. (설계도 가격 - 기존 함선 가격) <= 보유 재화량
    /// 2. 기존 함선 모든 방 내구도 100%
    /// </summary>
    private void Update()
    {
        if (targetBlueprintShip != null && GameManager.Instance.playerShip != null)
        {
            totalBPCost = targetBlueprintShip.GetTotalBPCost(); // 설계도 가격

            bpCostText.text = $"Blueprint Cost : {totalBPCost}";

            // tooltip에 띄울 피드백
            if (validationResult != null)
                feedbackText.text = $"{validationResult.Message}";
            else
                feedbackText.text = "";
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
        playerShip.SetShipContentsActive(false);

        ReloadBlueprintFromSlot(BlueprintSlotManager.Instance.currentSlotIndex);

        // 항상 essential로 초기화
        blueprintInventoryUI.FilterByCategory(RoomCategory.Essential);
        blueprintInventoryUI.categoryButtonHandler.SetButtonState(RoomCategory.Essential);

        // 카메라 - 설계도 함선 기준으로 세팅
        CenterCameraToBP();

        // 외갑판 임시 비활성화
        GameManager.Instance.playerShip.ClearExistingHulls();

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

        // 카메라 - 기존 함선 기준으로 복구
        ResetCameraToOriginShip();

        // 외갑판 활성화
        GameManager.Instance.playerShip.UpdateOuterHullVisuals();

        // 함선 방 collider 활성화
        SetPlayerShipCollidersActive(true);
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
    /// 클릭한 슬롯 도안 호출
    /// </summary>
    /// <param name="slotIndex"></param>
    public void ReloadBlueprintFromSlot(int slotIndex)
    {
        gridTiles.SetActive(true);
        targetBlueprintShip.ClearRooms();

        BlueprintSaveData data = BlueprintSlotManager.Instance.GetBlueprintAt(slotIndex);

        if (data != null && (data.rooms.Count > 0 || data.weapons.Count > 0))
        {
            foreach (BlueprintRoomSaveData room in data.rooms)
                gridPlacer.PlaceRoom(room.bpRoomData, room.bpLevelIndex, room.bpPosition, room.bpRotation);

            foreach (BlueprintWeaponSaveData weapon in data.weapons)
            {
                BlueprintWeapon w = gridPlacer.PlaceWeapon(weapon.bpWeaponData, weapon.bpPosition, weapon.bpDirection);
                w.SetHullLevel(data.hullLevel);
            }
        }

        gridPlacer.occupiedGridTiles = BlueprintSlotManager.Instance.GetOccupiedTiles(slotIndex);

        CenterCameraToBP();
        SetPlayerShipCollidersActive(false);
    }

    /// <summary>
    /// 함선의 모든 방에 대해 collider 활성화/비활성화
    /// </summary>
    /// <param name="active"></param>
    private void SetPlayerShipCollidersActive(bool active)
    {
        if (GameManager.Instance.playerShip == null)
            return;

        foreach (Room room in GameManager.Instance.playerShip.GetAllRooms())
        {
            BoxCollider2D collider = room.GetComponent<BoxCollider2D>();
            if (collider != null)
                collider.enabled = active;
        }
    }

    #region 함선 제작 버튼 (유효성 검사 및 교체)

    /// <summary>
    /// 도안 저장 버튼 클릭 - 유효성 검사
    /// </summary>
    public void OnClickSave()
    {
        Ship playerShip = GameManager.Instance.playerShip;

        // 1. 편집 중이던 도안 slot에 저장
        BlueprintRoom[] bpRooms = targetBlueprintShip.GetComponentsInChildren<BlueprintRoom>();
        // BlueprintLayoutSaver.SaveRoomLayout(bpRooms);

        BlueprintWeapon[] bpWeapons = targetBlueprintShip.GetComponentsInChildren<BlueprintWeapon>();
        // BlueprintLayoutSaver.SaveWeaponLayout(bpWeapons);

        Customize_2_Controller controller2 = customize2Panel.GetComponent<Customize_2_Controller>();
        BlueprintLayoutSaver.SaveLayout(bpRooms, bpWeapons, controller2.selectedHullLevel);

        List<BlueprintRoomSaveData> savedRooms = BlueprintLayoutSaver.LoadRoomLayout();
        List<BlueprintWeaponSaveData> savedWeapons = BlueprintLayoutSaver.LoadWeaponLayout();
        int hullLvl = controller2.selectedHullLevel;
        BlueprintSaveData newData = new(savedRooms, savedWeapons, hullLvl);

        // 2. 슬롯에 저장
        BlueprintSlotManager.Instance.SaveBlueprintToCurrentSlot(newData);
        BlueprintSlotManager.Instance.SaveOccupiedTilesToCurrentSlot(gridPlacer.occupiedGridTiles);

        // 3. 기존 선원, 함선 백업
        playerShip.BackupAllCrews();
        playerShip.BackupCurrentShip();

        // 4. 설계도 -> 실제 함선으로 변환 (유효성 검사 위한 단계)
        playerShip.ReplaceShipFromBlueprint(targetBlueprintShip);

        // 5. 유효성 검사
        validationResult = new ShipValidationHelper().ValidateShipLayout(playerShip);

        // 6. 유효성 검사 결과
        if (!validationResult.IsValid)
        {
            feedbackText.text = $"X {validationResult.Message}";

            // build 버튼 일시적 빨간색 처리
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashSaveButtonColor(new Color(1, 0, 0, 0.5f), 2f));
        }

        // 7. 검사 결과에 따라 slot 버튼 업데이트
        Customize_0_Controller controller0 = customize0Panel.GetComponent<Customize_0_Controller>();
        controller0.UpdateSlotButtonColor(BlueprintSlotManager.Instance.currentSlotIndex, validationResult.IsValid);

        BlueprintSlotManager.Instance.isValidBP[BlueprintSlotManager.Instance.currentSlotIndex] = validationResult.IsValid;

        // 8. 도안은 도안이므로 기존 함선으로 복구 (함선, 선원)
        playerShip.RevertToOriginalShip();
        playerShip.CrewSystem.RevertOriginalCrews(playerShip.backupCrewDatas);
    }

    #endregion

    /// <summary>
    /// customize_0_panel로 복귀
    /// </summary>
    public void OnClickBack()
    {
        // blueprintship 초기화
        BlueprintRoom[] bpRooms = targetBlueprintShip.GetComponentsInChildren<BlueprintRoom>();
        BlueprintWeapon[] bpWeapons = targetBlueprintShip.GetComponentsInChildren<BlueprintWeapon>();

        foreach (BlueprintRoom r in bpRooms)
            Destroy(r.gameObject);

        foreach (BlueprintWeapon w in bpWeapons)
            Destroy(w.gameObject);

        targetBlueprintShip.ClearRooms();

        // flashCoroutine 작동 정지
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // save 버튼 색상 복구
        Image buttonImage = saveButton.GetComponent<Image>();
        if (buttonImage != null && originButtonColor.HasValue)
            buttonImage.color = originButtonColor.Value;

        validationResult = null;
        feedbackText.text = "";

        customize1Panel.SetActive(false);
        customize0Panel.SetActive(true);

        // 도안 최신화
        Customize_0_Controller controller0 = customize0Panel.GetComponent<Customize_0_Controller>();
        controller0.OnClickSlot(BlueprintSlotManager.Instance.currentSlotIndex);
    }

    #region Control Tab (clear, save)

    /// <summary>
    /// 설계도 초기화 및 점유 타일 초기화
    /// </summary>
    public void OnClickClear()
    {
        targetBlueprintShip.ClearRooms();
        gridPlacer.occupiedGridTiles = new HashSet<Vector2Int>();
        validationResult = null;
        feedbackText.text = "";
    }

    #endregion

    #region 기타

    /// <summary>
    /// 유효성 검사 실패 시 build button 일시적 빨간색
    /// </summary>
    /// <param name="flashColor"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator FlashSaveButtonColor(Color flashColor, float duration)
    {
        Image buttonImage = saveButton.GetComponent<Image>();
        if (buttonImage == null)
            yield break;

        if (originButtonColor == null)
            originButtonColor = buttonImage.color;

        buttonImage.color = flashColor;

        yield return new WaitForSeconds(duration);

        if (originButtonColor.HasValue)
            buttonImage.color = originButtonColor.Value;

        flashCoroutine = null;
    }

    #endregion
}
