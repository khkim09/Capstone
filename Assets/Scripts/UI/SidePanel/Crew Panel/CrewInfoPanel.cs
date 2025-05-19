using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrewInfoPanel : TooltipPanelBase
{
    [SerializeField] private Button atkButton;
    [SerializeField] private Button defButton;
    [SerializeField] private Button assistButton;

    [SerializeField] private TextMeshProUGUI crewName;
    [SerializeField] private TextMeshProUGUI currentHealth;
    [SerializeField] private Image crewIcon;

    [SerializeField] private Button rtsButton;
    private SlidePanelController slidePanelController;

    private CrewMember currentCrew;

    protected override void Start()
    {
        base.Start();

        slidePanelController = GameObject.FindWithTag("SlidePanel").GetComponent<SlidePanelController>();
    }

    public void Initialize(CrewMember crew)
    {
        currentCrew = crew;
        crewName.text = crew.crewName;
        currentHealth.text = $"{crew.health}/{crew.maxHealth}";
        crewIcon.sprite = crew.spriteRenderer.sprite;

        Image atkImage = atkButton.GetComponent<Image>();
        Image defImage = defButton.GetComponent<Image>();
        Image assistImage = assistButton.GetComponent<Image>();

        if (crew.equippedWeapon != null)
            atkImage.sprite = crew.equippedWeapon.eqIcon;
        if (crew.equippedShield != null)
            defImage.sprite = crew.equippedShield.eqIcon;
        if (crew.equippedAssistant != null)
            assistImage.sprite = crew.equippedAssistant.eqIcon;

        atkButton.onClick.AddListener(() => { slidePanelController.OnCrewPanelEquipmentButtonClicked(crew); });
        defButton.onClick.AddListener(() => { slidePanelController.OnCrewPanelEquipmentButtonClicked(crew); });
        assistButton.onClick.AddListener(() => { slidePanelController.OnCrewPanelEquipmentButtonClicked(crew); });

        rtsButton.onClick.AddListener(() => { OnRTSButtonClicked(); });
    }

    private void OnRTSButtonClicked()
    {
        RTSSelectionManager.Instance.Select(currentCrew);
    }

    // 부모 클래스의 추상 메서드 구현
    protected override void SetToolTipText()
    {
        if (currentCrew == null || currentToolTip == null) return;

        TextMeshProUGUI toolTipText = currentToolTip.GetComponentInChildren<TextMeshProUGUI>();
        if (toolTipText != null)
            toolTipText.text =
                $"{"ui.crewinfo.attack".Localize()} {currentCrew.attack} {"ui.crewinfo.defense".Localize()} {currentCrew.defense}\n"
                + $"<{"ui.crewinfo.skill".Localize()}>\n"
                + $"{"room.roomtype.cockpit".Localize()} {currentCrew.skills[SkillType.PilotSkill]}/{currentCrew.maxPilotSkillValue}\n"
                + $"{"room.roomtype.engine".Localize()} {currentCrew.skills[SkillType.EngineSkill]}/{currentCrew.maxEngineSkillValue}\n"
                + $"{"room.roomtype.power".Localize()} {currentCrew.skills[SkillType.PowerSkill]}/{currentCrew.maxPowerSkillValue}\n"
                + $"{"room.roomtype.shield".Localize()} {currentCrew.skills[SkillType.ShieldSkill]}/{currentCrew.maxShieldSkillValue}\n"
                + $"{"room.roomtype.weaponcontrol".Localize()} {currentCrew.skills[SkillType.WeaponSkill]}/{currentCrew.maxWeaponSkillValue}\n"
                + $"{"room.roomtype.ammunition".Localize()} {currentCrew.skills[SkillType.AmmunitionSkill]}/{currentCrew.maxAmmunitionSkillValue}\n"
                + $"{"room.roomtype.medbay".Localize()} {currentCrew.skills[SkillType.MedBaySkill]}/{currentCrew.maxMedBaySkillValue}\n"
                + $"{"crew.skilltype.repairshort".Localize()} {currentCrew.skills[SkillType.RepairSkill]}/{currentCrew.maxRepairSkillValue}";
    }

    public void RefreshCrewEquipments(CrewMember crew)
    {
        Debug.LogError("갱신");
        Image atkImage = atkButton.GetComponent<Image>();
        Image defImage = defButton.GetComponent<Image>();
        Image assistImage = assistButton.GetComponent<Image>();

        if (crew.equippedWeapon != null)
        {
            Debug.LogError($"weapon : {crew.equippedWeapon}");
            atkImage.sprite = crew.equippedWeapon.eqIcon;
        }
        else
            Debug.LogError("장착무기 null");
        if (crew.equippedShield != null)
            defImage.sprite = crew.equippedShield.eqIcon;
        if (crew.equippedAssistant != null)
            assistImage.sprite = crew.equippedAssistant.eqIcon;
    }
}
