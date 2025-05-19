using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Customize_2_Controller : MonoBehaviour
{
    [Header("UI panel")]
    [SerializeField] private GameObject customize0Panel;
    [SerializeField] private GameObject customize2Panel;

    [Header("선택된 외갑판")]
    [SerializeField] private int selectedHullLevel = 0;

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
    public BlueprintShip targetBlueprintShip;

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

    private void Start()
    {
        outerhullbutton1.onClick.AddListener(() => { OnClickOuterHullButton(1); });
        outerhullbutton2.onClick.AddListener(() => { OnClickOuterHullButton(2); });
        outerhullbutton3.onClick.AddListener(() => { OnClickOuterHullButton(3); });

        buyButton.onClick.AddListener(() => { OnClickBuy(); });
        cancelButton.onClick.AddListener(() => { OnClickCancel(); });

    }

    /// <summary>
    /// 현재 설계도의 가격 지속 갱신
    /// 설계도로 함선 교체 가능 조건
    /// 1. (설계도 가격 - 기존 함선 가격) <= 보유 재화량
    /// 2. 기존 함선 모든 방 내구도 100%
    /// </summary>
    private void OnEnable()
    {
        selectedData = BlueprintSlotManager.Instance.GetBlueprintAt(BlueprintSlotManager.Instance.currentSlotIndex);
        if (selectedData == null)
            return;

        if (targetBlueprintShip != null && playerShip != null)
        {
            int currentCurrency = ResourceManager.Instance.COMA; // 보유 재화량
            totalBPCost = targetBlueprintShip.GetTotalBPCost(); // 설계도 가격
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

        // BlueprintSaveData 기반으로 blueprintShip 생성
        MakeBPShipWithSaveData();
    }

    /// <summary>
    /// BlueprintSaveData 기반으로 blueprintShip 생성
    /// </summary>
    private void MakeBPShipWithSaveData()
    {
        foreach (BlueprintRoomSaveData room in selectedData.rooms)
            gridPlacer.PlaceRoom(room.bpRoomData, room.bpLevelIndex, room.bpPosition, room.bpRotation);

        foreach (BlueprintWeaponSaveData weapon in selectedData.weapons)
        {
            BlueprintWeapon bw = gridPlacer.PlaceWeapon(weapon.bpWeaponData, weapon.bpPosition, weapon.bpDirection);
            bw.ApplyAttachedDirectionSprite();
        }
    }

    /// <summary>
    /// '함선 제작' 버튼 클릭 시 호출.
    /// 조건 만족 시 기존 함선을 설계도의 함선으로 교체합니다.
    /// 이후 방, 문의 연결 유효성 검사 수행 후 만족 여부에 따라 실제 함선으로 교체, 다시 설계도로 복구를 수행
    /// </summary>
    public void OnClickBuy()
    {
        // selectedData = BlueprintSlotManager.Instance.GetBlueprintAt(BlueprintSlotManager.Instance.currentSlotIndex);
        if (selectedData == null)
            return;

        if (selectedHullLevel == 0)
        {
            feedbackText.text = "Select your OuterHull";
            return;
        }

        // 1. 기존 함선 가격 저장
        originalShipCost = playerShip.GetTotalShipValue();

        // 2. 기존 선원, 함선 백업
        playerShip.BackupAllCrews();

        // 3. 설계도 -> 실제 함선 (bpRoom -> Room) 변환
        playerShip.ReplaceShipFromBlueprint(targetBlueprintShip);

        // 4. 함선 교체 로직
        Debug.LogError("유효 O");

        // 기존 함선 판매
        ResourceManager.Instance.ChangeResource(ResourceType.COMA, originalShipCost);

        // 설계도 함선 구매 (재화량 차감)
        ResourceManager.Instance.ChangeResource(ResourceType.COMA, -targetBlueprintShip.GetTotalBPCost());

        // RTS 이동을 위한 data 업데이트
        RTSSelectionManager.Instance.RefreshMovementData();

        // 1) 함선 내 모든 방의 타일 - 선원 점유 상태 초기화
        Debug.LogError($"빌드 후 전체 방 수 : {playerShip.GetAllRooms().Count}");
        CrewReservationManager.ClearAllReservations(playerShip);

        // 2) 기존 선원 복구 및 랜덤 배치
        playerShip.CrewSystem.RestoreCrewAfterBuild(playerShip.backupCrewDatas);

        playerShip.AllFreeze();

        // 함선 스텟 다시 계산
        playerShip.RecalculateAllStats();

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
        BlueprintSlotManager.Instance.currentSlotIndex = -1;
        selectedHullLevel = 0;
    }

    /// <summary>
    /// 외갑판 적용
    /// </summary>
    /// <param name="level"></param>
    public void OnClickOuterHullButton(int level)
    {
        selectedHullLevel = level;

        // 외갑판 적용
        if (targetBlueprintShip != null)
            targetBlueprintShip.SetHullLevel(selectedHullLevel);

        // 그냥 onenable()에서 replacefromblueprint()해버리고 - onclickbuy() 1~3
        // onclickbuy()에서는 구매작업만
        // onclickcancel()누르면 reverttooriginalship() 다시 가져오면
        // outerhull 작업 + 카메라로 preview띄우기 쉬울듯?

        // bpship으로 할 수 있는 방법 있으면 bpship으로 외갑판 + preview하면 젤 간단
    }
}
