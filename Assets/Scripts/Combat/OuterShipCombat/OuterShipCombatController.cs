using UnityEngine;

/// <summary>
/// 우주선 전투에서 실제 무기 발사를 제어하는 컨트롤러 클래스입니다.
/// 무기 종류별 발사 처리, 타격 판정, 투사체 관리 등을 담당합니다.
/// </summary>
public class OuterShipCombatController
{
    /// <summary>
    /// 지정된 함선이 무기를 발사하도록 시도합니다.
    /// 무기가 준비되지 않았거나 대상이 없으면 실패합니다.
    /// </summary>
    /// <param name="attacker">공격을 시도하는 함선.</param>
    /// <param name="weapon">발사할 무기.</param>
    /// <returns>발사 성공 여부.</returns>
    public bool WeaponFire(Ship attacker, ShipWeapon weapon)
    {
        // TODO : attacker 가 플레이어면 타겟이 적, 적이면 플레이어로 설정해야함.
        Ship targetShip = GameManager.Instance.GetCurrentEnemyShip();
        if (targetShip == null) return false;
        // if target 파괴됨 -> return false;

        if (!weapon.IsReady()) return false;

        float attackDamage = attacker.GetActualDamage(weapon.GetDamage());

        Vector2Int targetPosition = targetShip.GetRandomTargetPosition();


        ProjectileManager.Instance.FireProjectile(
            weapon.GetFirePosition().position, // 발사 위치
            targetPosition, // 목표 위치
            weapon.GetWeaponType(), // 무기 타입
            () =>
            {
                // 투사체 도착 시 콜백 (실제 데미지 적용)
                targetShip.TakeAttack(attackDamage, weapon.GetWeaponType(), targetPosition);
            }
        );

        return true;
    }

    /// <summary>
    /// 레이저(발사체) 무기 발사를 처리합니다.
    /// </summary>
    /// <param name="shooter">발사하는 함선.</param>
    /// <param name="weapon">사용할 무기.</param>
    /// <param name="target">목표 트랜스폼.</param>
    /// <returns>발사 성공 여부.</returns>
    private bool FireLaser(Ship shooter, ShipWeapon weapon, Transform target)
    {
        // 1. 발사체 인스턴스 생성 (풀링 시스템에서)
        // Projectile projectile = projectilePool.GetProjectile(weapon.Data.projectilePrefab);


        // 3. 발사 효과 재생

        // 4. 사운드 재생

        return true;
    }

    /// <summary>
    /// 레일건(빔) 무기 발사를 처리합니다.
    /// </summary>
    /// <param name="shooter">발사하는 함선.</param>
    /// <param name="weapon">사용할 무기.</param>
    /// <param name="target">목표 트랜스폼.</param>
    /// <returns>발사 성공 여부.</returns>
    private bool FireRailgun(Ship shooter, ShipWeapon weapon, Transform target)
    {
        // 빔 무기 구현

        return true;
    }

    /// <summary>
    /// 미사일 무기 발사를 처리합니다.
    /// </summary>
    /// <param name="shooter">발사하는 함선.</param>
    /// <param name="weapon">사용할 무기.</param>
    /// <param name="target">목표 트랜스폼.</param>
    /// <returns>발사 성공 여부.</returns>
    private bool FireMissile(Ship shooter, ShipWeapon weapon, Transform target)
    {
        // 미사일 무기 구현

        return true;
    }


    /// <summary>
    /// 함선이 파괴되었을 때 호출되어, 이펙트 및 보상을 처리합니다.
    /// </summary>
    /// <param name="destroyed">파괴된 함선.</param>
    /// <param name="destroyer">파괴한 함선 (null일 수 있음).</param>
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

    /// <summary>
    /// 함선 파괴 시 잔해를 생성합니다.
    /// </summary>
    /// <param name="ship">잔해를 생성할 원본 함선.</param>
    private void CreateDebris(Ship ship)
    {
        // 함선 크기에 따라 잔해 생성
        // ...
    }
}
