using UnityEngine;

public class OuterShipCombatController
{
    public bool WeaponFire(Ship attacker, Ship target, ShipWeapon weapon)
    {
        // if target 파괴됨 -> return false;

        if (!weapon.IsReady()) return false;

        float attackDamage = attacker.GetSystem<WeaponSystem>().GetActualDamage(weapon.GetDamage());

        if (weapon.GetWeaponType() == WeaponType.Railgun && target.GetSystem<ShieldSystem>().IsShieldActive())
            attackDamage *= 1.5f;

        float damageAfterShield = target.GetSystem<ShieldSystem>().TakeDamage(attackDamage);
        damageAfterShield = target.GetSystem<OuterHullSystem>().ReduceDamage(damageAfterShield);

        if (weapon.GetWeaponType() == WeaponType.Missile)
        {
            target.TakeDamage(damageAfterShield, true);
        }
        else
        {
            target.TakeDamage(damageAfterShield, false);
        }

        return true;
    }

    // 무기 발사 요청 처리
    public bool RequestWeaponFire(Ship shooter, ShipWeapon weapon, Transform target, float cooldown)
    {
        // 무기 타입에 따라 다른 처리
        switch (weapon.GetWeaponType())
        {
            case WeaponType.Laser:
                return FireLaser(shooter, weapon, target);

            case WeaponType.Railgun:
                return FireRailgun(shooter, weapon, target);

            case WeaponType.Missile:
                return FireMissile(shooter, weapon, target);

            default:
                return false;
        }
    }

    // 발사체 무기 처리
    private bool FireLaser(Ship shooter, ShipWeapon weapon, Transform target)
    {
        // 1. 발사체 인스턴스 생성 (풀링 시스템에서)
        // Projectile projectile = projectilePool.GetProjectile(weapon.Data.projectilePrefab);


        // 3. 발사 효과 재생

        // 4. 사운드 재생

        return true;
    }

    // 빔 무기 처리
    private bool FireRailgun(Ship shooter, ShipWeapon weapon, Transform target)
    {
        // 빔 무기 구현

        return true;
    }

    // 미사일 무기 처리
    private bool FireMissile(Ship shooter, ShipWeapon weapon, Transform target)
    {
        // 미사일 무기 구현

        return true;
    }


    // 함선 파괴 처리
    private void HandleShipDestroyed(Ship destroyed, Ship destroyer)
    {
        // 파괴 이펙트

        // 파괴음 재생
        // AudioManager.Instance.PlaySound(destroyed.DestructionSound, destroyed.transform.position);

        // 보상 처리
        //  if (destroyer != null && destroyer.IsPlayerShip) GameManager.Instance.AddScore(destroyed.ScoreValue);

        // 잔해 생성
        CreateDebris(destroyed);
    }

    // 잔해 생성
    private void CreateDebris(Ship ship)
    {
        // 함선 크기에 따라 잔해 생성
        // ...
    }
}
