public class CrewQuartersRoom : Room
{
    public float moraleBonus = 0f;
    public int maxCrewCapacity = 1;

    protected override void UpdateRoom()
    {
        if (!isActive) return; // 숙소는 전력 없이도 작동 가능

        // 선원 사기 증진
        ApplyMoraleBonus();
    }

    private void ApplyMoraleBonus()
    {
        // 레벨에 따른 사기 보너스
        switch (currentLevel)
        {
            case 1: // 일반 생활관
                moraleBonus = 0;
                maxCrewCapacity = 6;
                break;
            case 2: // 큰 생활관
                moraleBonus = 0;
                maxCrewCapacity = 10;
                break;
            case 3: // 개인 생활관
                moraleBonus = 1;
                maxCrewCapacity = 1;
                break;
            case 4: // 호화 생활관
                moraleBonus = 3;
                maxCrewCapacity = 1;
                break;
            default:
                moraleBonus = 0;
                maxCrewCapacity = 1;
                break;
        }

        // 배정된 선원에게 사기 보너스 적용
        foreach (CrewMember crew in crewInRoom) crew.AddMoraleBonus(moraleBonus);
    }
}
