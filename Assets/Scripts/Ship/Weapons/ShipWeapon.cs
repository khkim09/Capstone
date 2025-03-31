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
    /// 무기 데이터를 기반으로 무기를 초기화합니다.
    /// </summary>
    /// <param name="data">초기화할 무기 데이터.</param>
    public ShipWeapon(ShipWeaponData data)
    {
        weaponData = data;
        ResetCooldown();
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
    /// 무기 이름을 반환합니다.
    /// </summary>
    /// <returns>무기 이름 문자열.</returns>
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
    /// 무기의 타입을 반환합니다.
    /// </summary>
    /// <returns>무기 타입 enum 값.</returns>
    public Vector2 GetFirePosition()
    {
        return new Vector2(firePoint.position.x, firePoint.position.y);
    }
}
