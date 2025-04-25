using UnityEngine;

public class ItemFactory : MonoBehaviour
{
    [SerializeField] private TradingItemDataBase itemDataBase;

    public GameObject ItemPrefabShape;

    private void Awake()
    {
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        if (itemDataBase != null)
            itemDataBase.InitializeDictionary();
        else
            Debug.LogError("Item Database not assigned!");
    }

    public TradingItemData GetItemData(int id)
    {
        return itemDataBase.GetItemData(id);
    }

    public GameObject CreateItemObject(int itemId, int quantity, Vector2Int position)
    {
        TradingItemData itemData = GetItemData(itemId);
        if (itemData == null) return null;

        GameObject itemObject = new($"Item_{itemId}");
        TradingItem itemInstance = itemObject.AddComponent<TradingItem>();
        itemInstance.Initialize(itemData, quantity);

        return itemObject;
    }

    public TradingItem CreateItemObject(TradingItem item)
    {
        GameObject itemObject = Instantiate(ItemPrefabShape);

        TradingItem itemComponent = itemObject.GetComponent<TradingItem>();
        if (itemComponent == null) itemComponent = item.gameObject.AddComponent<TradingItem>();

        itemComponent.CopyFrom(item);

        itemObject.name = $"item_{item.name}";

        return itemComponent;
    }

    public TradingItem CreateItemInstance(int itemId, int quantity)
    {
        TradingItemData itemData = GetItemData(itemId);
        if (itemData == null) return null;

        // 새로운 GameObject 생성
        GameObject itemObject = Instantiate(ItemPrefabShape);
        itemObject.name = "Item : " + itemData.debugName;

        TradingItem itemInstance = itemObject.GetComponent<TradingItem>();
        itemInstance.Initialize(itemData, quantity);


        return itemInstance;
    }
}
