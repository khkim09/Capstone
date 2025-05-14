/// <summary>
/// 투사체 프리팹 설정 가이드 및 자동 설정 도구
/// </summary>

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ProjectilePrefabGenerator : EditorWindow
{
    [SerializeField] private ShipWeaponType weaponType;
    [SerializeField] private Sprite projectileSprite;
    [SerializeField] private Material trailMaterial;
    [SerializeField] private GameObject particleEffect;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private float projectileSpeed = 10f;

    [MenuItem("Game/Create Projectile Prefab")]
    public static void ShowWindow()
    {
        GetWindow<ProjectilePrefabGenerator>("Projectile Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Projectile Prefab Generator", EditorStyles.boldLabel);

        weaponType = (ShipWeaponType)EditorGUILayout.EnumPopup("Weapon Type", weaponType);
        projectileSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", projectileSprite, typeof(Sprite), false);
        trailMaterial = (Material)EditorGUILayout.ObjectField("Trail Material", trailMaterial, typeof(Material), false);
        particleEffect =
            (GameObject)EditorGUILayout.ObjectField("Particle Effect", particleEffect, typeof(GameObject), false);
        fireSound = (AudioClip)EditorGUILayout.ObjectField("Fire Sound", fireSound, typeof(AudioClip), false);
        projectileSpeed = EditorGUILayout.FloatField("Speed", projectileSpeed);

        if (GUILayout.Button("Create Projectile Prefab")) CreateProjectilePrefab();
    }

    private void CreateProjectilePrefab()
    {
        // 1. 메인 GameObject 생성
        GameObject projectile = new($"{weaponType}Projectile");

        // 2. SpriteRenderer 추가
        SpriteRenderer spriteRenderer = projectile.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = projectileSprite;
        spriteRenderer.sortingLayerName = "Projectiles";
        spriteRenderer.sortingOrder = 1;

        // 3. Rigidbody2D 추가
        Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.gravityScale = 0f;

        // 4. Collider 추가 (무기 타입에 따라)
        switch (weaponType)
        {
            case ShipWeaponType.Laser:
                CapsuleCollider2D laserCollider = projectile.AddComponent<CapsuleCollider2D>();
                laserCollider.isTrigger = true;
                laserCollider.direction = CapsuleDirection2D.Horizontal;
                laserCollider.size = new Vector2(1f, 0.2f);
                break;

            case ShipWeaponType.Missile:
                CircleCollider2D missileCollider = projectile.AddComponent<CircleCollider2D>();
                missileCollider.isTrigger = true;
                missileCollider.radius = 0.5f;
                break;
        }

        // 5. TrailRenderer 추가
        if (weaponType != ShipWeaponType.Railgun)
        {
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.material = trailMaterial;
            trail.time = 0.5f;
            trail.startWidth = 0.1f;
            trail.endWidth = 0.05f;
            trail.startColor = Color.cyan;
            trail.endColor = Color.clear;
            trail.minVertexDistance = 0.1f;
        }

        // 6. AudioSource 추가
        AudioSource audioSource = projectile.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D 사운드

        // 7. Projectile 스크립트 추가
        MyProjectile projectileScript = projectile.AddComponent<MyProjectile>();

        // 8. 파티클 시스템 추가 (자식 객체로)
        if (particleEffect != null)
        {
            GameObject effectInstance = Instantiate(particleEffect, projectile.transform);
            effectInstance.name = "OnFireEffect";
        }

        // 9. 프리팹으로 저장
        string prefabPath = $"Assets/Prefabs/Projectiles/{weaponType}Projectile.prefab";

        // 폴더가 없으면 생성
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Projectiles"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Projectiles");

        // 프리팹 저장
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(projectile, prefabPath);

        // 씬의 임시 객체 삭제
        DestroyImmediate(projectile);

        // 프리팹 선택
        Selection.activeObject = prefab;

        Debug.Log($"{weaponType} 투사체 프리팹이 생성되었습니다: {prefabPath}");
    }
}
#endif
