using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 장비 관련 UI 담당 Handler
/// </summary>
public class EquipmentUIHandler : MonoBehaviour
{
    /// <summary>
    /// 장비 구매 팝업 패널입니다.
    /// </summary>
    [Header("Tip Panel References")] public GameObject itemBuyPanel;

    /// <summary>
    /// 보유 중인 전체 선원 나타내는 패널
    /// </summary>
    public GameObject allCrewPanel;

    /// <summary>
    /// 보유 중인 전체 장비 나타내는 패널
    /// </summary>
    public GameObject allEquipmentPanel;

    /// <summary>
    /// 개별 선원 디테일 정보 표시
    /// </summary>
    public GameObject crewDetailPanel;

    /// <summary>
    /// 플레이어 함선
    /// </summary>
    public Ship playerShip;

    /// <summary>
    /// 팝업 내 장비 아이콘 이미지입니다.
    /// </summary>
    public Image tipItemImage;

    /// <summary>
    /// 팝업 내 장비 이름 텍스트입니다.
    /// </summary>
    public TextMeshProUGUI tipItemName;

    /// <summary>
    /// 팝업 내 장비 가격 텍스트입니다.
    /// </summary>
    public TextMeshProUGUI tipItemPrice;

    /// <summary>
    /// 팝업 내 장비 설명 텍스트입니다.
    /// </summary>
    public TextMeshProUGUI tipItemDetails;

    /// <summary>
    /// 팝업 내 현재 보유 재화 표시 텍스트입니다.
    /// </summary>
    public TextMeshProUGUI tipCurrencyText;

    /// <summary>
    /// 구매 버튼입니다.
    /// </summary>
    public Button buyButton;

    /// <summary>
    /// 뒤로 가기 버튼입니다.
    /// </summary>
    public Button backButton;

    /// <summary>
    /// 장비 구매 완료 시 버튼에 적용할 색상입니다.
    /// </summary>
    [Header("Color Settings")] public Color purchasedButtonColor = Color.gray;

    /// <summary>
    /// 장비 버튼의 기본 색상입니다.
    /// </summary>
    public Color defaultButtonColor = Color.white;

    /// <summary>
    /// 현재 선택된 장비 데이터입니다.
    /// </summary>
    public EquipmentItem currentSelectedItem;

    /// <summary>
    /// 현재 선택된 장비 버튼입니다.
    /// </summary>
    public EquipmentButton currentSelectedButton;

    /// <summary>
    /// 구매한 장비 목록입니다.
    /// 중복 구매를 방지하기 위해 HashSet으로 관리됩니다.
    /// </summary>
    public HashSet<EquipmentItem> purchasedItems = new();

    /// <summary>
    /// 아직 선원에게 적용시키지 않은 장비 목록
    /// </summary>
    public HashSet<EquipmentItem> unUsedItems = new();

    /// <summary>
    /// 장비 착용할 선택된 선원
    /// </summary>
    public CrewMember selectedCrew = null;

    /// <summary>
    /// 임시로 관리하는 플레이어 보유 재화(COMAs)입니다.
    /// </summary>
    public int currentCurrency = 0;

    [Header("Apply UI References")]
    /// <summary>
    /// 선원 리스트 UI에서 각 선원의 Prefab
    /// </summary>
    public GameObject crewIconPrefab;

    /// <summary>
    /// 장비 Prefab
    /// </summary>
    public GameObject equipmentIconPrefab;

    /// <summary>
    /// 각 crew가 설치될 content
    /// </summary>
    public Transform crewContentParent;

    /// <summary>
    /// 각 장비가 설치될 content
    /// </summary>
    public Transform equipmentContent;

    /// <summary>
    /// 개별 선원 디테일 UI에서 선원 이미지
    /// </summary>
    public Image detailCrewImage;

    /// <summary>
    /// 개별 선원 디테일 UI에서 무기 장비 이미지
    /// </summary>
    public Image detailWeaponImage;

