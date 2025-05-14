using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 투사체 관리 시스템 (ID 기반)
/// 모든 투사체의 생성, 풀링, 추적, 효과를 중앙에서 관리합니다.
/// </summary>
public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    [Header("투사체 설정")] [SerializeField] private List<ProjectileData> projectileData;

    [Header("풀링 설정")] [SerializeField] private int poolSize = 100;

    [Header("조직화 설정")] [SerializeField] private bool organizeIntoFolders = true;

    /// <summary>
    /// 투사체 풀링 시스템 (ID 기반)
    /// </summary>
    private Dictionary<int, Queue<MyProjectile>> projectilePools;

    /// <summary>
    /// 투사체 데이터 캐시 (ID로 빠른 접근)
    /// </summary>
    private Dictionary<int, ProjectileData> projectileDataCache;

    /// <summary>
    /// 활성화된 투사체 목록
    /// </summary>
    private List<MyProjectile> activeProjectiles;

    [Header("이펙트 관리")] [SerializeField] private Transform effectsParent; // 이펙트들을 정리할 부모 오브젝트

    // 조직화를 위한 부모 오브젝트들
    private Transform projectilePoolsParent;
    private Transform activeProjectilesParent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeOrganization();
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 하이어라키 조직화 초기화
    /// </summary>
    private void InitializeOrganization()
    {
        if (organizeIntoFolders)
        {
            // 프로젝타일 풀 폴더 생성
            GameObject poolsFolder = new("Projectile Pools");
            poolsFolder.transform.SetParent(transform);
            projectilePoolsParent = poolsFolder.transform;

            // 활성 프로젝타일 폴더 생성
            GameObject activeFolder = new("Active Projectiles");
            activeFolder.transform.SetParent(transform);
            activeProjectilesParent = activeFolder.transform;

            // 이펙트 폴더가 없으면 생성
            if (effectsParent == null)
            {
                GameObject effectsFolder = new("Impact Effects");
                effectsFolder.transform.SetParent(transform);
                effectsParent = effectsFolder.transform;
            }
        }
        else
        {
            // 조직화하지 않는 경우 ProjectileManager 자체를 부모로 사용
            projectilePoolsParent = transform;
            activeProjectilesParent = transform;
            if (effectsParent == null)
                effectsParent = transform;
        }
    }

    /// <summary>
    /// 투사체 풀 초기화
    /// </summary>
    private void InitializePools()
    {
        projectilePools = new Dictionary<int, Queue<MyProjectile>>();
        projectileDataCache = new Dictionary<int, ProjectileData>();
        activeProjectiles = new List<MyProjectile>();

        // 각 투사체 데이터별로 풀 생성
        foreach (ProjectileData data in projectileData)
        {
            // 캐시에 저장
            projectileDataCache[data.projectileId] = data;

            if (!projectilePools.ContainsKey(data.projectileId))
                projectilePools[data.projectileId] = new Queue<MyProjectile>();

            // 투사체 ID별 폴더 생성 (조직화 옵션이 켜진 경우)
            Transform projectileIdParent = projectilePoolsParent;
            if (organizeIntoFolders)
            {
                GameObject projectileFolder = new($"{data.projectileName} (ID: {data.projectileId})");
                projectileFolder.transform.SetParent(projectilePoolsParent);
                projectileIdParent = projectileFolder.transform;
            }

            // 미리 투사체 생성
            for (int i = 0; i < poolSize; i++)
                if (data.projectilePrefab != null)
                {
                    GameObject projectileObj = Instantiate(data.projectilePrefab, projectileIdParent);
                    MyProjectile projectile = projectileObj.GetComponent<MyProjectile>();
                    if (projectile != null)
                    {
                        projectile.Initialize(data);
                        projectile.gameObject.SetActive(false);
                        projectilePools[data.projectileId].Enqueue(projectile);
                    }
                }
        }
    }

    /// <summary>
    /// 투사체 발사 (단순화된 버전)
    /// </summary>
    /// <param name="startPos">발사 위치</param>
    /// <param name="targetWorldPos">목표 위치</param>
    /// <param name="projectileId">투사체 ID</param>
    /// <param name="onHitCallback">타격 시 호출될 콜백</param>
    /// <returns>발사된 투사체</returns>
    public MyProjectile FireProjectile(Vector3 startPos, Vector3 targetWorldPos, int projectileId,
        Action onHitCallback)
    {
        // 풀에서 투사체 가져오기
        MyProjectile projectile = GetProjectileFromPool(projectileId);
        if (projectile == null)
        {
            Debug.LogWarning($"No available projectile for ID: {projectileId}");
            return null;
        }

        // 투사체 설정
        projectile.gameObject.SetActive(true);
        projectile.transform.position = startPos;

        // 활성 투사체 폴더로 이동 (조직화 옵션이 켜진 경우)
        if (organizeIntoFolders)
            projectile.transform.SetParent(activeProjectilesParent);

        // 모든 투사체는 동일한 Fire 메서드 사용
        projectile.Fire(targetWorldPos, onHitCallback);

        activeProjectiles.Add(projectile);
        return projectile;
    }

    /// <summary>
    /// 투사체 ID로 데이터 찾기
    /// </summary>
    /// <param name="projectileId">투사체 ID</param>
    /// <returns>해당 ID의 투사체 데이터</returns>
    public ProjectileData GetProjectileData(int projectileId)
    {
        if (projectileDataCache.ContainsKey(projectileId))
            return projectileDataCache[projectileId];

        Debug.LogWarning($"No projectile data found for ID: {projectileId}");
        return null;
    }

    /// <summary>
    /// 풀에서 투사체 가져오기
    /// </summary>
    private MyProjectile GetProjectileFromPool(int projectileId)
    {
        if (!projectilePools.ContainsKey(projectileId) || projectilePools[projectileId].Count == 0)
            // 풀이 비어있으면 새로 생성
            CreateAdditionalProjectiles(projectileId, 10);

        if (projectilePools[projectileId].Count > 0)
            return projectilePools[projectileId].Dequeue();

        return null;
    }

    /// <summary>
    /// 풀이 부족할 때 추가 투사체 생성
    /// </summary>
    private void CreateAdditionalProjectiles(int projectileId, int count)
    {
        ProjectileData data = GetProjectileData(projectileId);
        if (data == null || data.projectilePrefab == null) return;

        if (!projectilePools.ContainsKey(projectileId))
            projectilePools[projectileId] = new Queue<MyProjectile>();

        // 해당 투사체 ID의 폴더 찾기
        Transform projectileIdParent = projectilePoolsParent;
        if (organizeIntoFolders)
            // 기존 폴더 찾기
            foreach (Transform child in projectilePoolsParent)
                if (child.name == $"{data.projectileName} (ID: {projectileId})")
                {
                    projectileIdParent = child;
                    break;
                }

        for (int i = 0; i < count; i++)
        {
            GameObject projectileObj = Instantiate(data.projectilePrefab, projectileIdParent);
            MyProjectile projectile = projectileObj.GetComponent<MyProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(data);
                projectile.gameObject.SetActive(false);
                projectilePools[projectileId].Enqueue(projectile);
            }
        }
    }

    /// <summary>
    /// 투사체를 풀로 반환
    /// </summary>
    public void ReturnProjectileToPool(MyProjectile projectile)
    {
        if (projectile == null) return;

        projectile.gameObject.SetActive(false);
        activeProjectiles.Remove(projectile);

        int projectileId = projectile.GetProjectileId();
        ProjectileData data = GetProjectileData(projectileId);

        // 투사체 ID별 폴더로 다시 이동
        if (organizeIntoFolders && projectilePools.ContainsKey(projectileId) && data != null)
        {
            foreach (Transform child in projectilePoolsParent)
                if (child.name == $"{data.projectileName} (ID: {projectileId})")
                {
                    projectile.transform.SetParent(child);
                    break;
                }
        }
        else
        {
            projectile.transform.SetParent(projectilePoolsParent);
        }

        if (projectilePools.ContainsKey(projectileId))
            projectilePools[projectileId].Enqueue(projectile);
    }

    /// <summary>
    /// 모든 활성 투사체 업데이트
    /// </summary>
    private void Update()
    {
        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
            if (activeProjectiles[i] != null && activeProjectiles[i].gameObject.activeInHierarchy)
                activeProjectiles[i].UpdateProjectile(Time.deltaTime);
            else
                activeProjectiles.RemoveAt(i);
    }

    /// <summary>
    /// 임팩트 이펙트 생성 (중앙 관리)
    /// </summary>
    /// <param name="effectPrefab">이펙트 프리팹</param>
    /// <param name="position">생성 위치</param>
    /// <param name="duration">지속 시간</param>
    public void CreateImpactEffect(GameObject effectPrefab, Vector3 position, float duration)
    {
        if (effectPrefab == null) return;

        GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);

        // 이펙트 정리를 위한 부모 설정
        if (effectsParent != null)
            effect.transform.SetParent(effectsParent);

        // 지정된 시간 후 제거
        Destroy(effect, duration);

        // 추가 기능: 파티클 시스템이 있다면 자동 정리
        ParticleSystem particle = effect.GetComponent<ParticleSystem>();
        if (particle != null)
        {
            // 파티클 시스템이 끝나면 자동으로 제거
            particle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            float particleDuration =
                Mathf.Max(duration, particle.main.duration + particle.main.startLifetime.constantMax);
            Destroy(effect, particleDuration);
        }
    }

    /// <summary>
    /// 무기 타입과 호환되는 투사체 ID 목록 반환
    /// </summary>
    /// <param name="weaponType">무기 타입</param>
    /// <returns>호환되는 투사체 ID 목록</returns>
    public List<int> GetCompatibleProjectileIds(ShipWeaponType weaponType)
    {
        return projectileData
            .Where(data => data.compatibleWeaponTypes.Contains(weaponType))
            .Select(data => data.projectileId)
            .ToList();
    }
}
