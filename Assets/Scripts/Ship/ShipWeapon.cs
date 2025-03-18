[System.Serializable]
public class ShipWeapon
{
    public string weaponName;
    public float damage;
    public float accuracy;
    public float reloadTime;
    public float currentCooldown;
    public bool isReady => currentCooldown <= 0;
    public WeaponType weaponType;

    public void Update(float deltaTime, float reloadBonus)
    {
        if (!isReady) currentCooldown -= deltaTime * (1 + reloadBonus / 100f);
    }

    public void Fire()
    {
        if (isReady) currentCooldown = reloadTime;
    }
}

public enum WeaponType
{
    Laser,
    Missile,
    Railgun
}
