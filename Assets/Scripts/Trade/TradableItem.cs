using UnityEngine;

public enum ItemStorageType
{
    Normal,
    Temperature,
    Animal
}

[System.Serializable]
public class TradableItem
{
    public string itemName;
    public string description;
    public Sprite icon;

    public int basePrice; // 기본 가격
    public int currentPrice; // 현재 행성에서의 가격

    public float weight; // 무게
    public float volume; // 부피
    public ItemStorageType storageType; // 보관 조건
    public int maxStackSize; // 최대 적재량

    public string originPlanet; // 생산 행성

    // 가격 변동 계산
    public void UpdatePrice(float marketFactor)
    {
        // marketFactor: 행성 시장 상황에 따른 변동 요소 (0.5 ~ 1.5)
        currentPrice = Mathf.RoundToInt(basePrice * marketFactor);
    }
}
