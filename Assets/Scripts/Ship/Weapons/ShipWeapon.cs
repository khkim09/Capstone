using UnityEngine;

/// <summary>
/// 실제 무기 인스턴스를 나타내는 클래스.
/// 무기 데이터와 쿨다운 상태를 포함하며, 발사 위치 및 각종 무기 정보를 제공합니다.
/// </summary>
public class ShipWeapon : MonoBehaviour
{
    /// <summary>
    /// 현재 쿨타임.
    /// </summary>
    private float currentCooldown = 0f;

    /// <summary>
    /// 이 무기에 해당하는 무기 데이터입니다.
    /// </summary>
    public ShipWeaponData weaponData;

    /// <summary>
    /// 무기의 발사 지점 Transform입니다.
    /// </summary>
    [SerializeField] private Transform firePoint;

    /// <summary>
    /// 무기의 그리드 상 좌표입니다.
    /// </summary>
    [SerializeField] private Vector2Int gridPosition;

    public Vector2Int gridSize = new(2, 1);

    /// <summary>
    /// 무기가 시설과 연결되어있는 방향(회전 상태)입니다.
    /// 회전 상태는 East → South → North → East 순으로 순환합니다.
    /// </summary>
    private ShipWeaponAttachedDirection attachedDirection = ShipWeaponAttachedDirection.North;

    /// <summary>
    /// SpriteRenderer 컴포넌트 참조 (회전 시 스프라이트 갱신용)
    /// </summary>
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// 적중 횟수 (통계용)
    /// </summary>
    private int hits = 0;

    /// <summary>
    /// 입힌 총 피해량 (통계용)
    /// </summary>
    private float totalDamageDealt = 0f;

    /// <summary>
    /// 무기 활성화 상태
    /// </summary>
    private bool isEnabled = true;

    private void Awake()
    {
        // SpriteRenderer 컴포넌트 가져오기
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = SortingOrderConstants.Weapon;
    }

    /// <summary>
    /// 무기 초기화 (무기 생성 후 필요한 추가 설정)
    /// </summary>
    public void Initialize()
    {
        // TODO: 회전 상태와 외갑판에 따른 스프라이트 설정 필요
    }

    /// <summary>
    /// 현재 부착 방향에 따른 인덱스 반환 (0: North, 1: East, 2: South)
    /// </summary>
    private int GetDirectionIndex()
    {
        switch (attachedDirection)
        {
            case ShipWeaponAttachedDirection.North: return 0;
            case ShipWeaponAttachedDirection.East: return 1;
            case ShipWeaponAttachedDirection.South: return 2;
            default: return 1; // 기본값은 East
        }
    }

    /// <summary>
    /// 무기 방향 변경 시 스프라이트 업데이트
    /// </summary>
    /// <param name="newDirection">새 방향</param>
    public void SetAttachedDirection(ShipWeaponAttachedDirection newDirection)
    {
        attachedDirection = newDirection;

        // TODO: 회전 상태와 외갑판에 따른 스프라이트 설정 필요
    }

    /// <summary>
    /// 무기 활성화 상태 반환
    /// </summary>
    public bool IsEnabled()
    {
        return isEnabled;
    }