    /// <summary>
    /// 개별 선원 디테일 UI에서 방어구 장비 이미지
    /// </summary>
    public Image detailShieldImage;

    /// <summary>
    /// 개별 선원 디테일 UI에서 보조 장비 이미지
    /// </summary>
    public Image detailAssistantImage;

    /// <summary>
    /// 완료 버튼 (global용)
    /// </summary>
    public Button globalCompleteButton;

    /// <summary>
    /// 뒤로 가기 버튼 (personal용)
    /// </summary>
    public Button personalBackButton;

    /// <summary>
    /// 착용 중인 장비 없음을 나타내는 icon
    /// </summary>
    public Sprite noneIcon;

    [Header("Applied Tag")]
    /// <summary>
    /// 장비 적용됨 알리는 tag
    /// </summary>
    public GameObject appliedTagPrefab;

    /// <summary>
    /// 생성된 장비 적용 태그 추적 리스트
    /// </summary>
    private List<GameObject> activeAppliedTags = new();

    [Header("Stat Comparison")]
    /// <summary>
    /// 스텟 비교 패널
    /// </summary>
    public GameObject statComparePanel;

    /// <summary>
    /// 기존 스텟
    /// </summary>
    public TextMeshProUGUI baseStatsText;

    /// <summary>
    /// 장비 착용, 미착용 후 변화 스텟
    /// </summary>
    public TextMeshProUGUI newStatsText;

    /// <summary>
    /// 차이
    /// </summary>
    public TextMeshProUGUI statDiffText;

    private EquipmentItem pendingEquipItem = null;
    private EquipmentType pendingEquipType = EquipmentType.None;
    private bool isPendingRemoval = false;

    /// <summary>
    /// 시작 시 장비 팝업을 비활성화합니다.
    /// </summary>
    private void Start()
    {
        // 팝업 비활성화
        itemBuyPanel.SetActive(false);
    }

    /// <summary>
    /// 현재 재화 보유량 업데이트
    /// </summary>
    private void OnEnable()
    {
        // 보유 재화량 업데이트
        currentCurrency = (int)ResourceManager.Instance.COMA;
    }

    /// <summary>
    /// 장비 버튼 클릭 시 팝업을 띄우고 UI 요소를 채웁니다.
    /// 이미 구매했거나 재화 부족 시 구매 불가 처리도 포함됩니다.
    /// </summary>
    /// <param name="eqItem">선택된 장비 아이템.</param>
    /// <param name="eqButton">선택된 장비 버튼.</param>
    public void ShowItemTip(EquipmentItem eqItem, EquipmentButton eqButton)
    {
        currentSelectedItem = eqItem;
        currentSelectedButton = eqButton;

        // 팝업 UI 채우기
        tipItemImage.sprite = eqItem.eqIcon;
        tipItemName.text = eqItem.eqName;
        tipItemPrice.text = eqItem.eqPrice.ToString();
        // tipItemDetails = eqItem.Details;
        tipCurrencyText.text = "COMA: " + currentCurrency;

        // 이미 구매한 아이템이면 buy 버튼 비활성화
        if (purchasedItems.Contains(eqItem))
            buyButton.interactable = false;
        else
            // 구매 가능 여부 확인
            buyButton.interactable = currentCurrency >= eqItem.eqPrice;

        // 팝업 표시
        itemBuyPanel.SetActive(true);
    }

