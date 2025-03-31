using System.Collections;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 투사체 발사 및 시각적 처리
    public void FireProjectile(Vector2 startPosition, Vector2 targetPosition, WeaponType weaponType,
        System.Action onHit)
    {
        // 무기 타입에 따라 다른 투사체 생성 및 애니메이션 처리
        GameObject projectilePrefab = GetProjectilePrefab(weaponType);

        // 투사체 생성
        GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);

        // 투사체 이동 및 충돌 처리 컴포넌트 설정
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent != null)
            projectileComponent.Initialize(targetPosition, onHit);
        else
            // 투사체 컴포넌트가 없는 경우, 간단한 애니메이션 후 콜백
            StartCoroutine(SimpleProjectileAnimation(projectile, startPosition, targetPosition, 0.5f, onHit));
    }

    // 간단한 투사체 애니메이션
    private IEnumerator SimpleProjectileAnimation(GameObject projectile, Vector3 start, Vector3 end, float duration,
        System.Action onComplete)
    {
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            projectile.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        onComplete?.Invoke();
        Destroy(projectile);
    }

    // 무기 타입별 투사체 프리팹 반환
    private GameObject GetProjectilePrefab(WeaponType weaponType)
    {
        // 실제 구현에서는 무기 타입별 프리팹 반환
        // 이 예시에서는 간단히 리소스 로드
        switch (weaponType)
        {
            case WeaponType.Laser:
                return Resources.Load<GameObject>("Prefabs/LaserProjectile");
            case WeaponType.Railgun:
                return Resources.Load<GameObject>("Prefabs/RailgunProjectile");
            case WeaponType.Missile:
                return Resources.Load<GameObject>("Prefabs/MissileProjectile");
            default:
                return Resources.Load<GameObject>("Prefabs/DefaultProjectile");
        }
    }
}
