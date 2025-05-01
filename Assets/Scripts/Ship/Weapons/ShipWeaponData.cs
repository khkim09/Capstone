using UnityEngine;
using System;

/// <summary>
/// 무기 정보를 저장하는 ScriptableObject.
/// 이름, 데미지, 쿨다운, 무기 종류 등의 데이터를 포함합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "Ship/Weapon/Weapon")]
public class ShipWeaponData : ScriptableObject
{
    /// <summary>
    /// 무기 고유 ID
    /// </summary>
    [SerializeField] public int id;

    /// <summary>
    /// 무기 이름입니다.
    /// </summary>
    [SerializeField] public string weaponName;

    /// <summary>
    /// 무기 설명(툴팁 등에 사용)
    /// </summary>
    [SerializeField] [TextArea(3, 5)] public string description;

    /// <summary>
    /// 무기의 기본 데미지 값입니다.
    /// </summary>
    [SerializeField] public float damage;

    /// <summary>
    /// 무기의 초당 쿨타임 증가량입니다.
    /// </summary>
    public float cooldownPerSecond;

    /// <summary>
    /// 무기 인벤토리 아이콘
    /// </summary>
    [SerializeField] public Sprite weaponIcon;

    /// <summary>
    /// 무기 설계도 스프라이트 (외갑판 레벨[0-2], 방향[East, South, North])
    /// [0,0] = 외갑판 레벨 1, East 방향
    /// [0,1] = 외갑판 레벨 1, South 방향
    /// [0,2] = 외갑판 레벨 1, North 방향
    /// [1,0] = 외갑판 레벨 2, East 방향
    /// ...
    /// [2,2] = 외갑판 레벨 3, North 방향
    /// </summary>
    [SerializeField] public Sprite[,] blueprintSprites = new Sprite[3, 3];

    /// <summary>
    /// Inspector에서 표시할 1차원 스프라이트 배열 (외갑판 레벨 1 East, South, North, 레벨 2 East, South, North, ...)
    /// </summary>
    [SerializeField] public Sprite[] flattenedSprites = new Sprite[9];

    /// <summary>
    /// 무기의 타입입니다.
    /// </summary>
    [SerializeField] public ShipWeaponTypeData weaponType;

    /// <summary>
    /// 무기의 효과입니다.
    /// </summary>
    public WeaponEffectData effectData;


    /// <summary>
    /// 추가 효과 강도 (퍼센트 또는 절대값)
    /// </summary>
    [SerializeField] public float effectPower;

    /// <summary>
    /// 무기 설치 비용
    /// </summary>
    [SerializeField] public int cost;

    /// <summary>
    /// 필요한 탄두 종류
    /// </summary>
    public WarheadTypeData warheadType;

    /// <summary>
    /// 무기 이름을 반환합니다.
    /// </summary>
    /// <returns>무기 이름 문자열.</returns>
    public string GetWeaponName()
    {
        return weaponName;
    }

    /// <summary>
    /// 무기의 데미지를 반환합니다.
    /// </summary>
    /// <returns>기본 데미지 값.</returns>
    public float GetDamage()
    {
        return damage;
    }


    /// <summary>
    /// 무기의 초당 쿨타임 증가량을 반환합니다.
    /// </summary>
    /// <returns>초당 쿨타임 증가량.</returns>
    public float GetCooldownPerSecond()
    {
        return cooldownPerSecond;
    }

    /// <summary>
    /// 무기의 타입을 반환합니다.
    /// </summary>
    /// <returns>무기 타입 enum 값.</returns>
    public ShipWeaponType GetWeaponType()
    {
        return weaponType.weaponType;
    }

    /// <summary>
    /// 1차원 배열에서 2차원 배열로 변환 (Inspector에서 사용)
    /// </summary>
    public void OnValidate()
    {
        if (flattenedSprites.Length != 9)
            Array.Resize(ref flattenedSprites, 9);

        // 1차원 배열을 2차원 배열로 변환
        blueprintSprites = new Sprite[3, 3];
        for (int hull = 0; hull < 3; hull++)
        for (int dir = 0; dir < 3; dir++)
        {
            int index = hull * 3 + dir;
            if (index < flattenedSprites.Length)
                blueprintSprites[hull, dir] = flattenedSprites[index];
        }
    }
}
