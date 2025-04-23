using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 창고에 저장된 개별 아이템 정보입니다.
/// </summary>
[System.Serializable]
public class StoredItem
{
    /// <summary>
    /// 저장된 아이템 데이터입니다.
    /// </summary>
    public TradingItemData itemData;

    /// <summary>
    /// 아이템의 수량입니다.
    /// </summary>
    public int quantity;

    /// <summary>
    /// 새로운 StoredItem 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="itemData">저장할 아이템 데이터</param>
    /// <param name="quantity">저장할 수량</param>
    public StoredItem(TradingItemData itemData, int quantity)
    {
        this.itemData = itemData;
        this.quantity = quantity;
    }
}

/// <summary>
/// Storage는 플레이어의 창고 시스템을 관리하는 클래스입니다.
/// 각 아이템은 TradingItemData에 정의된 최대 적층량 이상 저장할 수 없습니다.
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
    /// <param name="itemData">추가하려는 아이템 데이터</param>
    /// <param name="quantity">추가하려는 수량</param>
    /// <returns>추가 가능 여부</returns>
    public bool CanAddItem(TradingItemData itemData, int quantity)
    {
        StoredItem stored = storedItems.Find(x => x.itemData.itemName == itemData.itemName);
        int currentQuantity = stored != null ? stored.quantity : 0;
        return (currentQuantity + quantity) <= itemData.capacity;
    }

    /// <summary>
    /// 창고에 아이템을 추가합니다.
    /// 이미 저장된 아이템이라면 수량을 누적하며, 최대 적층량을 초과할 수 없습니다.
    /// </summary>
    /// <param name="itemData">추가할 아이템 데이터</param>
    /// <param name="quantity">추가할 수량</param>
    /// <returns>성공 여부</returns>
    public bool AddItem(TradingItemData itemData, int quantity)
    {
        StoredItem stored = storedItems.Find(x => x.itemData.id == itemData.id);
        if (stored != null)
        {
            if (stored.quantity + quantity > itemData.capacity)
            {
                Debug.LogWarning($"최대 적재량({itemData.capacity}) 초과로 {itemData.itemName} x {quantity} 추가 불가");
                return false;
            }
            stored.quantity += quantity;
        }
        else
        {
            if (quantity > itemData.capacity)
            {
                Debug.LogWarning($"최대 적재량({itemData.capacity}) 초과로 {itemData.itemName} x {quantity} 추가 불가");
                return false;
            }
            storedItems.Add(new StoredItem(itemData, quantity));
        }

        Debug.Log($"창고에 {itemData.itemName} x {quantity} 추가됨. 총 수량: {GetItemQuantity(itemData)}");
        return true;
    }

    /// <summary>
    /// 창고에 해당 아이템이 일정 수량 이상 있는지 확인합니다.
    /// </summary>
    /// <param name="itemData">확인할 아이템 데이터</param>
    /// <param name="quantity">요구 수량</param>
    /// <returns>충분한 수량 여부</returns>
    public bool HasItem(TradingItemData itemData, int quantity)
    {
        StoredItem stored = storedItems.Find(x => x.itemData.itemName == itemData.itemName);
        return stored != null && stored.quantity >= quantity;
    }

    /// <summary>
    /// 창고에서 지정한 아이템의 수량을 제거합니다.
    /// 수량이 0 이하가 되면 아이템 자체를 제거합니다.
    /// </summary>
    /// <param name="itemData">제거할 아이템 데이터</param>
    /// <param name="quantity">제거할 수량</param>
    /// <returns>성공 여부</returns>
    public bool RemoveItem(TradingItemData itemData, int quantity)
    {
        StoredItem stored = storedItems.Find(x => x.itemData.itemName == itemData.itemName);
        if (stored != null && stored.quantity >= quantity)
        {
            stored.quantity -= quantity;
            if (stored.quantity <= 0)
            {
                storedItems.Remove(stored);
            }

            Debug.Log($"창고에서 {itemData.itemName} x {quantity} 제거됨. 남은 수량: {GetItemQuantity(itemData)}");
            return true;
        }

        Debug.LogWarning($"{itemData.itemName} x {quantity} 제거 실패: 수량 부족");
        return false;
    }

    /// <summary>
    /// 창고에 저장된 특정 아이템의 총 수량을 반환합니다.
    /// </summary>
    /// <param name="itemData">확인할 아이템 데이터</param>
    /// <returns>해당 아이템의 저장 수량</returns>
    public int GetItemQuantity(TradingItemData itemData)
    {
        StoredItem stored = storedItems.Find(x => x.itemData.itemName == itemData.itemName);
        return stored != null ? stored.quantity : 0;
    }
}
