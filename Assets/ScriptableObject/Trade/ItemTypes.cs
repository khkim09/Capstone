using UnityEngine;

/// <summary>
/// 행성을 나타내는 열거형입니다.
/// </summary>
public enum ItemPlanet
{
    /// <summary>SIS 행성</summary>
    SIS,

    /// <summary>CCK 행성</summary>
    CCK,

    /// <summary>ICM 행성</summary>
    ICM,

    /// <summary>RCE 행성</summary>
    RCE,

    /// <summary>KTL 행성</summary>
    KTL,

    /// <summary>기본</summary>
    Default
}

/// <summary>
/// 아이템의 티어(등급)를 나타내는 열거형입니다.
/// </summary>
public enum ItemTierLevel
{
    /// <summary>T1 티어</summary>
    T1,

    /// <summary>T2 티어</summary>
    T2,

    /// <summary>T3 티어</summary>
    T3,

    /// <summary>기본</summary>
    Default
}

/// <summary>
/// 아이템의 분류를 나타내는 열거형입니다.
/// </summary>
public enum ItemCategory
{
    /// <summary>광물</summary>
    Mineral,

    /// <summary>동물</summary>
    Livestock,

    /// <summary>무기</summary>
    Weapon,

    /// <summary>보석</summary>
    Gem,

    /// <summary>사치품</summary>
    Luxury,

    /// <summary>소재</summary>
    Material,

    /// <summary>식량</summary>
    Food,

    /// <summary>유물</summary>
    Artifact,

    /// <summary>향신료</summary>
    Spice,

    /// <summary>기본</summary>
    Default
}

/// <summary>
/// 아이템의 상태를 나타내는 열거형입니다.
/// </summary>
public enum ItemState
{
    /// <summary>정상</summary>
    Normal,

    /// <summary>조금 훼손됨 (가치 25% 하락)</summary>
    SlightlyDamaged,

    /// <summary>다소 훼손됨 (가치 50% 하락)</summary>
    ModeratelyDamaged,

    /// <summary>판매 불가 (가치 100% 하락)</summary>
    Unsellable
}

/// <summary>
/// ItemTypes는 ScriptableObject를 기반으로 하여, 무역에 사용되는 아이템의 정보를 저장합니다.
/// 여기에는 아이템의 기본 정보와 속성이 포함됩니다.
/// </summary>
[CreateAssetMenu(fileName = "TradableItems", menuName = "Trade/TradableItems")]
public class ItemTypes : ScriptableObject
{
    #region Item Information

    /// <summary>
    /// 아이템이 속한 행성 타입입니다.
    /// </summary>
    [Header("Item Information")] public ItemPlanet planetType;

    /// <summary>
    /// 아이템의 티어(등급)입니다.
    /// </summary>
    public ItemTierLevel tierType;

    /// <summary>
    /// 아이템의 분류입니다.
    /// </summary>
    public ItemCategory categoryType;

    /// <summary>
    /// 아이템의 상태를 나타냅니다.
    /// </summary>
    public ItemState stateType;

    /// <summary>
    /// 아이템의 표시용 이름입니다.
    /// </summary>
    public string displayName;

    #endregion

    #region Item Attributes

    /// <summary>
    /// 아이템의 최소 보관 온도 (℃)입니다.
    /// </summary>
    [Header("Item Attributes")] public float minimumTemperature;

    /// <summary>
    /// 아이템의 최대 보관 온도 (℃)입니다.
    /// </summary>
    public float maximumTemperature;

    /// <summary>
    /// 아이템의 기본 가격입니다.
    /// </summary>
    public int price;

    /// <summary>
    /// 아이템의 최대 적층량입니다.
    /// </summary>
    public float maxStackAmount;

    /// <summary>
    /// 가격 변동폭을 나타냅니다.
    /// </summary>
    public int fluctuation;

    /// <summary>
    /// 아이템에 대한 설명입니다.
    /// </summary>
    public string description;

    #endregion
}
