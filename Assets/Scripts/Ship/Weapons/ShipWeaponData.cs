using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Ship/Weapon")]
public class ShipWeaponData : ScriptableObject
{
    private string weaponName;
    private float damage;
    private float accuracy;
    private float baseCooldown;
    private WeaponType weaponType;

    private Sprite weaponSprite;

    public string GetWeaponName()
    {
        return weaponName;
    }

    public float GetDamage()
    {
        return damage;
    }

    public float GetAccuracy()
    {
        return accuracy;
    }

    public float GetBaseCooldown()
    {
        return baseCooldown;
    }

    public WeaponType GetWeaponType()
    {
        return weaponType;
    }
}
