/// <summary>
/// 배의 스탯을 정의하는 열거형(enum)입니다.
/// </summary>
public enum ShipStat
{
    /// <summary>회피율 (%)</summary>
    DodgeChance,

    /// <summary>선체 최대 체력</summary>
    HitPointsMax,

    /// <summary>연료 효율 (%)</summary>
    FuelEfficiency,

    /// <summary>연료 사용량</summary>
    FuelConsumption,

    /// <summary>방어막 최대 용량</summary>
    ShieldMaxAmount,

    /// <summary>초당 산소 생산량</summary>
    OxygenGeneratePerSecond,

    /// <summary>
    /// 초당 산소 소모량
    /// (예: 선원당 1%, 향후 방 손상 시 누출 등 추가 고려 가능)
    /// </summary>
    OxygenUsingPerSecond,

    /// <summary>현재 사용 중인 파워량</summary>
    PowerUsing,

    /// <summary>사용 가능한 총 파워량 (전력실의 생산량)</summary>
    PowerCapacity,

    /// <summary>쉴드 리스폰 쿨타임 (초)</summary>
    ShieldRespawnTime,

    /// <summary>초당 쉴드 재생량</summary>
    ShieldRegeneratePerSecond,

    /// <summary>초당 체력 회복량 (의무실 등)</summary>
    HealPerSecond, // 의무실에서 초당 회복량

    /// <summary>선원 최대 고용 수</summary>
    CrewCapacity, // 선원 최대 고용수

    /// <summary>무기 피격 시 피해 감소율 (%)</summary>
    DamageReduction,

    /// <summary>무기 명중률 (%)</summary>
    Accuracy,

    /// <summary>재장전 시간 보너스 (%)
    /// (값이 높을수록 쿨타임 감소)</summary>
    ReloadTimeBonus,

    /// <summary>무기 피해량 보너스 (%)</summary>
    DamageBonus,

    /// <summary>전체 사기(Morale)에 영향을 주는 보너스</summary>
    CrewMoraleBonus
}