    /// <summary>
    /// 구매 버튼 클릭 시 호출됩니다.
    /// 재화를 차감하고, 장비 효과를 적용하며, 버튼 색상도 변경됩니다.
    /// </summary>
    public void OnClickBuy()
    {
        // 이미 구매했거나 골드 부족하면 return
        if (purchasedItems.Contains(currentSelectedItem))
            return;
        if (currentCurrency < currentSelectedItem.eqPrice)
            return;

        // 구매 처리
        currentCurrency -= currentSelectedItem.eqPrice;
        unUsedItems.Add(currentSelectedItem);

        // 보유 전체 아이템 리스트 삭제 예정
        purchasedItems.Add(currentSelectedItem);

        EquipmentItem eq = currentSelectedItem;

        // 구매 팝업 닫기
        itemBuyPanel.SetActive(false);

        // 착용 팝업 활성화
        allCrewPanel.SetActive(true);
        allEquipmentPanel.SetActive(true);

        // 전체 장비일 경우 즉시 갱신
        if (eq.isGlobalEquip)
            EquipmentManager.Instance.PurchaseAndEquipGlobal(eq);

        // 보유 선원 전체 리스트 띄우기
        PopulateCrewIcons(eq);

        // 보유 장비 전체 리스트 띄우기
        PopulateEquipmentIcons();

        // 개인 장비 갱신
        if (!eq.isGlobalEquip)
        {
            if (selectedCrew == null)
            {
                // 장비 착용할 선원 경고 UI 필요
                Debug.LogWarning("장비 착용 실패, 선택된 선원 X");
                return;
            }

            EquipmentManager.Instance.PurchaseAndEquipPersonal(selectedCrew, eq);
        }
    }

    /// <summary>
    /// 장비 구매 팝업을 닫습니다.
    /// </summary>
    public void OnClickBack()
    {
        // 구매 팝업 닫기
        itemBuyPanel.SetActive(false);

        // 착용 팝업 닫기
        allCrewPanel.SetActive(false);
        allEquipmentPanel.SetActive(false);

        // 보유 전체 선원 ui 업데이트 필요
        /*
        foreach (Transform child in crewContentParent)
        {
            child.
        }
        */
    }

    /// <summary>
    /// 보유 중인 모든 선원 리스트 띄우기
    /// </summary>
    /// <param name="eq"></param>
    private void PopulateCrewIcons(EquipmentItem eq)
    {
        // 기존 항목 제거
        foreach (Transform child in crewContentParent)
            Destroy(child.gameObject);

        List<CrewMember> crewList = playerShip.allCrews;

        // 모든 선원 표시 리스트 호출
        foreach (CrewMember crew in crewList)
        {
            GameObject iconObj = Instantiate(crewIconPrefab, crewContentParent);
            CrewIconButton icon = iconObj.GetComponent<CrewIconButton>();
            icon.Initialize(crew, OnCrewIconSelected);

            // global 장비인 경우 즉시 모든 선원에게 적용 및 apply tag 띄우기
            if (eq.isGlobalEquip)
            {
                GameObject tag = Instantiate(appliedTagPrefab, iconObj.transform);
                RectTransform tagRect = tag.GetComponent<RectTransform>();
                tagRect.anchorMin = new Vector2(0, 0);
                tagRect.anchorMax = new Vector2(0, 0);
                tagRect.pivot = new Vector2(0, 0);
                tagRect.anchoredPosition = new Vector2(0, -30);
                tagRect.localScale = Vector3.one;

                activeAppliedTags.Add(tag);
            }
        }

        crewDetailPanel.SetActive(false);
    }

    /// <summary>
    /// 보유 중인 전체 장비 리스트 갱신
    /// </summary>
    private void PopulateEquipmentIcons()
    {
        foreach (Transform child in equipmentContent)
            Destroy(child.gameObject);

        foreach (EquipmentItem eqItem in unUsedItems)
        {
            GameObject eqObj = Instantiate(equipmentIconPrefab, equipmentContent);
            EquipmentIconButton icon = eqObj.GetComponent<EquipmentIconButton>();
            icon.Initialize(eqItem, OnEquipmentIconSelected);
        }
    }

