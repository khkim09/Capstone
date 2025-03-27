using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : ShipSystem
{
    private List<ShipWeapon> weapons = new();

    public override void Update(float deltaTime)
    {
        foreach (ShipWeapon weapon in weapons)
            // 일반 쿨다운 업데이트
            weapon.UpdateCooldown(deltaTime);

        // if (weapon.IsReady() && autoFireEnabled)
        // {
        //     weapon.Fire();
        // }
    }

    public bool AddWeapon(ShipWeaponData weaponData)
    {
        weapons.Add(new ShipWeapon(weaponData));
        return true;
    }

    public bool RemoveWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count)
            return false;

        weapons.RemoveAt(index);
        return true;
    }

    public bool FireWeapon(int index, Transform target)
    {
        ShipWeapon weapon = GetWeapon(index);
        if (weapon != null && weapon.IsReady())
        {
            bool fired = CombatManager.Instance.outerShipCombatController.WeaponFire(parentShip, weapon);

            if (fired)
            {
                // 발사 성공 시 쿨다운 리셋
                weapon.ResetCooldown(GetActualCooldown(weapon.GetBaseCooldown()));
                return true;
            }
        }

        return false;
    }


    // 무기의 실제 쿨타임 계산 (쿨타임 감소 적용)
    public float GetActualCooldown(float baseCooldown)
    {
        return baseCooldown * (1f - GetShipStat(ShipStat.ReloadTimeBonus));
    }

    public float GetActualDamage(float damage)
    {
        return damage * GetShipStat(ShipStat.DamageBonus);
    }


    public List<ShipWeapon> GetWeapons()
    {
        return weapons;
    }

    public int GetWeaponCount()
    {
        return weapons.Count;
    }

    public ShipWeapon GetWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count)
            return null;

        return weapons[index];
    }
}
