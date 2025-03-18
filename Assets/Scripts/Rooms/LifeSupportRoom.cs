public class LifeSupportRoom : Room
{
    public float moraleBonus = 0f;
    public string facilityType = ""; // 오락실, 수면실, 사우나, 영화관 등

    protected override void UpdateRoom()
    {
        if (!IsOperational()) return;

        // 선원 사기 증진
        ApplyMoraleBonus();
    }

    private void ApplyMoraleBonus()
    {
        // 시설 유형에 따른 사기 보너스
        switch (facilityType)
        {
            case "Recreation":
                moraleBonus = 3f;
                break;
            case "SleepingQuarters":
                moraleBonus = 1f;
                break;
            case "Sauna":
                moraleBonus = 1f;
                break;
            case "Cinema":
                moraleBonus = 4f;
                break;
            default:
                moraleBonus = 0f;
                break;
        }

        // 함선 내 모든 선원에게 사기 보너스 적용
        Ship ship = GetComponentInParent<Ship>();
        if (ship != null) ship.ApplyMoraleBonusToAllCrew(moraleBonus);
    }
}
