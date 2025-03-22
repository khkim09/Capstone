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
    }

    private void CalculateReloadSpeedBonus()
    {
    }
}
