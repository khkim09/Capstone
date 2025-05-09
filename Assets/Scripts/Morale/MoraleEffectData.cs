/// <summary>
/// 사기 효과에 대한 데이터만 담는 클래스
/// </summary>
[System.Serializable]
public class MoraleEffectData
{
    /// <summary>
    /// 대상 종족 (None 이면 전체)
    /// </summary>
    public CrewRace targetRace;

    /// <summary>
    /// 적용되는 사기 값
    /// </summary>
    public int value;

    /// <summary>
    /// 지속 년도
    /// </summary>
    public int duration;

    /// <summary>
    /// 시작 연도
    /// </summary>
    public int startYear;

    /// <summary>
    /// 끝나는 년도 프로퍼티
    /// </summary>
    public int EndYear => startYear + duration;

    // 효과 출처에 대한 설명 (오버레이에 표시할 용도)
    public string source = "";

    public MoraleEffectData(CrewRace race, int value, int startYear, int duration, string source = "")
    {
        targetRace = race;
        this.value = value;
        this.startYear = startYear;
        this.duration = duration;
        this.source = source;
    }
}
