using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Storage는 플레이어가 보유한 무역 아이템을 관리하는 클래스입니다.
/// 아이템은 itemId(int)를 키로 사용하여 TradingItemData와 수량(int)을 저장합니다.
/// </summary>
public class Storage : MonoBehaviour
{
    /// <summary>싱글톤 인스턴스입니다.</summary>
    public static Storage Instance { get; private set; }

    /// <summary>창고에 저장된 아이템 목록 (Key: item ID, Value: (itemData, quantity))</summary>
    private Dictionary<int, (TradingItemData itemData, int quantity)> storedItems
        = new Dictionary<int, (TradingItemData, int)>();

    /// <summary>아이템 수량 변동 시 발생하는 이벤트입니다.</summary>
    public event Action<TradingItemData> OnStorageChanged;

    /// <summary>퀘스트 연동용 TradingItemDataBase입니다.</summary>
    [Header("Quest 연동용 ItemDatabase")]
    [SerializeField] private TradingItemDataBase itemDatabase;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (itemDatabase == null)
            itemDatabase = Resources.Load<TradingItemDataBase>("ItemDatabase");
    }

    /// <summary>
    /// 해당 아이템을 추가할 수 있는지 검사합니다.
    /// </summary>
    public bool CanAddItem(TradingItemData itemData, int quantity)
    {
        int current = storedItems.ContainsKey(itemData.id) ? storedItems[itemData.id].quantity : 0;
        return (current + quantity) <= itemData.capacity;
    }

    /// <summary>
    /// 아이템을 창고에 추가합니다.
    /// </summary>
    public bool AddItem(TradingItemData itemData, int quantity)
    {
        if (!CanAddItem(itemData, quantity))
        {
            Debug.LogWarning($"[{itemData.itemName}] 최대 적재량 초과");
            return false;
        }

        if (storedItems.ContainsKey(itemData.id))
        {
            storedItems[itemData.id] = (itemData, storedItems[itemData.id].quantity + quantity);
        }
        else
        {
            storedItems.Add(itemData.id, (itemData, quantity));
        }

        Debug.Log($"[Storage] {itemData.itemName} x{quantity} 추가 (총 {GetItemQuantity(itemData)})");
        OnStorageChanged?.Invoke(itemData);
        return true;
    }

    /// <summary>
    /// 아이템을 창고에서 제거합니다.
    /// </summary>
    public bool RemoveItem(TradingItemData itemData, int quantity)
    {
        if (!storedItems.ContainsKey(itemData.id))
        {
            Debug.LogWarning($"[Storage] {itemData.itemName} 제거 실패 (보유하지 않음)");
            return false;
        }

        var current = storedItems[itemData.id];
        if (current.quantity < quantity)
        {
            Debug.LogWarning($"[Storage] {itemData.itemName} 제거 실패 (수량 부족)");
            return false;
        }

        int remaining = current.quantity - quantity;
        if (remaining > 0)
        {
            storedItems[itemData.id] = (itemData, remaining);
        }
        else
        {
            storedItems.Remove(itemData.id);
        }

        Debug.Log($"[Storage] {itemData.itemName} x{quantity} 제거 (남은 {remaining})");
        OnStorageChanged?.Invoke(itemData);
        return true;
    }

    /// <summary>
    /// 아이템 보유 여부를 검사합니다.
    /// </summary>
    public bool HasItem(TradingItemData itemData, int quantity)
    {
        return storedItems.ContainsKey(itemData.id) && storedItems[itemData.id].quantity >= quantity;
    }

    /// <summary>
    /// 특정 아이템의 현재 보유 수량을 반환합니다.
    /// </summary>
    public int GetItemQuantity(TradingItemData itemData)
    {
        return storedItems.ContainsKey(itemData.id) ? storedItems[itemData.id].quantity : 0;
    }

    /// <summary>
    /// 퀘스트 UI용 ID 기반 수량 조회
    /// </summary>
    public int GetItemQuantityById(string targetId)
    {
        if (itemDatabase == null)
        {
            Debug.LogError("[Storage] ItemDatabase가 할당되지 않았습니다!");
            return 0;
        }

        if (!int.TryParse(targetId, out var id))
        {
            Debug.LogWarning($"[Storage] 잘못된 ID 포맷: {targetId}");
            return 0;
        }

        var data = itemDatabase.GetItemData(id);
        if (data == null)
        {
            Debug.LogWarning($"[Storage] 데이터베이스에 ID {id} 아이템이 없습니다.");
            return 0;
        }

        return GetItemQuantity(data);
    }

    /// <summary>
    /// 저장된 모든 아이템 목록을 반환합니다.
    /// </summary>
    public List<(TradingItemData itemData, int quantity)> GetAllItems()
    {
        List<(TradingItemData, int)> result = new List<(TradingItemData, int)>();
        foreach (var entry in storedItems.Values)
            result.Add(entry);
        return result;
    }
}
