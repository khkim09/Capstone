using UnityEngine;

/// <summary>
/// 장비 타입 - 무기, 방어구, 보조 장치
/// </summary>
public enum EquipmentType
{
    WeaponEquipment, // 무기 - 모든 선원 적용
    ShieldEquipment, // 방어구 - 모든 선원 적용
    AssistantEquipment // 보조 장치 - 개별 선원 적용
}

/// <summary>
/// 각 장비 별 필요한 필드 정리
/// </summary>
[CreateAssetMenu(fileName = "EquipmentItem", menuName = "Equipment/EquipmentItem")]
public class EquipmentItem : ScriptableObject
{
    [Header("Basic Info")] public int eqId;
    public Sprite eqIcon;
    public string eqName;
    public EquipmentType eqType;
    public int eqPrice;
    public bool isGlobalEquip;

    // 장비가 부여하는 능력치 보너스 (필요한 값들을 추가)
    [Header("Attack/Defense Bonus")] public float eqAttackBonus;
    public float eqDefenseBonus;
    public float eqHealthBonus;

    // 보조장치의 경우, 숙련도 최대치 상승 등 추가 효과도 가능
    [Header("Assistant Bonus")] public float eqAdditionalPilotSkill;
    public float eqAdditionalEngineSkill;
    public float eqAdditionalPowerSkill;
    public float eqAdditionalShieldSkill;
    public float eqAdditionalWeaponSkill;
    public float eqAdditionalAmmunitionSkill;
    public float eqAdditionalMedBaySkill;
    public float eqAdditionalRepairSkill;
}
