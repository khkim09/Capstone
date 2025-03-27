using UnityEngine;

public enum ItemPlanet
{
    SIS,
    CCK,
    ICM,
    RCE,
    KTL
}

public enum ItemTierLevel
{
    T1,
    T2,
    T3
}

public enum ItemCategory
{
    Mineral, // 광물
    Livestock, // 동물
    Weapon, // 무기
    Gem, // 보석
    Luxury, // 사치품
    Material, // 소재
    Food, // 식량
    Artifact, // 유물
    Spice // 향신료
}

public enum ItemState
{
    Normal, // 정상
    SlightlyDamaged, // 조금 훼손됨 (가치 25% 하락)
    ModeratelyDamaged, // 다소 훼손됨 (가치 50% 하락)
    Unsellable // 판매 불가 (가치 100% 하락)
}

[CreateAssetMenu(fileName = "TradableItems", menuName = "Trade/TradableItems")]
public class ItemTypes : ScriptableObject
{
    [Header("Item Information")]
    public ItemPlanet planetType;
    public ItemTierLevel tierType;
    public ItemCategory categoryType;
    public ItemState stateType;
    public string displayName;

    [Header("Item Attributes")]
    public float minimumTemperature;
    public float maximumTemperature;
    public int price;
    public float maxStackAmount;
    public int fluctuation;
    public string description;
}