    /// <summary>
    /// 무기 활성화/비활성화
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }

    /// <summary>
    /// 적중 횟수 반환
    /// </summary>
    public int GetHits()
    {
        return hits;
    }

    /// <summary>
    /// 적중 횟수 설정
    /// </summary>
    public void SetHits(int value)
    {
        hits = Mathf.Max(0, value);
    }

    /// <summary>
    /// 적중 추가 (적 타격 시 호출)
    /// </summary>
    public void AddHit()
    {
        hits++;
    }

    /// <summary>
    /// 입힌 총 피해량 반환
    /// </summary>
    public float GetTotalDamageDealt()
    {
        return totalDamageDealt;
    }

    /// <summary>
    /// 입힌 총 피해량 설정
    /// </summary>
    public void SetTotalDamageDealt(float value)
    {
        totalDamageDealt = Mathf.Max(0, value);
    }

    /// <summary>
    /// 피해량 추가 (적 타격 시 호출)
    /// </summary>
    public void AddDamageDealt(float damage)
    {
        totalDamageDealt += damage;
    }

    /// <summary>
    /// 매 프레임 호출되어 쿨다운 시간을 감소시킵니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
    public void UpdateCooldown(float deltaTime)
    {
        if (currentCooldown < 100) currentCooldown += deltaTime * GetCooldownPerSecond();
    }

    /// <summary>
    /// 무기가 발사 가능한 상태인지 확인합니다.
    /// </summary>
    /// <returns>쿨다운이 완료되었으면 true.</returns>
    public bool IsReady()
    {
        return currentCooldown >= 100;
    }

    /// <summary>
    /// 무기의 쿨다운을 기본값으로 초기화합니다.
    /// </summary>
    public void ResetCooldown()
    {
        currentCooldown = 0f;
    }

    /// <summary>
    /// 무기의 쿨다운을 지정한 값으로 설정합니다.
    /// </summary>
    /// <param name="cooldown">설정할 쿨다운 시간.</param>
    public void ResetCooldown(float cooldown)
    {
        currentCooldown = cooldown;
    }

    /// <summary>
    /// 무기 이름을 반환합니다.
    /// </summary>
    /// <returns>무기 이름 문자열.</returns>
    public string GetWeaponName()
    {
        return weaponData.GetWeaponName();
    }

    /// <summary>
    /// 무기의 피해량을 반환합니다.
    /// </summary>
    /// <returns>피해량 값.</returns>
    public float GetDamage()
    {
        return weaponData.GetDamage();
    }

    public float GetCooldownPerSecond()
    {
        return weaponData.GetCooldownPerSecond();
    }

    /// <summary>
    /// 무기의 타입을 반환합니다.
    /// </summary>
    /// <returns>무기 타입 enum 값.</returns>
    public ShipWeaponType GetWeaponType()
    {
        return weaponData.GetWeaponType();
    }

    /// <summary>
    /// 무기 투사체의 발사 위치를 반환합니다.
    /// </summary>
    /// <returns>무기 투사체 발사 위치 Transform.</returns>
    public Transform GetFirePosition()
    {
        return firePoint;
    }

    /// <summary>
    /// 무기의 그리드 상 위치를 반환합니다.
    /// </summary>
    /// <returns>그리드 상의 좌표.</returns>
    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }

    /// <summary>
    /// 무기의 그리드 상 위치를 설정합니다.
    /// </summary>
    /// <param name="position">설정할 그리드 위치.</param>
    public void SetGridPosition(Vector2Int position)
    {
        gridPosition = position;
    }

    /// <summary>
    /// 무기가 시설과 연결된 방향(회전 상태)을 반환합니다.
    /// </summary>
    /// <returns>무기가 연결된 방향.</returns>
    public ShipWeaponAttachedDirection GetAttachedDirection()
    {
        return attachedDirection;
    }


    /// <summary>
    /// 우클릭 시 호출되어 무기의 회전(힌지 회전) 상태를 변경합니다.
    /// 회전 상태는 East → South → North → East 순으로 순환합니다.
    /// </summary>
    public void RotateWeapon()
    {
        // 사용하지 말것!!!! 방향은 인 게임 중에 바뀔 일이 없고, 배를 만들 때만 바뀜

        // 현재 attachedDirection 값을 기반으로 다음 상태로 전환
        switch (attachedDirection)
        {
            case ShipWeaponAttachedDirection.East:
                attachedDirection = ShipWeaponAttachedDirection.South;
                break;
            case ShipWeaponAttachedDirection.South:
                attachedDirection = ShipWeaponAttachedDirection.North;
                break;
            case ShipWeaponAttachedDirection.North:
                attachedDirection = ShipWeaponAttachedDirection.East;
                break;
        }

        // 변경된 attachedDirection에 따른 스프라이트 적용
        ApplyRotationSprite();
    }

    /// <summary>
    /// 현재 attachedDirection에 맞는 스프라이트를 SpriteRenderer에 적용합니다.
    /// 인덱스 매핑: 0 - Up (North), 1 - Right (East), 2 - Down (South)
    /// </summary>
    /// <param name="hullLevel">함선의 외갑판 레벨 (0: 레벨 1, 1: 레벨 2, 2: 레벨 3)</param>
    public void ApplyRotationSprite(int hullLevel = 0)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null || weaponData == null || weaponData.flattenedSprites == null)
            return;

        // 유효한 인덱스 범위 확인
        int dirIndex = GetDirectionIndex();
        int totalIndex = hullLevel * 3 + dirIndex;

        // 배열 범위 체크
        if (totalIndex >= 0 && totalIndex < weaponData.flattenedSprites.Length)
        {
            spriteRenderer.sprite = weaponData.flattenedSprites[totalIndex];
        }
        else if (weaponData.weaponIcon != null)
        {
            // 인덱스가 범위를 벗어나면 기본 아이콘 사용
            spriteRenderer.sprite = weaponData.weaponIcon;
            Debug.LogWarning(
                $"Invalid sprite index {totalIndex} for weapon {weaponData.GetWeaponName()}, using default icon");
        }
    }

    public void CopyFrom(ShipWeapon other)
    {
        if (other == null) return;

        weaponData = other.weaponData;
        firePoint = other.firePoint;
        gridPosition = other.gridPosition;
        gridSize = other.gridSize;
        attachedDirection = other.attachedDirection;

        currentCooldown = other.currentCooldown;
        hits = other.hits;
        totalDamageDealt = other.totalDamageDealt;
        isEnabled = other.isEnabled;
    }
}
