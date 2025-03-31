using System.Collections;
using UnityEngine;

/// <summary>
/// 투사체의 생성, 발사, 애니메이션 및 충돌 처리를 담당하는 매니저 클래스입니다.
/// 무기 타입에 따라 적절한 투사체 프리팹을 로드하고 초기화합니다.
/// </summary>
public class ProjectileManager : MonoBehaviour
{
    /// <summary>
    /// 싱글톤 인스턴스입니다.
    /// </summary>
    public static ProjectileManager Instance { get; private set; }


    /// <summary>
    /// 초기화 시 싱글톤 인스턴스를 설정합니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 투사체를 생성하고 발사합니다.
    /// 무기 타입에 따라 프리팹을 선택하고, 목표 위치까지 이동시키며,
    /// 도착 시 콜백을 실행합니다.
    /// </summary>
    /// <param name="startPosition">발사 시작 위치.</param>
    /// <param name="targetPosition">목표 위치.</param>
    /// <param name="weaponType">무기 타입.</param>
    /// <param name="onHit">도달 시 실행할 콜백.</param>
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

    /// <summary>
    /// Projectile 컴포넌트가 없는 경우, 단순한 선형 애니메이션으로 투사체를 이동시킵니다.
    /// </summary>
    /// <param name="projectile">투사체 오브젝트.</param>
    /// <param name="start">시작 위치.</param>
    /// <param name="end">종료 위치.</param>
    /// <param name="duration">이동 시간.</param>
    /// <param name="onComplete">완료 시 콜백.</param>
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

    /// <summary>
    /// 무기 타입에 따른 투사체 프리팹을 반환합니다.
    /// </summary>
    /// <param name="weaponType">무기 타입.</param>
    /// <returns>프리팹 GameObject.</returns>
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
