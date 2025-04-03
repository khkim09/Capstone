using UnityEngine;

/// <summary>
/// 실제 무기 인스턴스를 나타내는 클래스.
/// 무기 데이터와 쿨다운 상태를 포함하며, 발사 위치 및 각종 무기 정보를 제공합니다.
/// </summary>
public class ShipWeapon : MonoBehaviour
{
    /// <summary>
    /// 남은 쿨다운 시간입니다.
    /// </summary>
    private float cooldownRemaining = 0f;

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
    private Vector2Int gridPosition;

    /// <summary>
    /// 무기가 시설과 연결되어있는 방향(회전 상태)입니다.
    /// 무기는 항상 오른쪽으로 발사되어야 하므로 기본값은 East이며,
    /// 회전 상태는 East → South → North → East 순으로 순환합니다.
    /// </summary>
    private ShipWeaponAttachedDirection attachedDirection = ShipWeaponAttachedDirection.East;

    /// <summary>
    /// 각 회전 상태에 따른 스프라이트 배열입니다.
    /// 인덱스 매핑: 0 - Up (North), 1 - Right (East), 2 - Down (South)
    /// </summary>
    public Sprite[] rotationSprites;

    /// <summary>
    /// SpriteRenderer 컴포넌트 참조 (회전 시 스프라이트 갱신용)
    /// </summary>
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// 무기 데이터를 기반으로 무기를 초기화합니다.
    /// </summary>
    /// <param name="data">초기화할 무기 데이터.</param>
    public ShipWeapon(ShipWeaponData data)
    {
        weaponData = data;
        ResetCooldown();
    }

    private void Awake()
    {
        // SpriteRenderer 컴포넌트 가져오기
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 초기 스프라이트 적용
        ApplyRotationSprite();
    }

    /// <summary>
    /// 매 프레임 호출되어 쿨다운 시간을 감소시킵니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
    public void UpdateCooldown(float deltaTime)
    {
        if (cooldownRemaining > 0)
            cooldownRemaining -= deltaTime;
    }

    /// <summary>
    /// 무기가 발사 가능한 상태인지 확인합니다.
    /// </summary>
    /// <returns>쿨다운이 완료되었으면 true.</returns>
    public bool IsReady()
    {
        return cooldownRemaining <= 0;
    }

    /// <summary>
    /// 무기의 쿨다운을 기본값으로 초기화합니다.
    /// </summary>
    public void ResetCooldown()
    {
        cooldownRemaining = weaponData.GetBaseCooldown();
    }

    /// <summary>
    /// 무기의 쿨다운을 지정한 값으로 설정합니다.
    /// </summary>
    /// <param name="cooldown">설정할 쿨다운 시간.</param>
    public void ResetCooldown(float cooldown)
    {
        cooldownRemaining = cooldown;
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

    /// <summary>
    /// 무기의 기본 쿨다운 시간을 반환합니다.
    /// </summary>
    /// <returns>기본 쿨다운 시간.</returns>
    public float GetBaseCooldown()
    {
        return weaponData.GetBaseCooldown();
    }

    /// <summary>
    /// 무기의 타입을 반환합니다.
    /// </summary>
    /// <returns>무기 타입 enum 값.</returns>
    public WeaponType GetWeaponType()
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
        this.gridPosition = position;
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
    /// 무기가 시설과 연결된 방향(회전 상태)을 설정합니다.
    /// </summary>
    /// <param name="direction">설정할 방향.</param>
    public void SetAttachedDirection(ShipWeaponAttachedDirection direction)
    {
        this.attachedDirection = direction;
        ApplyRotationSprite();
    }

    /// <summary>
    /// 우클릭 시 호출되어 무기의 회전(힌지 회전) 상태를 변경합니다.
    /// 회전 상태는 East → South → North → East 순으로 순환합니다.
    /// </summary>
    public void RotateWeapon()
    {
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
    private void ApplyRotationSprite()
    {
        if (rotationSprites != null && rotationSprites.Length >= 3 && spriteRenderer != null)
        {
            int spriteIndex = 0;
            switch (attachedDirection)
            {
                case ShipWeaponAttachedDirection.North:
                    spriteIndex = 0;
                    break;
                case ShipWeaponAttachedDirection.East:
                    spriteIndex = 1;
                    break;
                case ShipWeaponAttachedDirection.South:
                    spriteIndex = 2;
                    break;
            }
            spriteRenderer.sprite = rotationSprites[spriteIndex];
        }
    }
}
