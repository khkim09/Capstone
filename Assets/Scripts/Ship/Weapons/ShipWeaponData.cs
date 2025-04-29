using UnityEngine;
using System;

/// <summary>
/// 무기 정보를 저장하는 ScriptableObject.
/// 이름, 데미지, 쿨다운, 무기 종류 등의 데이터를 포함합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "Ship/Weapon")]
public class ShipWeaponData : ScriptableObject
{
    /// <summary>
    /// 무기 고유 ID
    /// </summary>
    [SerializeField] public int id;

    /// <summary>
    /// 무기 이름입니다.
    /// </summary>
    [SerializeField] public string weaponName;

    /// <summary>
    /// 무기 설명(툴팁 등에 사용)
    /// </summary>
    [SerializeField] [TextArea(3, 5)] public string description;

    /// <summary>
    /// 무기의 기본 데미지 값입니다.
    /// </summary>
    [SerializeField] public float damage;

    /// <summary>
    /// 무기의 초당 쿨타임 증가량입니다.
    /// </summary>
    public float cooldownPerSecond;

    /// <summary>
    /// 무기의 타입입니다.
    /// </summary>
    [SerializeField] public ShipWeaponType weaponType;


    public WeaponEffectData effectData;


    /// <summary>
    /// 추가 효과 강도 (퍼센트 또는 절대값)
    /// </summary>
    [SerializeField] public float effectPower;

    /// <summary>
    /// 무기 설치 비용
    /// </summary>
    [SerializeField] public int cost;

    /// <summary>
    /// 필요한 탄두 종류
    /// </summary>
    public WarheadType warheadType;

    /// <summary>
    /// 무기 이름을 반환합니다.
    /// </summary>
    /// <returns>무기 이름 문자열.</returns>
    public string GetWeaponName()
    {
        return weaponName;
    }

    /// <summary>
    /// 무기의 데미지를 반환합니다.
    /// </summary>
    /// <returns>기본 데미지 값.</returns>
    public float GetDamage()
    {
        return damage;
    }


    /// <summary>
    /// 무기의 초당 쿨타임 증가량을 반환합니다.
    /// </summary>
    /// <returns>초당 쿨타임 증가량.</returns>
    public float GetCooldownPerSecond()
    {
        return cooldownPerSecond;
    }

    /// <summary>
    /// 무기의 타입을 반환합니다.
    /// </summary>
    /// <returns>무기 타입 enum 값.</returns>
    public ShipWeaponType GetWeaponType()
    {
        return weaponType;
    }
}
