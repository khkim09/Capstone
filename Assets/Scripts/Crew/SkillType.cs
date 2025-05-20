using System;

/// <summary>
/// 선원이 보유할 수 있는 다양한 숙련도(스킬) 타입을 정의합니다.
/// 각 스킬은 특정 방 작업이나 행동에 영향을 미칩니다.
/// </summary>
[Serializable]
public enum SkillType
{
    /// <summary>해당 없음 (기본값).</summary>
    None,

    /// <summary>조종 숙련도 – 조종석(Cockpit)에서 효과를 발휘합니다.</summary>
    PilotSkill,

    /// <summary>엔진 숙련도 – 엔진실(Engine)에서 효과를 발휘합니다.</summary>
    EngineSkill,

    /// <summary>전력 숙련도 – 전력실(PowerRoom)에서 효과를 발휘합니다.</summary>
    PowerSkill,

    /// <summary>배리어 숙련도 – 배리어실(ShieldRoom)에서 효과를 발휘합니다.</summary>
    ShieldSkill,

    /// <summary>무기 숙련도 – 무기 제어실(WeaponControlRoom)에서 효과를 발휘합니다.</summary>
    WeaponSkill,

    /// <summary>탄약 숙련도 – 탄약고(AmmunitionRoom)에서 효과를 발휘합니다.</summary>
    AmmunitionSkill,

    /// <summary>의무 숙련도 – 의무실(MedBayRoom)에서 치료 속도 등에 영향을 줍니다.</summary>
    MedBaySkill,

    /// <summary>수리 숙련도 – 파손된 시설 수리 시 성능을 향상시킵니다.</summary>
    RepairSkill
}
