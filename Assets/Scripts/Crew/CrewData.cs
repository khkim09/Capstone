[System.Serializable]
public class CrewData
{
    public int 공격력;
    public int 방어력;
    public int 체력;
    public float 학습속도;
    // JSON 키 "산소 호흡" → "산소호흡" (공백 제거)
    public string 산소호흡;
    public int 조종실숙련도;
    public int 엔진실숙련도;
    public int 전력실숙련도;
    public int 배리어실숙련도;
    public int 조준석숙련도;
    public int 탄약고숙련도;
    public int 의무실숙련도;
    // 일부 항목은 빈 문자열이 들어올 수 있으므로 string 타입으로 처리
    public string 수리숙련도;
}
