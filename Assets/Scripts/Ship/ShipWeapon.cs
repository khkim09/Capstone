using UnityEngine;

public class ShipWeapon : MonoBehaviour
{
    [Header("Weapon Basic Settings")]
    public string weaponName = "Default Weapon";
    public float damage = 10f;
    public float fireRate = 1f; // 초당 발사 횟수
    public float range = 100f;

    // TODO: 무기 발사 관련 추가 변수들 (예: 발사 위치, 탄환 프리팹 등)
    // [Header("Weapon Firing Settings")]
    // public Transform firePoint;
    // public GameObject projectilePrefab;

    // 내부 타이머 (발사 속도 제어용)
    private float fireCooldown = 0f;

    void Update()
    {
        // 발사 쿨다운 타이머 업데이트
        fireCooldown -= Time.deltaTime;

        // TODO: 입력 처리 및 발사 로직 추가
        // 예시:
        // if (Input.GetButton("Fire1") && fireCooldown <= 0f)
        // {
        //     FireWeapon();
        //     fireCooldown = 1f / fireRate;
        // }
    }

    // TODO: 실제 무기 발사 로직 구현 (탄환 인스턴스화, 목표 지정, 데미지 적용 등)
    // private void FireWeapon()
    // {
    //     // 예시: 탄환을 생성하고, 방향 및 데미지 설정
    //     Debug.Log($"{weaponName} fired, dealing {damage} damage.");
    // }
}
