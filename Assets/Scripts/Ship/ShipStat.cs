/// <summary>
/// 배의 스탯을 나타내는 Enum
/// 주의해야할 점 : 실시간 값은 ship 객체에서 관리한다.
/// 다만 방의 개별 전력 사용량을 합친 사용 중인 PowerUsing은 예외적으로 ShipStat 에서 관리하는 것을 허용한다.
/// 그 이유는 확장성과 유지보수를 위해서, 그리고 각 방에서 Contribute를 모아 계산하기 용이하다는 점에 있다.
/// TODO: 실시간 산소량도 ShipStat 에서 두는 것이 좋을 것 같다. 이유는 실시간 사용 전력량과 같다. 산소 호흡을 하는 선원들은 초당 1% 의 산소를 소모하므로 ShipStat 관리하면
///       선원에게 IShipStatContributor 인터페이스를 달고 쉽게 구현 가능하다.
/// 실시간 체력, 실시간 방어막량 등은 Ship에 둘 것. 물론 실시간 값들을 모아서 ShipSystem, ShipState 등 클래스로 관리하는 것도 괜찮을 것 같다.
/// </summary>
public enum ShipStat
{
    DodgeChance, // 회피율 (%)
    HitPointsMax, // 선체 최대 체력
    FuelEfficiency, // 연료 효율 (%)
    FuelConsumption, // 연료 사용량
    ShieldMaxAmount, // 방어막 최대 용량
    OxygenGeneratePerSecond, // 초당 산소 생산량
    OxygenUsingPerSecond, // 초당 산소 소모량 (산소 호흡을 하는 선원 당 1%, 추후 방이 망가져서 구멍이 뚫린 경우 등도 계산 필요?)
    PowerUsing, // 사용 중인 파워량
    PowerCapacity, // 사용 가능한 파워량 = 전력실의 파워 생산량
    ShieldRespawnTime, // 쉴드 리스폰 쿨타임
    ShieldRegeneratePerSecond, // 초당 쉴드 재생량
    HealPerSecond, // 의무실에서 초당 회복량
    CrewCapacity, // 선원 최대 고용수
    DamageReduction, // 함선 무기 피격 데미지 경감률
    Accuracy, // 명중률 (%)
    ReloadTimeBonus, // 재장전 시간 보너스
    DamageBonus, // 피해량 보너스
    CrewMoraleBonus // 전체 적용 사기 보너스
}
