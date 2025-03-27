using UnityEngine;

public class OuterShipCombatController
{
    public bool WeaponFire(Ship attacker, ShipWeapon weapon)
    {
        // TODO : attacker 가 플레이어면 타겟이 적, 적이면 플레이어로 설정해야함.
        Ship targetShip = GameManager.Instance.GetCurrentEnemyShip();
        // if target 파괴됨 -> return false;

        if (!weapon.IsReady()) return false;

        float attackDamage = attacker.GetSystem<WeaponSystem>().GetActualDamage(weapon.GetDamage());

        if (weapon.GetWeaponType() == WeaponType.Railgun && targetShip.GetSystem<ShieldSystem>().IsShieldActive())
            attackDamage *= 1.5f;

        float damageAfterShield = targetShip.GetSystem<ShieldSystem>().TakeDamage(attackDamage);
        damageAfterShield = targetShip.GetSystem<OuterHullSystem>().ReduceDamage(damageAfterShield);

        if (weapon.GetWeaponType() == WeaponType.Missile)
            targetShip.TakeDamage(damageAfterShield, true);
        else
            targetShip.TakeDamage(damageAfterShield, false);

        // TODO: 공격이 실제로 날라가는 애니메이션 내지 투사체가 육안으로 확인이 되어야하는데,
        //       그러면 TakeDamage 내부에 있는 TakeRandomDamage함수가 밖으로 나와야하는 것 아닌가?
        //       Ship의 랜덤 칸을 반환하는 함수가 필요할지도.
        //       만약 그런 것이 존재한다면, weapon.Fire(target의 랜덤 칸) 같은 함수가 필요.
        //       weapon.Fire()함수는 발사 애니메이션과 폭발 애니메이션 보여주는 함수, 실제 데미지는 적용 X

        return true;
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
