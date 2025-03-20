public class WeaponControlRoom : Room
{
    public float accuracyBonus = 0f;

    protected override void UpdateRoom()
    {
        if (!IsOperational()) return;

        // 무기 명중률 보너스 계산
        CalculateAccuracyBonus();
    }

    private void CalculateAccuracyBonus()
    {
        // 기본 명중률 보너스
        float baseBonus = 0f;

        // 레벨에 따른 보너스 조정
        switch (currentPowerLevel)
        {
            case 1:
                baseBonus = 10f;
                break;
            case 2:
                baseBonus = 20f;
                break;
        }

        // 선원 숙련도 반영
        float crewBonus = 0f;
        foreach (CrewMember crew in crewInRoom) crewBonus += crew.GetSkillLevel(SkillType.WeaponSkill) * 0.5f;

        accuracyBonus = baseBonus + crewBonus;
    }
}
