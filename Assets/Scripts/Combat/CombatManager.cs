using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    // [SerializeField] private ProjectilePool projectilePool; // 탄환 풀링

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 인스턴스 방지
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 이동 시에도 유지
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
