using UnityEngine;

public class ShieldSystem : ShipSystem
{
    private float currentShield;
    private float shieldRespawnTimer = 0f; // 쉴드 리스폰 타이머
    private bool isShieldDestroyed = false; // 쉴드 파괴 상태

    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);
        currentShield = GetShipStat(ShipStat.ShieldMaxAmount);
    }

    public override void Update(float deltaTime)
    {
        // 쉴드가 파괴된 상태이면 리스폰 타이머 감소
        if (isShieldDestroyed)
        {
            shieldRespawnTimer -= deltaTime;

            // 타이머가 0 이하가 되면 쉴드 즉시 최대치로 복구
            if (shieldRespawnTimer <= 0)
            {
                isShieldDestroyed = false;
                currentShield = GetShipStat(ShipStat.ShieldMaxAmount); // 즉시 최대치로 복구
                Debug.Log("Shield system reactivated at full capacity");
            }
        }
        // 쉴드가 파괴된 상태가 아니고 최대치보다 작으면 재생성
        else if (currentShield < GetShipStat(ShipStat.ShieldMaxAmount))
        {
            RegenerateShield(deltaTime);
        }
    }

    private void RegenerateShield(float deltaTime)
    {
        float regenRate = GetShipStat(ShipStat.ShieldRegeneratePerSecond);
        currentShield += regenRate * deltaTime;
        currentShield = Mathf.Min(currentShield, GetShipStat(ShipStat.ShieldMaxAmount));
    }

    public float GetCurrentShield()
    {
        return currentShield;
    }

    public float GetMaxShield()
    {
        return GetShipStat(ShipStat.ShieldMaxAmount);
    }

    public float TakeDamage(float damage)
    {
        float afterShieldDamage;
        if (currentShield > damage)
        {
            afterShieldDamage = 0;
            currentShield -= damage;
        }
        else
        {
            afterShieldDamage = damage - currentShield;
            currentShield = 0;
            isShieldDestroyed = true;
            shieldRespawnTimer = GetShipStat(ShipStat.ShieldRespawnTime);
        }

        return afterShieldDamage;
    }

    public bool IsShieldActive()
    {
        return !isShieldDestroyed && currentShield > 0;
    }
}
