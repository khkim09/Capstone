using UnityEngine;

public class ShieldRoom : Room
{
    private const float RECHARGE_TIME = 2f; // 2초마다 충전

    private readonly int[] maxShieldsPerLevel = { 0, 1, 2 }; // 각 레벨별 최대 쉴드

    public int currentShields;

    private float shieldRechargeTimer;

    public override void Update()
    {
        base.Update();

        if (!isPowered) return;


        shieldRechargeTimer += Time.deltaTime;
        if (shieldRechargeTimer >= RECHARGE_TIME)
        {
            shieldRechargeTimer = 0f;
            if (currentShields < maxShieldsPerLevel[currentPowerLevel]) currentShields++;
        }

        GetCrewCount();
    }

    protected override void ApplyCurrentLevelEffects()
    {
        // 현재 레벨에 맞는 최대 쉴드 설정
        if (currentShields > maxShieldsPerLevel[currentPowerLevel])
            currentShields = maxShieldsPerLevel[currentPowerLevel];
    }

    public void TakeShieldDamage(int damage)
    {
        currentShields = Mathf.Max(0, currentShields - damage);
    }
}
