using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선 무기 데이터베이스
/// 모든 무기 데이터와 효과 데이터를 관리합니다.
/// </summary>
[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Ship/Weapon Database")]
public class ShipWeaponDatabase : ScriptableObject
{
    /// <summary>
    /// 모든 무기 데이터 목록
    /// </summary>
    [Header("무기 데이터")] public List<ShipWeaponData> allWeapons = new();

    // 아이템 검색용 딕셔너리 (런타임에만 사용)
    private Dictionary<int, ShipWeaponData> weaponDictionary;

    /// <summary>
    /// 데이터베이스 초기화
    /// </summary>
    public void Initialize()
    {
        InitializeWeaponDictionary();
    }

    /// <summary>
    /// 무기 검색용 딕셔너리 초기화
    /// </summary>
    private void InitializeWeaponDictionary()
    {
        weaponDictionary = new Dictionary<int, ShipWeaponData>();
        foreach (ShipWeaponData weapon in allWeapons)
            if (weapon != null)
                weaponDictionary[weapon.id] = weapon;
    }


    /// <summary>
    /// ID로 무기 데이터 검색
    /// </summary>
    /// <param name="id">무기 ID</param>
    /// <returns>무기 데이터 또는 null</returns>
    public ShipWeaponData GetWeaponData(int id)
    {
        if (weaponDictionary == null)
            InitializeWeaponDictionary();

        if (weaponDictionary.TryGetValue(id, out ShipWeaponData weapon))
            return weapon;

        return null;
    }

    /// <summary>
    /// 지정된 무기 타입의 무기 목록 반환
    /// </summary>
    /// <param name="type">무기 타입</param>
    /// <returns>해당 타입의 무기 목록</returns>
    public List<ShipWeaponData> GetWeaponsByType(ShipWeaponType type)
    {
        List<ShipWeaponData> result = new();

        foreach (ShipWeaponData weapon in allWeapons)
            if (weapon != null && weapon.weaponType == type)
                result.Add(weapon);

        return result;
    }


    /// <summary>
    /// 효과 유형별 무기 목록 반환
    /// </summary>
    /// <param name="effectType">효과 유형</param>
    /// <returns>해당 효과 유형을 가진 무기 목록</returns>
    public List<ShipWeaponData> GetWeaponsByEffectType(ShipWeaponEffectType effectType)
    {
        List<ShipWeaponData> result = new();

        foreach (ShipWeaponData weapon in allWeapons)
            if (weapon != null)
                if (weapon.effectData.effectType == effectType)
                    result.Add(weapon);

        return result;
    }
}
