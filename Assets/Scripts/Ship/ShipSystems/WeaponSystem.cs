﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 함선의 무기 시스템.
/// 무기의 추가/제거, 발사, 쿨다운 관리 및 스탯 보정을 적용한 데미지/쿨타임 계산을 담당합니다.
/// </summary>
public class WeaponSystem : ShipSystem
{
    /// <summary>
    /// 자동 발사 활성화 여부
    /// </summary>
    [SerializeField] private bool autoFireEnabled = false;

    /// <summary>
    /// 매 프레임마다 호출되어 각 무기의 쿨다운 상태를 업데이트합니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
    public override void Update(float deltaTime)
    {
        // 재장전 보너스를 한 번 계산해서 모든 무기에 적용
        float reloadBonus = GetShipStat(ShipStat.ReloadTimeBonus);
        reloadBonus = (reloadBonus + 100) / 100;

        // 임시 코드
        // reloadBonus += 50;
        //reloadBonus = -1000;

        foreach (ShipWeapon weapon in parentShip.allWeapons)
            // 재장전 보너스를 적용한 쿨다운 업데이트
            // 여기서 무기가 준비되면 자동으로 발사됨
        {
            UpdateWeaponCooldownWithBonus(weapon, deltaTime, reloadBonus);
            if (autoFireEnabled && weapon.IsReady())
                weapon.TryFire();
        }
    }

    /// <summary>
    /// 재장전 보너스를 적용하여 무기의 쿨다운을 업데이트합니다.
    /// </summary>
    /// <param name="weapon">업데이트할 무기</param>
    /// <param name="deltaTime">경과 시간</param>
    /// <param name="reloadBonus">재장전 보너스 (예: 1.2 = 20% 빠름)</param>
    private void UpdateWeaponCooldownWithBonus(ShipWeapon weapon, float deltaTime, float reloadBonus)
    {
        // ShipWeapon의 새로운 메서드를 호출하여 재장전 보너스 적용
        // 여기서 쿨다운이 완료되면 자동으로 발사됨
        if(weapon)
            weapon.UpdateCooldownWithBonus(deltaTime, reloadBonus);
    }

    /// <summary>
    /// 자동 발사 활성화/비활성화
    /// </summary>
    /// <param name="enabled">자동 발사 여부</param>
    public void SetAutoFireEnabled(bool enabled)
    {
        autoFireEnabled = enabled;
    }

    /// <summary>
    /// 자동 발사 상태 반환
    /// </summary>
    /// <returns>자동 발사 활성화 여부</returns>
    public bool IsAutoFireEnabled()
    {
        return autoFireEnabled;
    }

    /// <summary>
    /// 준비된 무기가 있는지 확인하고 발사를 시도합니다.
    /// </summary>
    /// <param name="weaponIndex">발사할 무기의 인덱스. -1이면 준비된 첫 번째 무기를 발사</param>
    /// <returns>발사된 무기 수</returns>
    public int FireWeapon(int weaponIndex = -1)
    {
        if (weaponIndex == -1)
        {
            // 준비된 모든 무기 발사
            return FireAllReadyWeapons();
        }
        else
        {
            // 특정 무기 발사
            if (weaponIndex >= 0 && weaponIndex < parentShip.allWeapons.Count)
            {
                ShipWeapon weapon = parentShip.allWeapons[weaponIndex];
                return weapon.TryFire() ? 1 : 0;
            }
        }

        return 0;
    }

    /// <summary>
    /// 준비된 모든 무기를 발사합니다.
    /// </summary>
    /// <returns>발사된 무기 수</returns>
    public int FireAllReadyWeapons()
    {
        int firedCount = 0;

        foreach (ShipWeapon weapon in parentShip.allWeapons)
            if (weapon.TryFire())
                firedCount++;

        return firedCount;
    }

