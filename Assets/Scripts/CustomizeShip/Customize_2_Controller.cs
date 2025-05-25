using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Customize_2_Controller : MonoBehaviour
{
    [Header("UI panel")]
    [SerializeField] private BPPreviewCamera previewCam;
    [SerializeField] private GameObject customize0Panel;
    [SerializeField] private GameObject customize2Panel;

    [Header("선택된 외갑판")]
    [SerializeField] public int originHullLevel = -1;
    [SerializeField] public int selectedHullLevel = -1;

    [Header("Buttons")]
    [SerializeField] private Button outerhullbutton1;
    [SerializeField] private Button outerhullbutton2;
    [SerializeField] private Button outerhullbutton3;

    [SerializeField] private Button buyButton;
    [SerializeField] private Button cancelButton;

    [Header("Texts")]
    /// <summary>
    /// 현재 보유 재화량
    /// </summary>
    public TMP_Text currency;

    /// <summary>
    /// 총 설계도 가격을 표시할 텍스트 UI.
    /// </summary>
    public TMP_Text bpCostText;

    /// <summary>
    /// 기존 함선 가격 텍스트
    /// </summary>
    public TMP_Text originShipCostText;

    /// <summary>
    /// 함선 교체 가능한지 여부 텍스트 (불가능 시 이유)
    /// </summary>
    public TMP_Text feedbackText;

    /// <summary>
    /// 현재 커스터마이징 중인 플레이어 함선입니다.
    /// </summary>
    [Header("Ship")]
    public Ship playerShip;

    /// <summary>
    /// 제작한 설계도
    /// </summary>
    public BlueprintShip bpShip;

    /// <summary>
    /// 배치용 가이드라인
    /// </summary>
    [SerializeField] private GridPlacer gridPlacer;

    /// <summary>
    /// 설계도 함선 가격
    /// </summary>
    private int totalBPCost = 0;

    /// <summary>
    /// 기존 사용 중인 함선 가격
    /// </summary>
    private int originalShipCost = 0;

    /// <summary>
    /// 슬롯에 저장된 도안 데이터
    /// </summary>
    private BlueprintSaveData selectedData;

    /// <summary>
    /// 외갑판 prefab (preview용)
    /// </summary>
    [SerializeField] public GameObject previewOuterHullPrefab;

    /// <summary>
    /// 외갑판 적용 preview 보여줄 이미지
    /// </summary>
    [SerializeField] private RawImage hullPreviewImage;

    private bool buyClicked = false;

    private void Start()
    {
        outerhullbutton1.onClick.AddListener(() => { OnClickOuterHullButton(0); });
        outerhullbutton2.onClick.AddListener(() => { OnClickOuterHullButton(1); });
        outerhullbutton3.onClick.AddListener(() => { OnClickOuterHullButton(2); });

        buyButton.onClick.AddListener(() => { OnClickBuy(); });
        cancelButton.onClick.AddListener(() => { OnClickCancel(); });

        playerShip = GameManager.Instance.playerShip;
    }

    /// <summary>
    /// 현재 설계도의 가격 지속 갱신
    /// 설계도로 함선 교체 가능 조건
    /// 1. (설계도 가격 - 기존 함선 가격) <= 보유 재화량
    /// 2. 기존 함선 모든 방 내구도 100%
    /// </summary>
    private void OnEnable()
    {
        playerShip = GameManager.Instance.playerShip;
        playerShip.SetShipContentsActive(false);

        selectedData = BlueprintSlotManager.Instance.GetBlueprintAt(BlueprintSlotManager.Instance.currentSlotIndex);
        if (selectedData == null)
            return;
        if (selectedData.rooms.Count == 0 && selectedData.weapons.Count == 0)
            return;

        // BlueprintSaveData 기반으로 blueprintShip 생성
        bpShip = MakeBPShipWithSaveData();

        int index = BlueprintSlotManager.Instance.currentSlotIndex;
        originHullLevel = BlueprintSlotManager.Instance.blueprintSlots[index].hullLevel;

        if (bpShip != null && playerShip != null)
        {
            AdjustRawImageAspect();

            int currentCurrency = ResourceManager.Instance.COMA; // 보유 재화량
            totalBPCost = bpShip.GetTotalBPCost(); // 설계도 가격
            originalShipCost = playerShip.GetTotalShipValue(); // 사용중인 함선 가격

            currency.text = $"Currency : {currentCurrency}";
            bpCostText.text = $"Blueprint Cost : {totalBPCost}";
            originShipCostText.text = $"Origin Ship Cost : {originalShipCost}";
            feedbackText.text = "";

            // 1. 조건 : 자산
            bool hasEnoughMoney = totalBPCost - originalShipCost <= currentCurrency;

            // 2. 기존 함선 모든 방 내구도 100%
            bool shipFullyRepaired = playerShip.IsFullHitPoint();

            // 조건 체크
            if (hasEnoughMoney && shipFullyRepaired)
            {
                feedbackText.text = "Can Buy!";
                buyButton.interactable = true;
            }
            else
            {
                if (!hasEnoughMoney)
                    feedbackText.text = "Not Enough Money";
                if (!shipFullyRepaired)
                    feedbackText.text = "Your Ship needs Repairs";

                buyButton.interactable = false;
            }
        }
    }

    /// <summary>
    /// BlueprintSaveData 기반으로 blueprintShip 생성
    /// </summary>
    public BlueprintShip MakeBPShipWithSaveData()
    {
        List<Vector2Int> tiles = new();

        bpShip.ClearRooms();

        int index = BlueprintSlotManager.Instance.currentSlotIndex;
        bool isValidIndex = (0 <= index && index <= 3) ? true : false;
        if (!isValidIndex)
            bpShip.ClearPreviewOuterHulls();

        selectedData = BlueprintSlotManager.Instance.GetBlueprintAt(BlueprintSlotManager.Instance.currentSlotIndex);
        if (selectedData == null)
            return null;

        if (selectedData.rooms.Count == 0)
            return bpShip;

        foreach (BlueprintRoomSaveData room in selectedData.rooms)
        {
            GameObject bpRoom = gridPlacer.PlacePreviewRoom(room.bpRoomData, room.bpLevelIndex, room.bpPosition, room.bpRotation, bpShip.transform, tiles);
            BlueprintRoom br = bpRoom.GetComponent<BlueprintRoom>();
            bpShip.AddPlaceable(br);
        }

        if (selectedData.weapons.Count == 0)
            return bpShip;

        foreach (BlueprintWeaponSaveData weapon in selectedData.weapons)
        {
            GameObject bpWeapon = gridPlacer.PlacePreviewWeapon(weapon.bpWeaponData, weapon.bpPosition, weapon.bpDirection, bpShip.transform, tiles);
            BlueprintWeapon bw = bpWeapon.GetComponent<BlueprintWeapon>();

            // 외갑판 세팅 X (하양으로 초기화) - 임시 0레벨
            if (bw.GetHullLevel() == -1)
            {
                bw.SetHullLevel(0);
                bw.ApplyAttachedDirectionSprite();
                bpShip.AddPlaceable(bw);
                bw.SetHullLevel(-1);
            }
            else // 외갑판 세팅 이미 했었음
            {
                bw.ApplyAttachedDirectionSprite();
                bpShip.AddPlaceable(bw);
            }
        }

        // return gridPlacer.targetBlueprintShip;
        return bpShip;
    }

    /// <summary>
    /// '함선 제작' 버튼 클릭 시 호출.
    /// 조건 만족 시 기존 함선을 설계도의 함선으로 교체합니다.
    /// 이후 방, 문의 연결 유효성 검사 수행 후 만족 여부에 따라 실제 함선으로 교체, 다시 설계도로 복구를 수행
    /// </summary>
    public void OnClickBuy()
    {
        if (selectedData == null || (selectedData.rooms.Count == 0 && selectedData.weapons.Count == 0))
            return;

        if (selectedHullLevel == -1)
        {
            feedbackText.text = "Select your OuterHull";
            return;
        }

        // 도안에 외갑판 레벨 저장
        selectedData.hullLevel = selectedHullLevel;

        // 1. 기존 함선 가격 저장
        originalShipCost = playerShip.GetTotalShipValue();

        // 2. 기존 선원, 함선 백업
        playerShip.BackupAllCrews();

        // 3. 설계도 -> 실제 함선 (bpRoom -> Room) 변환
        playerShip.ReplaceShipFromBlueprint(bpShip);

        // 4. 함선 교체 로직
        // 기존 함선 판매
        ResourceManager.Instance.ChangeResource(ResourceType.COMA, originalShipCost);

        // 설계도 함선 구매 (재화량 차감)
        ResourceManager.Instance.ChangeResource(ResourceType.COMA, -bpShip.GetTotalBPCost());

        // RTS 이동을 위한 data 업데이트
        RTSSelectionManager.Instance.RefreshMovementData();

        // 1) 함선 내 모든 방의 타일 - 선원 점유 상태 초기화
        CrewReservationManager.ClearAllReservations(playerShip);

        // 2) 기존 선원 복구 및 랜덤 배치
        playerShip.CrewSystem.RestoreCrewAfterBuild(playerShip.backupCrewDatas);

        playerShip.AllFreeze();

        // 함선 스텟 다시 계산
        playerShip.RecalculateAllStats();
        playerShip.RefreshAllSystems();

        // 기존 외갑판 삭제
        playerShip.OuterHullSystem.ClearExistingHulls();

        // 외갑판 실제로 생성
        playerShip.SetOuterHullLevel(selectedHullLevel);
        playerShip.UpdateOuterHullVisuals();
        BlueprintSlotManager.Instance.appliedSlotIndex = BlueprintSlotManager.Instance.currentSlotIndex;

        // 도안도 playership으로 업데이트
        BlueprintSaveData data = BlueprintSlotManager.Instance.GetBlueprintAt(BlueprintSlotManager.Instance.currentSlotIndex);
        Customize_0_Controller controller0 = customize0Panel.GetComponent<Customize_0_Controller>();
        controller0.bpPreviewArea.UpdateAndShow(data);
        controller0.OnClickSlot(BlueprintSlotManager.Instance.appliedSlotIndex);

        playerShip.SetShipContentsActive(false);

        buyClicked = true;

        Debug.Log("갈아끼워");

        // ShipSerialization.SaveShip(GameManager.Instance.playerShip, "playership");

        ES3.DeleteFile("playership");

        StartCoroutine(SaveAfterDelay());

        // OnClickCancel();
    }

    private IEnumerator SaveAfterDelay()
    {
        yield return new WaitForSeconds(0.25f);

        ShipSerialization.SaveShip(GameManager.Instance.playerShip, "playerShip");
        OnClickCancel();
    }

    /// <summary>
    /// 구매 취소 버튼 (customize_0_panel로 복귀)
    /// </summary>
    public void OnClickCancel()
    {
        customize2Panel.SetActive(false);
        customize0Panel.SetActive(true);

        // 초기화 작업
        selectedHullLevel = -1;
        feedbackText.text = "";
        Customize_0_Controller controller0 = customize0Panel.GetComponent<Customize_0_Controller>();
        controller0.applyButton.interactable = false;

        BlueprintRoom[] bpRooms = bpShip.GetComponentsInChildren<BlueprintRoom>();
        BlueprintWeapon[] bpWeapons = bpShip.GetComponentsInChildren<BlueprintWeapon>();

        foreach (BlueprintRoom r in bpRooms)
            Destroy(r.gameObject);

        foreach (BlueprintWeapon w in bpWeapons)
            Destroy(w.gameObject);

        if (!buyClicked)
        {
            bpShip.ClearPreviewOuterHulls();

            selectedData.hullLevel = originHullLevel;

            // 외갑판 초기화
            bpShip.SetBPHullLevel(selectedData.hullLevel, previewOuterHullPrefab);
        }

        buyClicked = false;
        bpShip.ClearRooms();
    }

    /// <summary>
    /// 외갑판 적용
    /// </summary>
    /// <param name="level"></param>
    public void OnClickOuterHullButton(int level)
    {
        selectedHullLevel = level;

        // 외갑판 적용
        if (bpShip != null)
            bpShip.SetBPHullLevel(selectedHullLevel, previewOuterHullPrefab);

        // 데이터에 저장
        selectedData.hullLevel = level;

        // 비율 조정
        AdjustRawImageAspect();

        // 강제 렌더링
        previewCam.RenderOnce();
    }

    /// <summary>
    /// Image 비율 조정
    /// </summary>
    private void AdjustRawImageAspect()
    {
        RenderTexture rt = previewCam.gameObject.GetComponent<Camera>().targetTexture;
        float textureWidth = rt.width;
        float textureHeight = rt.height;

        float imageWidth = hullPreviewImage.rectTransform.rect.width;
        float imageHeight = hullPreviewImage.rectTransform.rect.height;

        float textureRatio = textureWidth / textureHeight;
        float imageRatio = imageWidth / imageHeight;

        // 더 튀어나가지 않는 방향으로 기준 잡기
        if (textureRatio > imageRatio) // 텍스처가 더 가로로 길다
        {
            hullPreviewImage.rectTransform.sizeDelta = new Vector2(imageWidth, imageWidth / textureRatio);
        }
        else // 텍스처가 더 세로로 길다
        {
            hullPreviewImage.rectTransform.sizeDelta = new Vector2(imageHeight * textureRatio, imageHeight);
        }
    }

}
