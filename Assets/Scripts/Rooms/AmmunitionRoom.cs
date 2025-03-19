public class AmmunitionRoom : Room
{
    public float weaponDamageBonus = 0f;
    public float reloadSpeedBonus = 0f;

    protected override void UpdateRoom()
    {
        if (!IsOperational()) return;

        // 무기 피해량 보너스 계산
        CalculateDamageBonus();
        // 재장전 속도 보너스 계산
        CalculateReloadSpeedBonus();
    }

    private void CalculateDamageBonus()
    {
        // 기본 피해량 보너스
        float baseBonus = 0f;

        // 레벨에 따른 보너스 조정
        switch (currentPowerLevel)
        {
            case 1:
                baseBonus = 0f;
                break;
            case 2:
                baseBonus = 10f;
                break;
        }

        // 선원 숙련도 반영
        float crewBonus = 0f;
        foreach (CrewMember crew in crewInRoom) crewBonus += crew.GetSkillLevel(SkillType.WeaponSkill) * 0.5f;

        weaponDamageBonus = baseBonus + crewBonus;

        // 방 효율(체력) 반영
        weaponDamageBonus *= efficiency;
    }

    private void CalculateReloadSpeedBonus()
    {
        // 기본 재장전 속도 보너스
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

        reloadSpeedBonus = baseBonus + crewBonus;

        // 방 효율(체력) 반영
        reloadSpeedBonus *= efficiency;
    }
}
