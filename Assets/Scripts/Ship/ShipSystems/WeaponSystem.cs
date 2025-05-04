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
    /// <returns>만들어진 새로운 무기의 참조.</returns>
    public ShipWeapon AddWeapon(int weaponId, Vector2Int gridPosition, ShipWeaponAttachedDirection direction)
    {
        // 무기 인스턴스 생성
        ShipWeapon weapon = GameObjectFactory.Instance.ShipWeaponFactory.CreateWeaponInstance(weaponId);

        // 필요한 속성 설정
        weapon.SetGridPosition(gridPosition);
        weapon.SetAttachedDirection(direction);

        weapon.transform.SetParent(parentShip.transform);
        weapons.Add(weapon);

        weapon.transform.position = ShipGridHelper.GetRoomWorldPosition(gridPosition, weapon.gridSize) + new Vector3(0, 0, 5f);
        weapon.ApplyRotationSprite(parentShip.GetOuterHullLevel());
        return weapon;
    }

    public ShipWeapon AddWeapon(ShipWeapon shipWeapon)
    {
        ShipWeapon weapon = GameObjectFactory.Instance.ShipWeaponFactory.CreateWeaponObject(shipWeapon);


        weapon.transform.SetParent(parentShip.transform);
        weapon.ApplyRotationSprite(parentShip.GetOuterHullLevel());

        weapon.transform.position = ShipGridHelper.GetRoomWorldPosition(weapon.GetGridPosition(), weapon.gridSize) + new Vector3(0, 0, 5f);
        weapons.Add(weapon);

        return weapon;
    }


    public bool RemoveWeapon(ShipWeapon weapon)
    {
        if (weapons.Contains(weapon))
        {
            weapons.Remove(weapon);
            if (weapon != null) Object.Destroy(weapon.gameObject);
            return true;
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
        return baseCooldown * GetShipStat(ShipStat.ReloadTimeBonus);
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
    /// 인덱스로 무기를 제거합니다.
    /// </summary>
    /// <param name="index">제거할 무기의 인덱스</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveWeapon(int index)
    {
        if (index >= 0 && index < weapons.Count)
        {
            ShipWeapon weapon = weapons[index];
            weapons.RemoveAt(index);

            // 게임 오브젝트 제거
            if (weapon != null) Object.Destroy(weapon.gameObject);

            return true;
        }

        return false;
    }

    /// <summary>
    /// 인덱스로 무기를 가져옵니다.
    /// </summary>
    /// <param name="index">무기 인덱스</param>
    /// <returns>해당 인덱스의 무기 또는 null</returns>
    public ShipWeapon GetWeaponByIndex(int index)
    {
        if (index >= 0 && index < weapons.Count)
            return weapons[index];
        return null;
    }

    /// <summary>
    /// 모든 무기가 발사 가능한지 확인합니다.
    /// </summary>
    /// <returns>발사 가능한 무기가 있으면 true</returns>
    public bool IsEveryWeaponReady()
    {
        foreach (ShipWeapon weapon in weapons)
            if (weapon.IsReady() && weapon.IsEnabled())
                return true;
        return false;
    }

    /// <summary>
    /// 모든 무기의 쿨다운을 업데이트합니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간</param>
    public void UpdateWeaponCooldowns(float deltaTime)
    {
        foreach (ShipWeapon weapon in weapons) weapon.UpdateCooldown(deltaTime);
    }

    /// <summary>
    /// 특정 타입의 무기만 반환합니다.
    /// </summary>
    /// <param name="type">무기 타입</param>
    /// <returns>해당 타입의 무기 목록</returns>
    public List<ShipWeapon> GetWeaponsByType(ShipWeaponType type)
    {
        return weapons.FindAll(w => w.GetWeaponType() == type);
    }

    /// <summary>
    /// 모든 무기의 통계 데이터를 초기화합니다.
    /// </summary>
    public void ResetAllWeaponStats()
    {
        foreach (ShipWeapon weapon in weapons)
        {
            weapon.SetHits(0);
            weapon.SetTotalDamageDealt(0f);
        }
    }

    /// <summary>
    /// 선택한 타입의 탄두를 필요로 하는 무기 목록을 반환합니다.
    /// </summary>
    /// <param name="warheadType">탄두 타입</param>
    /// <returns>해당 탄두 타입을 사용하는 무기 목록</returns>
    public List<ShipWeapon> GetWeaponsByWarheadType(WarheadType warheadType)
    {
        return weapons.FindAll(w => w.weaponData.warheadType.warheadType == warheadType);
    }

    public List<ShipWeapon> GetWeaponsByEffect(ShipWeaponEffectType effectType)
    {
        return weapons.FindAll(w => w.weaponData.effectData.effectType == effectType);
    }
}
