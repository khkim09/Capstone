/// <summary>
/// 크루 유형별 스탯 데이터를 저장하는 래퍼 클래스.
/// JSON 구조 상 최상단에 위치하며, 각 유형별로 CrewData를 포함합니다.
/// </summary>
[System.Serializable]
public class CrewInfoWrapper
{
    /// <summary>
    /// 인간형 크루의 기본 스탯 데이터입니다.
    /// </summary>
    public CrewData 인간형;

    /// <summary>
    /// 부정형(언데드 등) 크루의 기본 스탯 데이터입니다.
    /// </summary>
    public CrewData 부정형;

    /// <summary>
    /// 돌격 기계형 크루의 기본 스탯 데이터입니다.
    /// </summary>
    public CrewData 돌격기계형;

    /// <summary>
    /// 지원 기계형 크루의 기본 스탯 데이터입니다.
    /// </summary>
    public CrewData 지원기계형;

    /// <summary>
    /// 짐승형(야수형) 크루의 기본 스탯 데이터입니다.
    /// </summary>
    public CrewData 짐승형;

    /// <summary>
    /// 곤충형 크루의 기본 스탯 데이터입니다.
    /// </summary>
    public CrewData 곤충형;
}
