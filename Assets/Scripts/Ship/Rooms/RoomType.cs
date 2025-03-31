/// <summary>
/// 함선 내 방(Room)의 유형을 정의하는 열거형입니다.
/// 각 타입은 고유한 기능과 ShipStat 기여도를 가집니다.
/// </summary>
public enum RoomType
{
    /// <summary>엔진실 – 회피율, 연료 효율 등과 관련됨.</summary>
    Engine,

    /// <summary>전력실 – 전력 생산(PowerCapacity)을 담당.</summary>
    Power,

    /// <summary>배리어실 – 방어막 생성 및 재생 기능 제공.</summary>
    Shield,

    /// <summary>산소실 – 산소 생성 기능 제공.</summary>
    Oxygen,

    /// <summary>조종실 – 연료 효율 및 회피율 보너스를 제공.</summary>
    Cockpit,

    /// <summary>조준석 – 무기 명중률(Accuracy)에 기여.</summary>
    WeaponControl,

    /// <summary>탄약고 – 재장전 속도 및 데미지 보너스를 제공.</summary>
    Ammunition,

    /// <summary>의무실 – 선원 체력 회복 기능 담당.</summary>
    MedBay,

    /// <summary>창고 – 아이템 보관 용도로 사용.</summary>
    Storage,

    /// <summary>복도 – 선원이 이동하는 통로 역할.</summary>
    Corridor,

    /// <summary>선원 숙소 – 선원 수용 및 일부 사기 보너스 제공.</summary>
    CrewQuarters,

    /// <summary>생활 시설 – 사기 보너스를 제공하는 편의 공간.</summary>
    LifeSupport,

    /// <summary>텔레포터 – 선원 순간이동 기능 담당.</summary>
    Teleporter
}
