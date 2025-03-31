using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 무기 정보를 저장하는 ScriptableObject.
/// 이름, 데미지, 쿨다운, 무기 종류 등의 데이터를 포함합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "Ship/Weapon")]
public class ShipWeaponData : ScriptableObject
{
    /// <summary>
    /// 무기 이름입니다.
    /// </summary>
    private string weaponName;

    /// <summary>
    /// 무기의 기본 데미지 값입니다.
    /// </summary>
    private float damage;

    /// <summary>
    /// 무기의 기본 쿨다운 시간입니다.
    /// </summary>
    private float baseCooldown;

    /// <summary>
    /// 무기의 타입입니다.
    /// </summary>
    private WeaponType weaponType;

    /// <summary>
    /// 무기의 아이콘 또는 스프라이트입니다.
    /// </summary>
    private Sprite weaponSprite;

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
    /// 무기의 기본 쿨다운 시간을 반환합니다.
    /// </summary>
    /// <returns>기본 쿨다운 시간.</returns>
    public float GetBaseCooldown()
    {
        return baseCooldown;
    }

    /// <summary>
    /// 무기의 타입을 반환합니다.
    /// </summary>
    /// <returns>무기 타입 enum 값.</returns>
    public WeaponType GetWeaponType()
    {
        return weaponType;
    }
}
