using UnityEngine;

/// <summary>
/// 무기 효과 데이터 클래스
/// </summary>
[CreateAssetMenu(fileName = "New Weapon Effect", menuName = "Ship/Weapon Effect")]
public class WeaponEffectData : ScriptableObject
{
    /// <summary>
    /// 효과 ID
    /// </summary>
    public int id;

    /// <summary>
    /// 효과 이름
    /// </summary>
    public string effectName;

    /// <summary>
    /// 효과 설명
    /// </summary>
    [TextArea(2, 5)]
    public string description;

    /// <summary>
    /// 효과 유형
    /// </summary>
    public ShipWeaponEffectType effectType;

    /// <summary>
    /// 효과 아이콘
    /// </summary>
    public Sprite icon;
}