    /// <summary>
    /// 특정 타입의 무기만 발사합니다.
    /// </summary>
    /// <param name="weaponType">발사할 무기 타입</param>
    /// <returns>발사된 무기 수</returns>
    public int FireWeaponsByType(ShipWeaponType weaponType)
    {
        int firedCount = 0;

        foreach (ShipWeapon weapon in parentShip.allWeapons)
            if (weapon.GetWeaponType() == weaponType && weapon.TryFire())
                firedCount++;

        return firedCount;
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
        // parentShip.allWeapons.Add(weapon);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(weapon.gridSize, Constants.Rotations.Rotation.Rotation0);

        weapon.transform.position = parentShip.GetWorldPositionFromGrid(gridPosition) + (Vector3)offset;


        weapon.ApplyRotationSprite(parentShip.GetOuterHullLevel());

        // 임시
        parentShip.allWeapons.Add(weapon);

        return weapon;
    }

    public ShipWeapon AddWeapon(ShipWeapon shipWeapon)
    {
        ShipWeapon weapon = GameObjectFactory.Instance.ShipWeaponFactory.CreateWeaponObject(shipWeapon);


        weapon.transform.SetParent(parentShip.transform);
        weapon.ApplyRotationSprite(parentShip.GetOuterHullLevel());

        Vector2 offset = RoomRotationUtility.GetRotationOffset(weapon.gridSize, Constants.Rotations.Rotation.Rotation0);

        weapon.transform.position = parentShip.GetWorldPositionFromGrid(weapon.GetGridPosition()) + (Vector3)offset;
        parentShip.allWeapons.Add(weapon);

        return weapon;
    }


    public bool RemoveWeapon(ShipWeapon weapon)
    {
        if (parentShip.allWeapons.Contains(weapon))
        {
            parentShip.allWeapons.Remove(weapon);
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
        return damage * (100 + GetShipStat(ShipStat.DamageBonus)) / 100;
    }


    /// <summary>
    /// 현재 장착된 모든 무기 리스트를 반환합니다.
    /// </summary>
    /// <returns>무기 리스트.</returns>
    public List<ShipWeapon> GetWeapons()
    {
        return parentShip.allWeapons;
    }

    /// <summary>
    /// 현재 장착된 무기 수를 반환합니다.
    /// </summary>
    /// <returns>무기 개수.</returns>
    public int GetWeaponCount()
    {
        return parentShip.allWeapons.Count;
    }

    /// <summary>
    /// 모든 무기 삭제
    /// </summary>
    public void RemoveAllWeapons()
    {
        foreach (ShipWeapon weapon in parentShip.allWeapons)
            Object.Destroy(weapon);

        parentShip.allWeapons.Clear();
    }

    /// <summary>
    /// 인덱스로 무기를 가져옵니다.
    /// </summary>
    /// <param name="index">무기 인덱스</param>
    /// <returns>해당 인덱스의 무기 또는 null</returns>
    public ShipWeapon GetWeaponByIndex(int index)
    {
        if (index >= 0 && index < parentShip.allWeapons.Count)
            return parentShip.allWeapons[index];
        return null;
    }

    /// <summary>
    /// 모든 무기가 발사 가능한지 확인합니다.
    /// </summary>
    /// <returns>발사 가능한 무기가 있으면 true</returns>
    public bool IsEveryWeaponReady()
    {
        foreach (ShipWeapon weapon in parentShip.allWeapons)
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
        float reloadBonus = GetShipStat(ShipStat.ReloadTimeBonus);
        foreach (ShipWeapon weapon in parentShip.allWeapons)
            UpdateWeaponCooldownWithBonus(weapon, deltaTime, reloadBonus);
    }

    /// <summary>
    /// 특정 타입의 무기만 반환합니다.
    /// </summary>
    /// <param name="type">무기 타입</param>
    /// <returns>해당 타입의 무기 목록</returns>
    public List<ShipWeapon> GetWeaponsByType(ShipWeaponType type)
    {
        return parentShip.allWeapons.FindAll(w => w.GetWeaponType() == type);
    }

    /// <summary>
    /// 모든 무기의 통계 데이터를 초기화합니다.
    /// </summary>
    public void ResetAllWeaponStats()
    {
        foreach (ShipWeapon weapon in parentShip.allWeapons)
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
        return parentShip.allWeapons.FindAll(w => w.weaponData.warheadType.warheadType == warheadType);
    }

    public List<ShipWeapon> GetWeaponsByEffect(ShipWeaponEffectType effectType)
    {
        return parentShip.allWeapons.FindAll(w => w.weaponData.effectData.effectType == effectType);
    }
}
