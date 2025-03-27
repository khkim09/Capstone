using UnityEngine;

public class ShipWeapon : MonoBehaviour
{
    private float cooldownRemaining = 0f;

    public ShipWeaponData weaponData;

    public ShipWeapon(ShipWeaponData data)
    {
        weaponData = data;
        ResetCooldown();
    }

    public void UpdateCooldown(float deltaTime)
    {
        if (cooldownRemaining > 0)
            cooldownRemaining -= deltaTime;
    }

    public bool IsReady()
    {
        return cooldownRemaining <= 0;
    }

    public void ResetCooldown()
    {
        cooldownRemaining = weaponData.GetBaseCooldown();
    }

    public void ResetCooldown(float cooldown)
    {
        cooldownRemaining = cooldown;
    }

    public string GetWeaponName()
    {
        return weaponData.GetWeaponName();
    }

    public float GetDamage()
    {
        return weaponData.GetDamage();
    }

    public float GetAccuracy()
    {
        return weaponData.GetAccuracy();
    }

    public float GetBaseCooldown()
    {
        return weaponData.GetBaseCooldown();
    }

    public WeaponType GetWeaponType()
    {
        return weaponData.GetWeaponType();
    }
}