    /// <summary>
    /// 선원 선택됨
    /// </summary>
    /// <param name="crew"></param>
    private void OnCrewIconSelected(CrewMember crew)
    {
        selectedCrew = crew;

        // 상세 정보 패널 표시
        crewDetailPanel.SetActive(true);

        // 전체 선원 패널 비활성화
        allCrewPanel.SetActive(false);


        detailCrewImage.sprite = crew.spriteRenderer.sprite;
        detailWeaponImage.sprite = crew.equippedWeapon.eqIcon != null ? crew.equippedWeapon.eqIcon : noneIcon;
        detailShieldImage.sprite = crew.equippedShield.eqIcon != null ? crew.equippedShield.eqIcon : noneIcon;
        detailAssistantImage.sprite = crew.equippedAssistant.eqIcon != null ? crew.equippedAssistant.eqIcon : noneIcon;

        // 장비 적용
        EquipmentManager.Instance.PurchaseAndEquipPersonal(crew, currentSelectedItem);
    }

    /// <summary>
    /// 장비 선택됨 (장비 착용, 미착용 등 스텟 비교)
    /// </summary>
    /// <param name="item"></param>
    private void OnEquipmentIconSelected(EquipmentItem item)
    {
        if (selectedCrew == null)
            return;

        // 기존 장비와 비교하여 스탯 변화 시뮬레이션
        // (선원 현재 능력치 저장하는 클래스가 있는지 확인 필요)
        EquipmentStats before = selectedCrew.GetCombinedStats();
        EquipmentStats after = selectedCrew.GetStatsIfEquipped(item);

        ShowStatComparison(before, after);
        statComparePanel.SetActive(true);

        pendingEquipItem = item;
        pendingEquipType = item.eqType;
        isPendingRemoval = false;
    }

    /// <summary>
    /// 장비 착용 / 해제 비교
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after"></param>
    private void ShowStatComparison(EquipmentStats before, EquipmentStats after)
    {
        baseStatsText.text = $@"
        {before.health}\n
        {before.attack}\n
        {before.defense}\n
        \n
        {before.pilotSkill}\n
        {before.engineSkill}\n
        {before.powerSkill}\n
        {before.shieldSkill}\n
        {before.weaponSkill}\n
        {before.ammunitionSkill}\n
        {before.medbaySkill}\n
        {before.repairSkill}
        ";

        newStatsText.text = $@"
        {after.health}\n
        {after.attack}\n
        {after.defense}\n
        \n
        {after.pilotSkill}\n
        {after.engineSkill}\n
        {after.powerSkill}\n
        {after.shieldSkill}\n
        {after.weaponSkill}\n
        {after.ammunitionSkill}\n
        {after.medbaySkill}\n
        {after.repairSkill}
        ";

        statDiffText.text = $@"
        {FormatDiff(after.health - before.health)}\n
        {FormatDiff(after.attack - before.attack)}\n
        {FormatDiff(after.defense - before.defense)}\n
        \n
        {FormatDiff(after.pilotSkill - before.pilotSkill)}\n
        {FormatDiff(after.engineSkill - before.engineSkill)}\n
        {FormatDiff(after.powerSkill - before.powerSkill)}\n
        {FormatDiff(after.shieldSkill - before.shieldSkill)}\n
        {FormatDiff(after.weaponSkill - before.weaponSkill)}\n
        {FormatDiff(after.ammunitionSkill - before.ammunitionSkill)}\n
        {FormatDiff(after.medbaySkill - before.medbaySkill)}\n
        {FormatDiff(after.repairSkill - before.repairSkill)}
        ";
    }

    /// <summary>
    /// 차이를 string으로 반환
    /// </summary>
    /// <param name="diff"></param>
    /// <returns></returns>
    private string FormatDiff(float diff)
    {
        return (int)diff == 0 ? "±0" : diff > 0 ? $"+{diff}" : diff.ToString();
    }

    /// <summary>
    /// 장비 해제 예비 작업
    /// </summary>
    /// <param name="type"></param>
    public void OnClickPendingUnEquip(EquipmentType type)
    {
        if (selectedCrew == null)
            return;

        EquipmentStats before = selectedCrew.GetCombinedStats();
        EquipmentStats after = selectedCrew.GetStatsIfEquipped(EquipmentManager.Instance.GetDefaultEquipment(type));

        ShowStatComparison(before, after);
        statComparePanel.SetActive(true);

        pendingEquipItem = EquipmentManager.Instance.GetDefaultEquipment(type);
        pendingEquipType = type;
        isPendingRemoval = true;
    }

