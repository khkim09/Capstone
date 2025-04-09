/// <summary>
/// 선원의 종족 타입을 정의합니다.
/// 종족마다 체력, 공격력, 산소 필요 여부 등 고유 특성이 존재합니다.
/// </summary>
public enum CrewRace
{
    /// <summary>미설정 상태 또는 기본값.</summary>
    None,

    /// <summary>인간 종족. 균형 잡힌 능력치를 가집니다.</summary>
    Human,

    /// <summary>부정형 생명체. 산소가 필요 없고 체력 회복이 빠릅니다.</summary>
    Amorphous,

    /// <summary>기계형 탱커. 높은 체력과 방어력을 가집니다.</summary>
    MechanicTank,

    /// <summary>기계형 서포터. 에너지 효율과 수리 능력이 우수합니다.</summary>
    MechanicSup,

    /// <summary>야수형 종족. 강한 공격력과 빠른 이동 속도를 가집니다.</summary>
    Beast,

    /// <summary>곤충형 종족. 작은 크기와 높은 회피율이 특징입니다.</summary>
    Insect
}
