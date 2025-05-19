using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Item/Item Database")]
public class TradingItemDataBase : ScriptableObject
{
    public List<TradingItemData> allItems = new();

    // 아이템 검색용 딕셔너리 (런타임에만 사용)
    private Dictionary<int, TradingItemData> itemDictionary;

    public void InitializeDictionary()
    {
        itemDictionary = new Dictionary<int, TradingItemData>();
        foreach (TradingItemData item in allItems) itemDictionary[item.id] = item;
    }

    public TradingItemData GetItemData(int id)
    {
        if (itemDictionary == null) InitializeDictionary();

        if (itemDictionary.TryGetValue(id, out TradingItemData item)) return item;
        return null;
    }

    public List<TradingItemData> GetItemsByPlanet(ItemPlanet planetName)
    {
        List<TradingItemData> result = new();
        foreach (TradingItemData item in allItems)
            if (item.planet == planetName)
                result.Add(item);

        return result;
    }

    public List<TradingItemData> GetItemsByTier(ItemTierLevel tier)
    {
        List<TradingItemData> result = new();
        foreach (TradingItemData item in allItems)
            if (item.tier == tier)
                result.Add(item);

        return result;
    }

    public TradingItemData GetRandomItem()
    {
        int index = Random.Range(0, allItems.Count);
        return allItems[index];
    }
}
