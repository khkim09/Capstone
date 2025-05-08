using System;
using System.Collections.Generic;
using UnityEngine;

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
    /// 효과가 적용될 선원의 종족을 나타냅니다. None 은 All
    /// </summary>
    public CrewRace raceType;

    /// <summary>
    /// 체력, 숙련도, 사기가 변하는 량
    /// </summary>
    public float changeAmount;

    /// <summary>
    /// 숙련도 종류를 의미합니다.
    /// None을 누르면 모둔 스킬(구현 예정)을 의미합니다.
    /// </summary>
    public SkillType skill;

    // TODO : 상태이상 종류를 의미합니다.

    /// <summary>
    /// 이벤트 지속 조건. 탈출 조건.
    /// </summary>
    public PersistenceCondition persistenceCondition;

    /// <summary>
    /// 이벤트 조건 관련 데이터. 특정 행성 돌입이면 행성 타입. 특정 재화를 모아야되면 재화타입 등등
    /// </summary>
    public string conditionData;

    /// <summary>
    /// 이벤트 조건 관련 양 데이터. 특정 시간이 지나야되는 이벤트면 지속 년도, 특정 재화를 모아야되면 재화의 양 등등
    /// </summary>
    public float conditionAmount;

    /// <summary>
    /// 선원에게 적용될 파티클. 여기서 할당하는 게 아니라, Amount 가 양수면 긍정 파티클, Amount 가 음수면 부정 파티클로 하는 것도 좋아보임.
    /// </summary>
    public Sprite effectIcon;
}

/// <summary>
/// CrewEffectType 열거형은 선원에게 적용될 효과의 다양한 유형을 정의합니다.
/// </summary>
public enum CrewEffectType
{
    /// <summary>
    /// 체력을 변화시키는 효과입니다.
    /// </summary>
    ChangeHealth,

    /// <summary>
    /// 사기를 변화시키는 효과입니다.
    /// </summary>
    ChangeMorale,

    /// <summary>
    /// 숙련도를 변화시키는 효과입니다.
    /// </summary>
    ChangeSkill,

    /// <summary>
    /// 선원에게 상태이상을 부여하는 효과입니다.
    /// </summary>
    ApplyStatusEffect
}

/// <summary>
/// 효과의 지속 조건을 정의합니다. 효과가 언제까지 지속될지 결정합니다.
/// </summary>
public enum PersistenceCondition
{
    /// <summary>
    /// 기간 기반 - duration 필드에 지정된 기간 동안 지속됩니다.
    /// </summary>
    Duration,

    /// <summary>
    /// 선원이 공격할 때까지 지속됩니다.
    /// </summary>
    UntilAttack,

    /// <summary>
    /// 선원이 치료실에서 회복할 때까지 지속됩니다.
    /// </summary>
    UntilHealed,

    /// <summary>
    /// 기계형 한정 수리될 때까지 지속됩니다.
    /// </summary>
    UntilRepaired,

    /// <summary>
    /// 특정 행성에 도착할 때까지 지속됩니다.
    /// </summary>
    UntilPlanetReached,

    /// <summary>
    /// 영구적으로 지속됩니다.
    /// </summary>
    Permanent,

    /// <summary>
    /// 즉시 단발성으로 발동합니다.
    /// </summary>
    Instant
}
