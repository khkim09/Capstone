public class CockpitRoom : Room
{
    public float dodgeRate = 0f;
    public float fuelEfficiency = 0f;

    protected override void UpdateRoom()
    {
        if (!IsOperational()) return;

        // 선원 숙련도 및 레벨에 따른 회피율 계산
        CalculateDodgeRate();
        // 선원 숙련도 및 레벨에 따른 연료 효율 계산
        CalculateFuelEfficiency();
    }

    private void CalculateDodgeRate()
    {
        // 기본 회피율
        float baseRate = 0f;

        // 레벨에 따른 회피율 조정
        switch (currentPowerLevel)
        {
            case 1:
                baseRate = 5f;
                break;
            case 2:
                baseRate = 10f;
                break;
            case 3:
                baseRate = 15f;
                break;
        }

        // 선원 숙련도 반영
        float crewBonus = 0f;
        foreach (CrewMember crew in crewInRoom) crewBonus += crew.GetSkillLevel(SkillType.Piloting) * 0.5f;

        dodgeRate = baseRate + crewBonus;

        // 방 효율(체력) 반영
        dodgeRate *= efficiency;
    }

    private void CalculateFuelEfficiency()
    {
        // 기본 연료 효율
        float baseEfficiency = 0f;

        // 레벨에 따른 효율 조정
        switch (currentPowerLevel)
        {
            case 1:
                baseEfficiency = 2f;
                break;
            case 2:
                baseEfficiency = 5f;
                break;
            case 3:
                baseEfficiency = 10f;
                break;
        }

        // 선원 숙련도 반영
        float crewBonus = 0f;
        foreach (CrewMember crew in crewInRoom) crewBonus += crew.GetSkillLevel(SkillType.Piloting) * 0.2f;

        fuelEfficiency = baseEfficiency + crewBonus;

        // 방 효율(체력) 반영
        fuelEfficiency *= efficiency;
    }
}
