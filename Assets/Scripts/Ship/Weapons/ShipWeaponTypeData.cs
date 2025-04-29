using UnityEngine;

/// <summary>
/// 무기 타입 데이터 클래스
/// </summary>
[CreateAssetMenu(fileName = "New Weapon Type", menuName = "Ship/Weapon/Type")]
public class ShipWeaponTypeData : ScriptableObject
{
    /// <summary>
    /// 타입 ID
    /// </summary>
    public int id;

    /// <summary>
    /// 타입 이름
    /// </summary>
    public string typeName;

    /// <summary>
    /// 타입 설명
    /// </summary>
    [TextArea(2, 5)] public string description;

    /// <summary>
    /// 타입 유형
    /// </summary>
    public ShipWeaponType weaponType;

    /// <summary>
    /// 타입 아이콘
    /// </summary>
    public Sprite icon;
}