    public void OnClickUnEquipWeaponSlot()
    {
        OnClickPendingUnEquip(EquipmentType.WeaponEquipment);
    }

    public void OnClickUnEquipShieldSlot()
    {
        OnClickPendingUnEquip(EquipmentType.ShieldEquipment);
    }

    public void OnClickUnEquipAssistantSlot()
    {
        OnClickPendingUnEquip(EquipmentType.AssistantEquipment);
    }

    /// <summary>
    /// ok 버튼 눌러 장비를 선원에게 직접 적용 / 해제
    /// </summary>
    public void OnClickStatApplyOK()
    {
        if (selectedCrew == null || pendingEquipItem == null)
            return;

        // 장비 해제면 이전 장비를 다시 unused 목록에 추가
        if (isPendingRemoval)
        {
            EquipmentItem removed = GetCurrentEquippedByType(pendingEquipType);
            if (removed != null && !removed.isGlobalEquip)
                unUsedItems.Add(removed);
        }
        else
        {
            if (!pendingEquipItem.isGlobalEquip)
                unUsedItems.Remove(pendingEquipItem);
        }

        // 실제 적용
        EquipmentManager.Instance.PurchaseAndEquipPersonal(selectedCrew, pendingEquipItem);

        // UI 갱신
        RefreshCrewDetailPanel();
        PopulateCrewIcons(pendingEquipItem);
        PopulateEquipmentIcons();

        // 리셋
        pendingEquipItem = null;
        pendingEquipType = EquipmentType.None;
        isPendingRemoval = false;

        statComparePanel.SetActive(false);
    }

    /// <summary>
    /// 현재 착용 중인 장비 반환
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private EquipmentItem GetCurrentEquippedByType(EquipmentType type)
    {
        return type switch
        {
            EquipmentType.WeaponEquipment => selectedCrew.equippedWeapon,
            EquipmentType.ShieldEquipment => selectedCrew.equippedShield,
            EquipmentType.AssistantEquipment => selectedCrew.equippedAssistant,
            _ => null
        };
    }

    /// <summary>
    /// 선원 개인 디테일 패널 정보 갱신
    /// </summary>
    private void RefreshCrewDetailPanel()
    {
        detailWeaponImage.sprite = selectedCrew.equippedWeapon != null ? selectedCrew.equippedWeapon.eqIcon : noneIcon;
        detailShieldImage.sprite = selectedCrew.equippedShield != null ? selectedCrew.equippedShield.eqIcon : noneIcon;
        detailAssistantImage.sprite =
            selectedCrew.equippedAssistant != null ? selectedCrew.equippedAssistant.eqIcon : noneIcon;

        EquipmentStats currentStats = selectedCrew.GetCombinedStats();
        baseStatsText.text = currentStats.ToString();
        newStatsText.text = "";
        statDiffText.text = "";
    }

    /// <summary>
    /// 선원에 장비 적용 마침
    /// </summary>
    public void OnClickComplete()
    {
        // 모든 선원 apply tag 삭제
        foreach (GameObject tag in activeAppliedTags)
            if (tag != null)
                Destroy(tag);

        activeAppliedTags.Clear();
        allCrewPanel.SetActive(false);
        allEquipmentPanel.SetActive(false);

        // 스텟 비교 창 초기화
        baseStatsText.text = selectedCrew.GetCombinedStats().ToString();
        newStatsText.text = "";
        statDiffText.text = "";
    }

    /// <summary>
    /// 선원 개인 디테일 panel 끄기
    /// </summary>
    public void OnClickBackPersonal()
    {
        crewDetailPanel.SetActive(false);

        allCrewPanel.SetActive(true);

        // 스텟 비교 창 초기화
        baseStatsText.text = selectedCrew.GetCombinedStats().ToString();
        newStatsText.text = "";
        statDiffText.text = "";
    }
}
