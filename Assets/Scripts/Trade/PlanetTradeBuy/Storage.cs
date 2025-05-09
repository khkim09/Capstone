using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StoredItem
{
    public TradingItemData itemData;  // 아이템 데이터
    public int quantity;              // 수량

    public StoredItem(TradingItemData itemData, int quantity)
    {
        this.itemData = itemData;
        this.quantity = quantity;
    }
}

public class Storage : MonoBehaviour
{
    public static Storage Instance { get; private set; }

    /// <summary>
    /// 창고에 저장된 아이템 목록
    /// </summary>
    public List<StoredItem> storedItems = new List<StoredItem>();

    /// <summary>
    /// 아이템 수량 변동 시 발생
    /// </summary>
    public event Action<TradingItemData> OnStorageChanged;

    // ─── 추가 부분 시작 ───
    [Header("Quest 연동용 ItemDatabase")]
    [SerializeField] private TradingItemDataBase itemDatabase;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // ItemDatabase 할당이 안 되어 있으면 Resources에서 자동 로드
        if (itemDatabase == null)
            itemDatabase = Resources.Load<TradingItemDataBase>("ItemDatabase");
    }
    // ─── 추가 부분 끝 ───

    public bool CanAddItem(TradingItemData itemData, int quantity)
    {
        var stored = storedItems.Find(x => x.itemData.id == itemData.id);
        int current = stored?.quantity ?? 0;
        return (current + quantity) <= itemData.capacity;
    }

    public bool AddItem(TradingItemData itemData, int quantity)
    {
        var stored = storedItems.Find(x => x.itemData.id == itemData.id);
        if (stored != null)
        {
            if (stored.quantity + quantity > itemData.capacity)
            {
                Debug.LogWarning($"[{itemData.itemName}] 최대 적재량 초과");
                return false;
            }
            stored.quantity += quantity;
        }
        else
        {
            if (quantity > itemData.capacity)
            {
                Debug.LogWarning($"[{itemData.itemName}] 최대 적재량 초과");
                return false;
            }
            storedItems.Add(new StoredItem(itemData, quantity));
        }

        Debug.Log($"[Storage] {itemData.itemName} x{quantity} 추가 (총 {GetItemQuantity(itemData)})");
        OnStorageChanged?.Invoke(itemData);
        return true;
    }

    public bool RemoveItem(TradingItemData itemData, int quantity)
    {
        var stored = storedItems.Find(x => x.itemData.id == itemData.id);
        if (stored != null && stored.quantity >= quantity)
        {
            stored.quantity -= quantity;
            if (stored.quantity <= 0)
                storedItems.Remove(stored);

            Debug.Log($"[Storage] {itemData.itemName} x{quantity} 제거 (남은 {GetItemQuantity(itemData)})");
            OnStorageChanged?.Invoke(itemData);
            return true;
        }

        Debug.LogWarning($"[Storage] {itemData.itemName} 제거 실패 (수량 부족)");
        return false;
    }

    public bool HasItem(TradingItemData itemData, int quantity)
    {
        var stored = storedItems.Find(x => x.itemData.id == itemData.id);
        return stored != null && stored.quantity >= quantity;
    }

    public int GetItemQuantity(TradingItemData itemData)
    {
        var stored = storedItems.Find(x => x.itemData.id == itemData.id);
        return stored?.quantity ?? 0;
    }

    // ─── 추가 부분 시작 ───
    /// <summary>
    /// QuestUIManager용: targetId(string)로 창고 내 수량 조회
    /// </summary>
    public int GetItemQuantityById(string targetId)
    {
        if (itemDatabase == null)
        {
            Debug.LogError("[Storage] ItemDatabase가 할당되지 않았습니다!");
            return 0;
        }

        // 문자열 ID를 int로 변환
        if (!int.TryParse(targetId, out var id))
        {
            Debug.LogWarning($"[Storage] 잘못된 ID 포맷: {targetId}");
            return 0;
        }

        // 데이터베이스에서 TradingItemData 조회
        var data = itemDatabase.GetItemData(id);
        if (data == null)
        {
            Debug.LogWarning($"[Storage] 데이터베이스에 ID {id} 아이템이 없습니다.");
            return 0;
        }

        // 기존 메서드로 수량 반환
        return GetItemQuantity(data);
    }
    // ─── 추가 부분 끝 ───
}
