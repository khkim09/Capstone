﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선 무기 관리 시스템
/// 무기 데이터 접근 및 무기 인스턴스 생성을 담당합니다.
/// </summary>
public class ShipWeaponFactory : MonoBehaviour
{
    [SerializeField] private ShipWeaponDatabase weaponDatabase;
    [SerializeField] private GameObject weaponPrefab; // 무기 기본 프리팹

    private void Awake()
    {
        InitializeWeaponDatabase();
    }

    /// <summary>
    /// 무기 데이터베이스 초기화
    /// </summary>
    private void InitializeWeaponDatabase()
    {
        if (weaponDatabase != null)
            weaponDatabase.Initialize();
        else
            Debug.LogError("Weapon Database not assigned!");
    }

    /// <summary>
    /// ID로 무기 데이터 검색
    /// </summary>
    /// <param name="id">무기 ID</param>
    /// <returns>무기 데이터 또는 null</returns>
    public ShipWeaponData GetWeaponData(int id)
    {
        if (weaponDatabase == null)
        {
            Debug.LogError("Weapon Database is not assigned!");
            return null;
        }

        return weaponDatabase.GetWeaponData(id);
    }

    /// <summary>
    /// 무기 인스턴스 생성 - 함선에 자동 추가하지 않음
    /// </summary>
    /// <param name="weaponId">무기 ID</param>
    /// <returns>생성된 무기 또는 null</returns>
    public ShipWeapon CreateWeaponInstance(int weaponId)
    {
        ShipWeaponData weaponData = GetWeaponData(weaponId);
        if (weaponData == null)
        {
            Debug.LogError($"Weapon data not found for ID: {weaponId}");
            return null;
        }

        if (weaponPrefab == null)
        {
            Debug.LogError("Weapon prefab is not assigned!");
            return null;
        }

        // 무기 프리팹 인스턴스 생성
        GameObject weaponObject = Instantiate(weaponPrefab);
        weaponObject.name = $"Weapon_{weaponData.weaponName}";

        // ShipWeapon 컴포넌트 설정
        ShipWeapon weaponInstance = weaponObject.GetComponent<ShipWeapon>();
        if (weaponInstance == null)
            weaponInstance = weaponObject.AddComponent<ShipWeapon>();

        // 무기 데이터 설정
        weaponInstance.weaponData = weaponData;

        // 무기 초기화
        weaponInstance.Initialize();

        return weaponInstance;
    }

    public ShipWeapon CreateWeaponObject(ShipWeapon shipWeapon)
    {
        GameObject weaponObject = Instantiate(weaponPrefab);

        ShipWeapon weaponComponent = weaponObject.GetComponent<ShipWeapon>();

        if (weaponComponent == null) weaponComponent = weaponObject.AddComponent<ShipWeapon>();

        weaponComponent.CopyFrom(shipWeapon);

        weaponObject.name = $"weapon_{shipWeapon.name}";

        return weaponComponent;
    }

    /// <summary>
    /// 모든 무기 데이터 반환
    /// </summary>
    /// <returns>모든 무기 데이터 목록</returns>
    public List<ShipWeaponData> GetAllWeaponData()
    {
        if (weaponDatabase == null)
            return new List<ShipWeaponData>();

        return weaponDatabase.allWeapons;
    }

    /// <summary>
    /// 특정 유형의 모든 무기 데이터 검색
    /// </summary>
    /// <param name="type">무기 유형</param>
    /// <returns>해당 유형의 모든 무기 데이터</returns>
    public List<ShipWeaponData> GetWeaponsByType(ShipWeaponType type)
    {
        if (weaponDatabase == null)
            return new List<ShipWeaponData>();

        return weaponDatabase.GetWeaponsByType(type);
    }

    /// <summary>
    /// 특정 효과 유형을 가진 모든 무기 데이터 검색
    /// </summary>
    /// <param name="effectType">효과 유형</param>
    /// <returns>해당 효과 유형을 가진 모든 무기 데이터</returns>
    public List<ShipWeaponData> GetWeaponsByEffectType(ShipWeaponEffectType effectType)
    {
        if (weaponDatabase == null)
            return new List<ShipWeaponData>();

        return weaponDatabase.GetWeaponsByEffectType(effectType);
    }
}
