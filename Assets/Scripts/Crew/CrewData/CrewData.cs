/// <summary>
/// 크루원(승무원)의 능력치 및 숙련도 정보를 담는 데이터 클래스.
/// 저장 및 로드 시 JSON 직렬화에 사용됩니다.
/// </summary>
[System.Serializable]
public class CrewData
{
    /// <summary>
    /// 크루원의 공격력 수치입니다.
    /// </summary>
    public int 공격력;

    /// <summary>
    /// 크루원의 방어력 수치입니다.
    /// </summary>
    public int 방어력;

    /// <summary>
    /// 크루원의 체력 수치입니다.
    /// </summary>
    public int 체력;

    /// <summary>
    /// 스탯 증가 속도 등 학습 관련 수치입니다.
    /// </summary>
    public float 학습속도;

    /// <summary>
    /// 산소 부족 환경에서 생존 가능한 능력입니다.
    /// JSON 키 "산소 호흡"에서 공백이 제거된 형태로 처리됩니다.
    /// </summary>
    public string 산소호흡;

    /// <summary>
    /// 조종실에서의 작업 능력 숙련도입니다.
    /// </summary>
    public int 조종실숙련도;

    /// <summary>
    /// 엔진실에서의 작업 능력 숙련도입니다.
    /// </summary>
    public int 엔진실숙련도;

    /// <summary>
    /// 전력실에서의 작업 능력 숙련도입니다.
    /// </summary>
    public int 전력실숙련도;

    /// <summary>
    /// 배리어실(쉴드룸)에서의 작업 능력 숙련도입니다.
    /// </summary>
    public int 배리어실숙련도;

    /// <summary>
    /// 조준석에서의 작업 능력 숙련도입니다.
    /// </summary>
    public int 조준석숙련도;

    /// <summary>
    /// 탄약고에서의 작업 능력 숙련도입니다.
    /// </summary>
    public int 탄약고숙련도;

    /// <summary>
    /// 의무실(회복실)에서의 작업 능력 숙련도입니다.
    /// </summary>
    public int 의무실숙련도;

    /// <summary>
    /// 수리 작업에 대한 숙련도입니다.
    /// 일부 데이터는 빈 문자열일 수 있으므로 string 타입으로 처리됩니다.
    /// </summary>
    public string 수리숙련도;
}
