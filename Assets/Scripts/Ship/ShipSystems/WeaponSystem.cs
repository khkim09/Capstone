using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 무기 시스템.
/// 무기의 추가/제거, 발사, 쿨다운 관리 및 스탯 보정을 적용한 데미지/쿨타임 계산을 담당합니다.
/// </summary>
public class WeaponSystem : ShipSystem
{
    /// <summary>
    /// 현재 장착된 무기들의 리스트입니다.
    /// </summary>
    private List<ShipWeapon> weapons = new();

    /// <summary>
    /// 매 프레임마다 호출되어 각 무기의 쿨다운 상태를 업데이트합니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
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

    /// <summary>
    /// 새로운 무기를 시스템에 추가합니다.
    /// </summary>
    /// <param name="weaponData">추가할 무기 데이터.</param>
    /// <param name="gridPosition">추가할 그리드 좌표./param>
    /// <returns>항상 true 반환 (현재는 실패 조건 없음).</returns>
    public bool AddWeapon(ShipWeaponData weaponData, Vector2Int gridPosition)
    {
        GameObject weaponObject = Object.Instantiate(parentShip.weaponPrefab, parentShip.transform);
        ShipWeapon weaponInstance = weaponObject.GetComponent<ShipWeapon>();
        weaponInstance.Initialize(weaponData, gridPosition);
        weapons.Add(weaponInstance);
        return true;
    }

    /// <summary>
    /// 지정한 인덱스의 무기를 제거합니다.
    /// </summary>
    /// <param name="index">제거할 무기의 인덱스.</param>
    /// <returns>제거에 성공하면 true, 유효하지 않은 인덱스이면 false.</returns>
    public bool RemoveWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count)
            return false;

        weapons.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// 지정한 인덱스의 무기를 대상에게 발사합니다.
    /// 무기가 준비된 상태일 경우에만 발사됩니다.
    /// </summary>
    /// <param name="index">발사할 무기의 인덱스.</param>
    /// <param name="target">공격 대상 Transform.</param>
    /// <returns>발사에 성공하면 true, 실패하면 false.</returns>
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


    /// <summary>
    /// 실제 적용될 쿨다운 시간을 계산합니다.
    /// 쿨다운 보너스를 고려하여 기본 쿨타임을 감소시킵니다.
    /// </summary>
    /// <param name="baseCooldown">기본 쿨다운 시간.</param>
    /// <returns>보정된 쿨다운 시간.</returns>
    public float GetActualCooldown(float baseCooldown)
    {
        return baseCooldown * (1f - GetShipStat(ShipStat.ReloadTimeBonus));
    }


    /// <summary>
    /// 실제 적용될 데미지를 계산합니다.
    /// 데미지 보너스를 적용하여 최종 데미지를 산출합니다.
    /// </summary>
    /// <param name="damage">기본 데미지 값.</param>
    /// <returns>보정된 데미지 값.</returns>
    public float GetActualDamage(float damage)
    {
        return damage * GetShipStat(ShipStat.DamageBonus);
    }


    /// <summary>
    /// 현재 장착된 모든 무기 리스트를 반환합니다.
    /// </summary>
    /// <returns>무기 리스트.</returns>
    public List<ShipWeapon> GetWeapons()
    {
        return weapons;
    }

    /// <summary>
    /// 현재 장착된 무기 수를 반환합니다.
    /// </summary>
    /// <returns>무기 개수.</returns>
    public int GetWeaponCount()
    {
        return weapons.Count;
    }

    /// <summary>
    /// 지정한 인덱스의 무기를 반환합니다.
    /// </summary>
    /// <param name="index">무기 인덱스.</param>
    /// <returns>해당 무기 객체. 유효하지 않은 경우 null.</returns>
    public ShipWeapon GetWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count)
            return null;

        return weapons[index];
    }
}
