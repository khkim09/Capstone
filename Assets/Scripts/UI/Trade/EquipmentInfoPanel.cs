using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class EquipmentInfoPanel : TooltipPanelBase
{
    [Header("UI References")] [SerializeField]
    private TextMeshProUGUI equipmentNameText;

    [SerializeField] public Button panelButton;

    [SerializeField] private Image iconImage;

    [SerializeField] private List<Image> skillIconImages;
    [SerializeField] private List<TextMeshProUGUI> skillEffectTexts;

    [SerializeField] private List<SkillSpritePair> skillIconSprites;
    private Dictionary<SkillType, Sprite> dict;

    [SerializeField] private Sprite attackSprite;
    [SerializeField] private Sprite defenseSprite;
    [SerializeField] private Sprite hitpointSprite;

    private EquipmentItem currentEquipmentItem;

    public EquipmentItem CurrentEquipmentItem
    {
        get => currentEquipmentItem;
        set => currentEquipmentItem = value;
    }

    private TradeUIController tradeUIController;

    private void Awake()
    {
        dict = new Dictionary<SkillType, Sprite>();
        foreach (SkillSpritePair pair in skillIconSprites) dict[pair.key] = pair.value;
    }

    private void Start()
    {
        base.Start();

        tradeUIController = GameObject.FindWithTag("TradeUIController").GetComponent<TradeUIController>();
    }

    public void Initialize(EquipmentItem equipmentItem)
    {
        foreach (Image icon in skillIconImages) icon.gameObject.SetActive(false);
        foreach (TextMeshProUGUI effect in skillEffectTexts) effect.text = "";
        int currentIndex = 0;

        currentEquipmentItem = equipmentItem;
        equipmentNameText.text = currentEquipmentItem.eqName.Localize();
        iconImage.sprite = currentEquipmentItem.eqIcon;

        if (equipmentItem.eqAttackBonus != 0)
        {
            skillIconImages[currentIndex].gameObject.SetActive(true);
            skillEffectTexts[currentIndex].gameObject.SetActive(true);
            skillIconImages[currentIndex].sprite = attackSprite;
            skillEffectTexts[currentIndex].text = currentEquipmentItem.eqAttackBonus.ToString();
            currentIndex++;
        }

        if (equipmentItem.eqDefenseBonus != 0)
        {
            skillIconImages[currentIndex].gameObject.SetActive(true);
            skillEffectTexts[currentIndex].gameObject.SetActive(true);
            skillIconImages[currentIndex].sprite = defenseSprite;
            skillEffectTexts[currentIndex].text = currentEquipmentItem.eqDefenseBonus.ToString();
            currentIndex++;
        }

        if (equipmentItem.eqHealthBonus != 0)
        {
            skillIconImages[currentIndex].gameObject.SetActive(true);
            skillEffectTexts[currentIndex].gameObject.SetActive(true);
            skillIconImages[currentIndex].sprite = hitpointSprite;
            skillEffectTexts[currentIndex].text = currentEquipmentItem.eqHealthBonus.ToString();
            currentIndex++;
        }

        if (equipmentItem.eqAdditionalPilotSkill != 0)
        {
            skillIconImages[currentIndex].gameObject.SetActive(true);
            skillEffectTexts[currentIndex].gameObject.SetActive(true);
            skillIconImages[currentIndex].sprite = GetSkillSprite(SkillType.PilotSkill);
            skillEffectTexts[currentIndex].text = currentEquipmentItem.eqAdditionalPilotSkill.ToString();
            currentIndex++;
        }

        if (equipmentItem.eqAdditionalEngineSkill != 0)
        {
            skillIconImages[currentIndex].gameObject.SetActive(true);
            skillEffectTexts[currentIndex].gameObject.SetActive(true);
            skillIconImages[currentIndex].sprite = GetSkillSprite(SkillType.EngineSkill);
            skillEffectTexts[currentIndex].text = currentEquipmentItem.eqAdditionalEngineSkill.ToString();
            currentIndex++;
        }

        if (equipmentItem.eqAdditionalPowerSkill != 0)
        {
            skillIconImages[currentIndex].gameObject.SetActive(true);
            skillEffectTexts[currentIndex].gameObject.SetActive(true);
            skillIconImages[currentIndex].sprite = GetSkillSprite(SkillType.PowerSkill);
            skillEffectTexts[currentIndex].text = currentEquipmentItem.eqAdditionalPowerSkill.ToString();
            currentIndex++;
        }

        // TODO : 확장성 있는 구조를 위해 나중에 4개보다 더 많은 능력치를 표현할 수 있게 수정하는 것도 좋다.
        if (currentIndex > 3)
            return;

        if (equipmentItem.eqAdditionalShieldSkill != 0)
        {
            skillIconImages[currentIndex].gameObject.SetActive(true);
            skillEffectTexts[currentIndex].gameObject.SetActive(true);
            skillIconImages[currentIndex].sprite = GetSkillSprite(SkillType.ShieldSkill);
            skillEffectTexts[currentIndex].text = currentEquipmentItem.eqAdditionalShieldSkill.ToString();
            currentIndex++;
        }

        if (currentIndex > 3)
            return;

        if (equipmentItem.eqAdditionalWeaponSkill != 0)
        {
            skillIconImages[currentIndex].gameObject.SetActive(true);
            skillEffectTexts[currentIndex].gameObject.SetActive(true);
            skillIconImages[currentIndex].sprite = GetSkillSprite(SkillType.WeaponSkill);
            skillEffectTexts[currentIndex].text = currentEquipmentItem.eqAdditionalWeaponSkill.ToString();
            currentIndex++;
        }

        if (currentIndex > 3)
            return;

        if (equipmentItem.eqAdditionalAmmunitionSkill != 0)
        {
            skillIconImages[currentIndex].gameObject.SetActive(true);
            skillEffectTexts[currentIndex].gameObject.SetActive(true);
            skillIconImages[currentIndex].sprite = GetSkillSprite(SkillType.AmmunitionSkill);
            skillEffectTexts[currentIndex].text = currentEquipmentItem.eqAdditionalAmmunitionSkill.ToString();
            currentIndex++;
        }

        if (currentIndex > 3)
            return;

        if (equipmentItem.eqAdditionalMedBaySkill != 0)
        {
            skillIconImages[currentIndex].gameObject.SetActive(true);
            skillEffectTexts[currentIndex].gameObject.SetActive(true);
            skillIconImages[currentIndex].sprite = GetSkillSprite(SkillType.MedBaySkill);
            skillEffectTexts[currentIndex].text = currentEquipmentItem.eqAdditionalMedBaySkill.ToString();
            currentIndex++;
        }

        if (currentIndex > 3)
            return;

        if (equipmentItem.eqAdditionalRepairSkill != 0)
        {
            skillIconImages[currentIndex].gameObject.SetActive(true);
            skillEffectTexts[currentIndex].gameObject.SetActive(true);
            skillIconImages[currentIndex].sprite = GetSkillSprite(SkillType.RepairSkill);
            skillEffectTexts[currentIndex].text = currentEquipmentItem.eqAdditionalRepairSkill.ToString();
            currentIndex++;
        }
    }

    public Sprite GetSkillSprite(SkillType skill)
    {
        return dict.TryGetValue(skill, out Sprite sprite) ? sprite : null;
    }

    protected override void SetToolTipText()
    {
        if (currentEquipmentItem == null || currentToolTip == null) return;

        TextMeshProUGUI toolTipText = currentToolTip.GetComponentInChildren<TextMeshProUGUI>();

        toolTipText.text = currentEquipmentItem.eqDescription.Localize();
    }

    public void HideTooltip()
    {
        currentToolTip.SetActive(false);
    }
}

[Serializable]
public class SkillSpritePair
{
    public SkillType key;
    public Sprite value;
}

// 바꾸기 전
