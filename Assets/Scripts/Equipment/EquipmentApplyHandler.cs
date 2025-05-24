using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentApplyHandler : MonoBehaviour
{
    /// <summary>
    /// 개별 선원 디테일 정보 표시
    /// </summary>
    public GameObject crewDetailPanel;

    /// <summary>
    /// 보유 중인 전체 장비 나타내는 패널
    /// </summary>
    public GameObject allEquipmentPanel;

    /// <summary>
    /// 구매한 장비 목록입니다.
    /// 중복 구매를 방지하기 위해 HashSet으로 관리됩니다.
    /// </summary>
    public HashSet<EquipmentItem> purchasedItems = new();

    /// <summary>
    /// 장비 착용할 선택된 선원
    /// </summary>
    public CrewMember selectedCrew = null;

    [Header("소유중인 모든 장비 UI")]
    /// <summary>
    /// 장비 Prefab
    /// </summary>
    public GameObject equipmentIconPrefab;

    /// <summary>
    /// 각 장비가 설치될 content
    /// </summary>
    public Transform equipmentContent;

    /// <summary>
    /// 개별 선원 디테일 UI에서 선원 이미지
    /// </summary>
    public Image detailCrewImage;

    /// <summary>
    /// 개별 선원 디테일 UI에서 무기 장비 버튼
    /// </summary>
    public Button detailWeaponButton;

    /// <summary>
    /// 개별 선원 디테일 UI에서 방어구 장비 버튼
    /// </summary>
    public Button detailShieldButton;

    /// <summary>
    /// 개별 선원 디테일 UI에서 보조 장비 버튼
    /// </summary>
    public Button detailAssistButton;

    /// <summary>
    /// 착용 중인 장비 없음을 나타내는 icon
    /// </summary>
    public Sprite noneIcon;

    /// <summary>
    /// 선원에게 장비 적용 버튼
    /// </summary>
    public Button applyButton;

    /// <summary>
    /// 선원 리스트로 돌아가는 버튼
    /// </summary>
    public Button backButton;

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

    /// <summary>
    /// 해제하는 장비
    /// </summary>
    private EquipmentItem pendingEquipItem = null;

    /// <summary>
    /// 해제하는 장비 타입
    /// </summary>
    private EquipmentType pendingEquipType = EquipmentType.None;

    /// <summary>
    /// 장비 해제 완료 여부
    /// </summary>
    private bool isPendingRemoval = false;

    /// <summary>
    /// 선원 패널
    /// </summary>
    public SlidePanelController slidePanelController;

    [Header("Stat Comparison Rows")]
    /// <summary>
    /// 변화 표시 오브젝트 (우측 화살표)
    /// </summary>
    public GameObject midArrows;
    [SerializeField] private Image[] changeArrows;
    [SerializeField] private Image beforeEquipImage;
    [SerializeField] private Image afterEquipImage;
    [SerializeField] private Sprite increaseSprite;
    [SerializeField] private Sprite decreaseSprite;
    [SerializeField] private Sprite noChangeSprite;

    private void OnEnable()
    {
        // 팝업 활성화
        crewDetailPanel.SetActive(true);
        allEquipmentPanel.SetActive(true);
    }

    public void Initialize()
    {
        // 보유 장비 전체 리스트 띄우기
        PopulateEquipmentIcons();

        // button click 함수 연결
        detailWeaponButton.onClick.AddListener(() => { OnClickUnEquipWeaponSlot(); });
        detailShieldButton.onClick.AddListener(() => { OnClickUnEquipShieldSlot(); });
        detailAssistButton.onClick.AddListener(() => { OnClickUnEquipAssistantSlot(); });
        applyButton.onClick.AddListener(() => { OnClickStatApplyButton(); });
        backButton.onClick.AddListener(() => { OnClickBackPersonal(); });
    }

    /// <summary>
    /// 보유 중인 전체 장비 리스트 갱신
    /// </summary>
    private void PopulateEquipmentIcons()
    {
        foreach (Transform child in equipmentContent)
            Destroy(child.gameObject);

        foreach (EquipmentItem eqItem in GameManager.Instance.playerShip.unUsedItems)
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
    public void OnCrewIconSelected(CrewMember crew)
    {
        selectedCrew = crew;

        // 상세 정보 패널 표시
        // crewDetailPanel.SetActive(true);

        detailCrewImage.sprite = crew.portraitSprite;

        Image detailWeaponImage = detailWeaponButton.GetComponent<Image>();
        Image detailShieldImage = detailShieldButton.GetComponent<Image>();
        Image detailAssistantImage = detailAssistButton.GetComponent<Image>();

        detailWeaponImage.sprite = crew.equippedWeapon.eqIcon != null ? crew.equippedWeapon.eqIcon : noneIcon;
        detailShieldImage.sprite = crew.equippedShield.eqIcon != null ? crew.equippedShield.eqIcon : noneIcon;
        detailAssistantImage.sprite = crew.equippedAssistant.eqIcon != null ? crew.equippedAssistant.eqIcon : noneIcon;

        // 스텟 초기화
        baseStatsText.text = "";
        newStatsText.text = "";
        statDiffText.text = "";

        // detail panel 활성화 시 기본 이미지 비활성화
        beforeEquipImage.enabled = false;
        afterEquipImage.enabled = false;

        foreach (Image arrow in changeArrows)
            arrow.enabled = false;
    }

    /// <summary>
    /// 장비 착용 / 해제 비교
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after"></param>
    private void ShowStatComparison(EquipmentStats before, EquipmentStats after)
    {
        List<float> beforeVals = new List<float>{
            before.health,
            before.attack,
            before.defense,
            before.pilotSkill,
            before.engineSkill,
            before.powerSkill,
            before.shieldSkill,
            before.weaponSkill,
            before.ammunitionSkill,
            before.medbaySkill,
            before.repairSkill
        };

        List<float> afterVals = new List<float>{
            after.health,
            after.attack,
            after.defense,
            after.pilotSkill,
            after.engineSkill,
            after.powerSkill,
            after.shieldSkill,
            after.weaponSkill,
            after.ammunitionSkill,
            after.medbaySkill,
            after.repairSkill
        };

        string baseStr = "";
        string newStr = "";
        string diffStr = "";

        beforeEquipImage.enabled = true;
        afterEquipImage.enabled = true;
        midArrows.SetActive(true);

        for (int i = 0; i < beforeVals.Count; i++)
        {
            if (i == 3)
            {
                baseStr += "\n";
                newStr += "\n";
                diffStr += "\n";
            }

            int diff = (int)(afterVals[i] - beforeVals[i]);
            baseStr += $"{beforeVals[i]}\n";
            newStr += $"{afterVals[i]}\n";

            if (diff == 0)
            {
                changeArrows[i].sprite = noChangeSprite;
                diffStr += "\n";
            }
            else if (diff > 0)
            {
                changeArrows[i].sprite = increaseSprite;
                diffStr += $"{diff}\n";
            }
            else
            {
                changeArrows[i].sprite = decreaseSprite;
                diffStr += $"{Mathf.Abs(diff)}\n";
            }
            changeArrows[i].enabled = true;
        }

        baseStatsText.text = baseStr;
        newStatsText.text = newStr;
        statDiffText.text = diffStr;
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

        if (item.eqType == EquipmentType.WeaponEquipment)
            beforeEquipImage.sprite = selectedCrew.equippedWeapon.eqIcon;
        else if (item.eqType == EquipmentType.ShieldEquipment)
            beforeEquipImage.sprite = selectedCrew.equippedShield.eqIcon;
        else if (item.eqType == EquipmentType.AssistantEquipment)
            beforeEquipImage.sprite = selectedCrew.equippedAssistant.eqIcon;

        afterEquipImage.sprite = item.eqIcon;

        pendingEquipItem = item;
        pendingEquipType = item.eqType;
        isPendingRemoval = false;
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
        // statComparePanel.SetActive(true);

        if (type == EquipmentType.WeaponEquipment)
        {
            beforeEquipImage.sprite = selectedCrew.equippedWeapon.eqIcon;
            afterEquipImage.sprite = EquipmentManager.Instance.defaultWeapon.eqIcon;
        }
        else if (type == EquipmentType.ShieldEquipment)
        {
            beforeEquipImage.sprite = selectedCrew.equippedShield.eqIcon;
            afterEquipImage.sprite = EquipmentManager.Instance.defaultShield.eqIcon;
        }
        else if (type == EquipmentType.AssistantEquipment)
        {
            beforeEquipImage.sprite = selectedCrew.equippedAssistant.eqIcon;
            afterEquipImage.sprite = EquipmentManager.Instance.defaultAssistant.eqIcon;
        }

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
    public void OnClickStatApplyButton()
    {
        if (selectedCrew == null || pendingEquipItem == null)
            return;

        // 장비 해제면 이전 장비를 다시 unused 목록에 추가
        if (isPendingRemoval)
        {
            EquipmentItem removed = GetCurrentEquippedByType(pendingEquipType);

            if (removed != null && !removed.isGlobalEquip && removed != EquipmentManager.Instance.defaultAssistant)
                GameManager.Instance.playerShip.unUsedItems.Add(removed);
        }
        else
        {
            EquipmentItem currentlyEquipped = GetCurrentEquippedByType(pendingEquipType);

            // 기존 장비 리스트에 추가
            if (currentlyEquipped != null
            && currentlyEquipped != EquipmentManager.Instance.GetDefaultEquipment(pendingEquipType)
            && !currentlyEquipped.isGlobalEquip)
                GameManager.Instance.playerShip.unUsedItems.Add(currentlyEquipped);

            // 새 장비 리스트에서 제거
            if (pendingEquipItem != EquipmentManager.Instance.defaultAssistant && !pendingEquipItem.isGlobalEquip)
                GameManager.Instance.playerShip.unUsedItems.Remove(pendingEquipItem);
        }

        // 실제 적용
        EquipmentManager.Instance.PurchaseAndEquipPersonal(selectedCrew, pendingEquipItem);

        // UI 갱신
        RefreshCrewDetailPanel(pendingEquipType);
        PopulateEquipmentIcons();
        slidePanelController.InitializeCrewPanel();

        // 리셋
        pendingEquipItem = null;
        pendingEquipType = EquipmentType.None;
        isPendingRemoval = false;

        midArrows.SetActive(false);
        for (int i = 0; i < 11; i++)
            changeArrows[i].enabled = false;
        // crewDetailPanel.SetActive(false);
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
    private void RefreshCrewDetailPanel(EquipmentType type)
    {
        Image detailWeaponImage = detailWeaponButton.GetComponent<Image>();
        Image detailShieldImage = detailShieldButton.GetComponent<Image>();
        Image detailAssistantImage = detailAssistButton.GetComponent<Image>();

        detailWeaponImage.sprite = selectedCrew.equippedWeapon != null ? selectedCrew.equippedWeapon.eqIcon : noneIcon;
        detailShieldImage.sprite = selectedCrew.equippedShield != null ? selectedCrew.equippedShield.eqIcon : noneIcon;
        detailAssistantImage.sprite = selectedCrew.equippedAssistant != null ? selectedCrew.equippedAssistant.eqIcon : noneIcon;

        // 좌측 갱신 (새로 적용한 장비 스탯 기준으로 출력)
        EquipmentStats currentStats = selectedCrew.GetCombinedStats();

        List<float> currentVals = new List<float>{
            currentStats.health,
            currentStats.attack,
            currentStats.defense,
            currentStats.pilotSkill,
            currentStats.engineSkill,
            currentStats.powerSkill,
            currentStats.shieldSkill,
            currentStats.weaponSkill,
            currentStats.ammunitionSkill,
            currentStats.medbaySkill,
            currentStats.repairSkill
        };

        string baseStr = "";

        for (int i = 0; i < currentVals.Count; i++)
        {
            if (i == 3)
                baseStr += "\n";

            baseStr += $"{currentVals[i]}\n";
        }

        if (type == EquipmentType.WeaponEquipment)
            beforeEquipImage.sprite = selectedCrew.equippedWeapon.eqIcon;
        else if (type == EquipmentType.ShieldEquipment)
            beforeEquipImage.sprite = selectedCrew.equippedShield.eqIcon;
        else if (type == EquipmentType.AssistantEquipment)
            beforeEquipImage.sprite = selectedCrew.equippedAssistant.eqIcon;

        baseStatsText.text = baseStr;
        newStatsText.text = "";
        statDiffText.text = "";

        // 우측 장비 image 비활성화
        afterEquipImage.enabled = false;
    }

    /// <summary>
    /// 선원 개인 디테일 panel 끄기
    /// </summary>
    public void OnClickBackPersonal()
    {
        // 스텟 비교 창 초기화
        baseStatsText.text = "";
        newStatsText.text = "";
        statDiffText.text = "";

        midArrows.SetActive(false);
        for (int i = 0; i < 11; i++)
            changeArrows[i].enabled = false;
        this.gameObject.SetActive(false);
    }
}
