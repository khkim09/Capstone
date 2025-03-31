using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 창고에 저장된 개별 아이템 정보.
/// </summary>
[System.Serializable]
public class StoredItem
{
    /// <summary>
    /// 저장된 아이템입니다.
    /// </summary>
    public TradableItem item;

    /// <summary>
    /// 아이템의 수량입니다.
    /// </summary>
    public int quantity;

    /// <summary>
    /// 새로운 StoredItem 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="item">저장할 TradableItem 객체.</param>
    /// <param name="quantity">저장할 아이템의 수량.</param>
    public StoredItem(TradableItem item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}

/// <summary>
/// 창고 시스템을 관리하는 클래스입니다.
/// 각 아이템은 JSON 데이터셋에 정의된 최대 적층량(maxStackAmount) 이상의 수량을 저장할 수 없습니다.
/// </summary>
public class Storage : MonoBehaviour
{
    /// <summary>
    /// 창고에 저장된 아이템 목록입니다.
    /// </summary>
    public List<StoredItem> storedItems = new List<StoredItem>();

    /// <summary>
    /// 주어진 아이템을 추가할 수 있는지 확인합니다.
    /// </summary>
    /// <param name="item">추가하려는 TradableItem 객체.</param>
    /// <param name="quantity">추가하려는 수량.</param>
    /// <returns>아이템을 추가할 수 있으면 true, 그렇지 않으면 false.</returns>
    public bool CanAddItem(TradableItem item, int quantity)
    {
        StoredItem stored = storedItems.Find(x => x.item.itemName == item.itemName);
        int currentQuantity = stored != null ? stored.quantity : 0;
        return (currentQuantity + quantity) <= item.maxStackAmount;
    }

    /// <summary>
    /// 창고에 아이템을 추가합니다.
    /// 이미 저장된 아이템의 경우, 총 수량이 maxStackAmount를 넘지 않는지 확인합니다.
    /// </summary>
    /// <param name="item">추가할 TradableItem 객체.</param>
    /// <param name="quantity">추가할 수량.</param>
    /// <returns>아이템 추가가 성공하면 true, 그렇지 않으면 false.</returns>
    public bool AddItem(TradableItem item, int quantity)
    {
        // 동일한 아이템이 이미 저장되어 있는지 확인 (여기서는 itemName을 고유키로 사용)
        StoredItem stored = storedItems.Find(x => x.item.itemName == item.itemName);
        if (stored != null)
        {
            if (stored.quantity + quantity > item.maxStackAmount)
            {
                Debug.LogWarning($"Cannot add {quantity} of {item.itemName}. Exceeds max stacking amount ({item.maxStackAmount}).");
                return false;
            }
            stored.quantity += quantity;
        }
        else
        {
            if (quantity > item.maxStackAmount)
            {
                Debug.LogWarning($"Cannot add {quantity} of {item.itemName}. Quantity exceeds max stacking amount ({item.maxStackAmount}).");
                return false;
            }
            storedItems.Add(new StoredItem(item, quantity));
        }
        Debug.Log($"Added {quantity} of {item.itemName} to storage. Total now: {GetItemQuantity(item)}");
        return true;
    }

    /// <summary>
    /// 창고에 해당 아이템이 일정 수량 이상 있는지 확인합니다.
    /// </summary>
    /// <param name="item">확인할 TradableItem 객체.</param>
    /// <param name="quantity">요청하는 수량.</param>
    /// <returns>창고에 아이템이 충분하면 true, 그렇지 않으면 false.</returns>
    public bool HasItem(TradableItem item, int quantity)
    {
        StoredItem stored = storedItems.Find(x => x.item.itemName == item.itemName);
        return stored != null && stored.quantity >= quantity;
    }

    /// <summary>
    /// 창고에서 지정한 아이템의 수량을 제거합니다.
    /// </summary>
    /// <param name="item">제거할 TradableItem 객체.</param>
    /// <param name="quantity">제거할 수량.</param>
    /// <returns>아이템 제거가 성공하면 true, 그렇지 않으면 false.</returns>
    public bool RemoveItem(TradableItem item, int quantity)
    {
        StoredItem stored = storedItems.Find(x => x.item.itemName == item.itemName);
        if (stored != null && stored.quantity >= quantity)
        {
            stored.quantity -= quantity;
            if (stored.quantity <= 0)
            {
                storedItems.Remove(stored);
            }
            Debug.Log($"Removed {quantity} of {item.itemName} from storage. Remaining: {GetItemQuantity(item)}");
            return true;
        }
        Debug.LogWarning($"Cannot remove {quantity} of {item.itemName}. Not enough in storage.");
        return false;
    }

    /// <summary>
    /// 창고에 저장된 특정 아이템의 총 수량을 반환합니다.
    /// </summary>
    /// <param name="item">확인할 TradableItem 객체.</param>
    /// <returns>저장된 수량. 아이템이 없으면 0을 반환합니다.</returns>
    public int GetItemQuantity(TradableItem item)
    {
        StoredItem stored = storedItems.Find(x => x.item.itemName == item.itemName);
        return stored != null ? stored.quantity : 0;
    }
}
