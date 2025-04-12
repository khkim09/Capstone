using System;
using System.Collections.Generic;

/// <summary>
/// CrewEffect 클래스는 선원(Crew)에게 적용되는 효과를 나타냅니다.
/// 이 효과는 선원의 건강 변화 및 상태 효과와 함께, 효과의 유형과 대상 종족을 포함합니다.
/// </summary>
[Serializable]
public class CrewEffect
{
    /// <summary>
    /// 효과의 유형을 나타냅니다.
    /// </summary>
    public CrewEffectType effectType;
    /// <summary>
    /// 효과가 적용될 선원의 종족을 나타냅니다.
    /// </summary>
    public CrewRace raceType;
    /// <summary>
    /// 이 효과로 인해 선원의 건강에 가해지는 변화 값입니다.
    /// 양수이면 치유 효과, 음수이면 피해 효과를 의미할 수 있습니다.
    /// </summary>
    public float healthChange;
    /// <summary>
    /// 숙련도 종류를 의미합니다.
    /// None을 누르면 모둔 스킬(구현 예정)을 의미합니다.
    /// </summary>
    public SkillType skill;
    /// <summary>
    /// 이 효과로 인해 선원의 숙련도에 가해지는 변화 값입니다.
    /// 양수이면 숙련도 증가, 음수이면 숙련도 감소를 의미할 수 있습니다..
    /// </summary>
    public float skillChange;
    /// <summary>
    /// 이 효과로 인해 선원의 사기에 가해지는 변화 값입니다.
    /// 양수이면 사기 증가, 음수이면 사기 감소를 의미할 수 있습니다.
    /// </summary>
    public MoraleEffect moralChange;
    /// <summary>
    /// 효과 적용 후 선원의 상태를 나타냅니다.
    /// </summary>
    public CrewStatus statusEffect;
}

/// <summary>
/// CrewEffectType 열거형은 선원에게 적용될 효과의 다양한 유형을 정의합니다.
/// </summary>
public enum CrewEffectType
{
    /// <summary>
    /// 공격을 가하는 효과입니다.
    /// </summary>
    Hit,
    /// <summary>
    /// 피해를 입는 효과입니다.
    /// </summary>
    Damage,
    /// <summary>
    /// 선원을 치유하는 효과입니다.
    /// </summary>
    Heal,
    /// <summary>
    /// 선원의 상태를 변화시키는 효과입니다.
    /// </summary>
    StatusChange,
    /// <summary>
    /// 선원을 제거(살해)하는 효과입니다.
    /// </summary>
    Kill,
    /// <summary>
    /// 새로운 선원을 추가하는 효과입니다.
    /// </summary>
    AddCrew,
    /// <summary>
    /// 선원을 제거하는 효과입니다.
    /// </summary>
    RemoveCrew
}

/// <summary>
/// CrewStatus 열거형은 선원의 현재 상태를 정의합니다.
/// 선원의 상태는 전투 능력이나 기타 게임 내 효과에 영향을 줄 수 있습니다.
/// </summary>
public enum CrewStatus
{
    /// <summary>
    /// 선원이 정상 상태임을 나타냅니다.
    /// </summary>
    Normal,
    /// <summary>
    /// 선원이 부상당한 상태임을 나타냅니다.
    /// </summary>
    Injured,
    /// <summary>
    /// 선원이 병에 걸린 상태임을 나타냅니다.
    /// </summary>
    Sick,
    /// <summary>
    /// 선원이 미친 상태임을 나타냅니다.
    /// </summary>
    Insane
}
