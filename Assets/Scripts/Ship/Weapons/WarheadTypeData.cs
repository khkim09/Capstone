using UnityEngine;

/// <summary>
/// 탄두 타입 데이터 클래스
/// </summary>
[CreateAssetMenu(fileName = "New Warhead Type", menuName = "Ship/Weapon/Warhead")]
public class WarheadTypeData : ScriptableObject
{
    /// <summary>
    /// 탄두 타입 ID
    /// </summary>
    public int id;

    /// <summary>
    /// 탄두 이름
    /// </summary>
    public string warheadName;

    /// <summary>
    /// 탄두 설명
    /// </summary>
    [TextArea(2, 5)] public string description;

    /// <summary>
    /// 탄두 유형
    /// </summary>
    public WarheadType warheadType;

    /// <summary>
    /// 탄두 아이콘
    /// </summary>
    public Sprite icon;
}
