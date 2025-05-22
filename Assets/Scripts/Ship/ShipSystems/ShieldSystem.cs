using UnityEngine;

/// <summary>
/// 함선의 방어막(쉴드)을 관리하는 시스템.
/// 쉴드의 피해 처리, 재생, 파괴 및 재활성화 타이머를 다룹니다.
/// </summary>
public class ShieldSystem : ShipSystem
{
    /// <summary>
    /// 현재 쉴드 수치입니다.
    /// </summary>
    private float currentShield;

    /// <summary>
    /// 쉴드가 파괴된 후 재활성화까지 남은 시간입니다.
    /// </summary>
    private float shieldRespawnTimer = 0f;

    /// <summary>
    /// 쉴드가 파괴된 상태인지 여부입니다.
    /// </summary>
    private bool isShieldDestroyed = false;

    /// <summary>
    /// 시스템을 초기화하고 쉴드를 최대치로 설정합니다.
    /// </summary>
    /// <param name="ship">초기화할 대상 함선 객체.</param>
    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);
        Refresh();
    }

    public override void Refresh()
    {
        currentShield = GetShipStat(ShipStat.ShieldMaxAmount);
    }

    /// <summary>
    /// 매 프레임마다 호출되어 쉴드 상태를 갱신합니다.
    /// 파괴 상태일 경우 재활성화 타이머를 감소시키고,
    /// 파괴되지 않은 상태에서는 자동으로 쉴드를 재생성합니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
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

    /// <summary>
    /// 쉴드를 재생성합니다. 초당 재생 속도에 따라 현재 쉴드를 회복합니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
    private void RegenerateShield(float deltaTime)
    {
        float regenRate = GetShipStat(ShipStat.ShieldRegeneratePerSecond);
        currentShield += regenRate * deltaTime;
        currentShield = Mathf.Min(currentShield, GetShipStat(ShipStat.ShieldMaxAmount));
    }

    /// <summary>
    /// 현재 쉴드 수치를 반환합니다.
    /// </summary>
    /// <returns>현재 쉴드 수치.</returns>
    public float GetCurrentShield()
    {
        return currentShield;
    }

    /// <summary>
    /// 쉴드의 최대 수치를 반환합니다.
    /// </summary>
    /// <returns>최대 쉴드 수치.</returns>
    public float GetMaxShield()
    {
        return GetShipStat(ShipStat.ShieldMaxAmount);
    }

    /// <summary>
    /// 쉴드에 피해를 입히고, 남은 피해량을 반환합니다.
    /// 쉴드가 모두 소모되면 파괴 상태로 전환되며, 재활성화 타이머가 시작됩니다.
    /// 특정 무기 타입은 피해량 보정이 적용됩니다.
    /// </summary>
    /// <param name="damage">입힌 총 피해량.</param>
    /// <param name="shipWeaponType">공격 무기 유형.</param>
    /// <returns>쉴드를 뚫고 전달된 남은 피해량.</returns>
    public float TakeDamage(float damage, ShipWeaponType shipWeaponType)
    {
        float afterShieldDamage;

        if (shipWeaponType == ShipWeaponType.Railgun) damage = 1.5f;

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

    /// <summary>
    /// 현재 쉴드가 활성화되어 있는지 여부를 반환합니다.
    /// </summary>
    /// <returns>쉴드가 존재하고 파괴되지 않았으면 true.</returns>
    public bool IsShieldActive()
    {
        return !isShieldDestroyed && currentShield > 0;
    }
}
