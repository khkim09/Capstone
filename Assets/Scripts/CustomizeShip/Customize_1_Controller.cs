using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
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
        gridTiles.SetActive(true);

        BlueprintSaveData data = BlueprintSlotManager.Instance.GetBlueprintAt(BlueprintSlotManager.Instance.currentSlotIndex);

        if (data != null)
        {
            foreach (BlueprintRoomSaveData room in data.rooms)
                gridPlacer.PlaceRoom(room.bpRoomData, room.bpLevelIndex, room.bpPosition, room.bpRotation);

            foreach (BlueprintWeaponSaveData weapon in data.weapons)
            {
                BlueprintWeapon bw = gridPlacer.PlaceWeapon(weapon.bpWeaponData, weapon.bpPosition, weapon.bpDirection);
                bw.ApplyAttachedDirectionSprite();
            }
        }

        // 설계도 방 호출, 배치
        // GetSavedBPRooms();

        // 설계도 함선 무기 호출, 배치
        // GetSavedWeaponRooms();

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

        // 설계도 설치한 방 데이터 모두 저장 후 제거
        // SaveBPRoomsandDestroy();

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

    public void ReloadBlueprintFromSlot(int slotIndex)
    {
        gridTiles.SetActive(true);
        targetBlueprintShip.ClearRooms();

        var data = BlueprintSlotManager.Instance.GetBlueprintAt(slotIndex);

        if (data != null)
        {
            foreach (var room in data.rooms)
                gridPlacer.PlaceRoom(room.bpRoomData, room.bpLevelIndex, room.bpPosition, room.bpRotation);

            foreach (var weapon in data.weapons)
            {
                BlueprintWeapon w = gridPlacer.PlaceWeapon(weapon.bpWeaponData, weapon.bpPosition, weapon.bpDirection);
                w.ApplyAttachedDirectionSprite();
            }
        }

        gridPlacer.occupiedGridTiles = BlueprintSlotManager.Instance.GetOccupiedTiles(slotIndex);

        CenterCameraToBP();
        GameManager.Instance.playerShip.ClearExistingHulls();
        SetPlayerShipCollidersActive(false);
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

    /// <summary>
    /// 기존에 작업 중이던 설계도의 모든 함선 무기 데이터 호출 및 배치
    /// </summary>
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

    /*
        /// <summary>
        /// '함선 제작' 버튼 클릭 시 호출.
        /// 조건 만족 시 기존 함선을 설계도의 함선으로 교체합니다.
        /// 이후 방, 문의 연결 유효성 검사 수행 후 만족 여부에 따라 실제 함선으로 교체, 다시 설계도로 복구를 수행
        /// </summary>
        public void OnClickBuild()
        {
            // 2. 기존 선원, 함선 백업
            GameManager.Instance.playerShip.BackupAllCrews();
            GameManager.Instance.playerShip.BackupCurrentShip();

            // 3. 설계도 -> 실제 함선 (bpRoom -> Room) 변환
            GameManager.Instance.playerShip.ReplaceShipFromBlueprint(targetBlueprintShip);

            // 4. 실제 Ship 상태 기준 유효성 검사 수행
            validationResult = new ShipValidationHelper().ValidateShipLayout(GameManager.Instance.playerShip);

            // feedbackText.text = $"{validationResult.Message}";
            // 5. 유효성 검사
            if (!validationResult.IsValid)
            {
                Debug.LogError("유효 X");

                // 실패
                feedbackText.text = $"X {validationResult.Message}";

                // 기존 함선으로 복원
                GameManager.Instance.playerShip.RevertToOriginalShip();

                // 기존 선원 복원
                GameManager.Instance.playerShip.CrewSystem.RevertOriginalCrews(GameManager.Instance.playerShip.backupCrewDatas);

                // build 버튼 색상 일시적 빨간색
                if (flashCoroutine != null)
                    StopCoroutine(flashCoroutine);

                flashCoroutine = StartCoroutine(FlashBuildButtonColor(new Color(1, 0, 0, 0.5f), 2f));
            }
            else
            {
                Debug.LogError("유효 O");

                // 성공 -> 함선 교체
                feedbackText.text = $"O Ship updated successfully\n{validationResult.Message}!";

                // RTS 이동을 위한 data 업데이트
                RTSSelectionManager.Instance.RefreshMovementData();

                // 1) 함선 내 모든 방의 타일 - 선원 점유 상태 초기화
                Debug.LogError($"빌드 후 전체 방 수 : {GameManager.Instance.playerShip.GetAllRooms().Count}");
                CrewReservationManager.ClearAllReservations(GameManager.Instance.playerShip);

                // 2) 기존 선원 복구 및 랜덤 배치
                GameManager.Instance.playerShip.CrewSystem.RestoreCrewAfterBuild(GameManager.Instance.playerShip.backupCrewDatas);

                GameManager.Instance.playerShip.AllFreeze();
            }

            // 함선 스텟 다시 계산
            // GameManager.Instance.playerShip.Initialize();
            GameManager.Instance.playerShip.RecalculateAllStats();
        }
    */

    /// <summary>
    /// 도안 저장 버튼 클릭 - 유효성 검사
    /// </summary>
    public void OnClickSave()
    {
        Ship playerShip = GameManager.Instance.playerShip;

        // 5. 편집 중이던 도안 slot에 저장
        BlueprintRoom[] bpRooms = targetBlueprintShip.GetComponentsInChildren<BlueprintRoom>();
        BlueprintLayoutSaver.SaveRoomLayout(bpRooms);

        BlueprintWeapon[] bpWeapons = targetBlueprintShip.GetComponentsInChildren<BlueprintWeapon>();
        BlueprintLayoutSaver.SaveWeaponLayout(bpWeapons);

        List<BlueprintRoomSaveData> savedRooms = BlueprintLayoutSaver.LoadRoomLayout();
        List<BlueprintWeaponSaveData> savedWeapons = BlueprintLayoutSaver.LoadWeaponLayout();
        BlueprintSaveData newData = new(savedRooms, savedWeapons);

        // 6. 슬롯에 저장
        BlueprintSlotManager.Instance.SaveBlueprintToCurrentSlot(newData);
        BlueprintSlotManager.Instance.SaveOccupiedTilesToCurrentSlot(gridPlacer.occupiedGridTiles);

        // 1. 기존 선원, 함선 백업
        playerShip.BackupAllCrews();
        playerShip.BackupCurrentShip();

        // 2. 설계도 -> 실제 함선으로 변환 (유효성 검사 위한 단계)
        playerShip.ReplaceShipFromBlueprint(targetBlueprintShip);

        // 3. 유효성 검사
        validationResult = new ShipValidationHelper().ValidateShipLayout(playerShip);

        // 4. 유효성 검사 결과
        if (!validationResult.IsValid)
        {
            feedbackText.text = $"X {validationResult.Message}";

            // build 버튼 일시적 빨간색 처리
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashSaveButtonColor(new Color(1, 0, 0, 0.5f), 2f));
        }

        // // 5. 편집 중이던 도안 slot에 저장
        // List<BlueprintRoomSaveData> savedRooms = BlueprintLayoutSaver.LoadRoomLayout();
        // List<BlueprintWeaponSaveData> savedWeapons = BlueprintLayoutSaver.LoadWeaponLayout();
        // BlueprintSaveData newData = new(savedRooms, savedWeapons);

        // // 6. 슬롯에 저장
        // BlueprintSlotManager.Instance.SaveBlueprintToCurrentSlot(newData);

        Customize_0_Controller controller0 = customize0Panel.GetComponent<Customize_0_Controller>();
        controller0.UpdateSlotButtonColor(BlueprintSlotManager.Instance.currentSlotIndex, validationResult.IsValid);

        // 7. 도안은 도안이므로 기존 함선으로 복구 (함선, 선원)
        playerShip.RevertToOriginalShip();
        playerShip.CrewSystem.RevertOriginalCrews(playerShip.backupCrewDatas);
    }

    #endregion

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

        customize1Panel.SetActive(false);
        customize0Panel.SetActive(true);
    }

    #region Control Tab (clear, save)

    /// <summary>
    /// 설계도 초기화 및 점유 타일 초기화
    /// </summary>
    public void OnClickClear()
    {
        targetBlueprintShip.ClearRooms();
        gridPlacer.occupiedGridTiles = new HashSet<Vector2Int>();
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
