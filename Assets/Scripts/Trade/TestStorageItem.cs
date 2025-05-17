using UnityEngine;

/// <summary>
/// 테스트용: 게임 시작 시 TradingItemDataBase에서 랜덤 아이템 여러 개를 Storage에 추가합니다.
/// </summary>
public class TestStorageItemAdder : MonoBehaviour
{
    /// <summary>
    /// 랜덤 아이템을 뽑을 TradingItemDataBase입니다.
    /// </summary>
    [SerializeField] private TradingItemDataBase itemDatabase;

    /// <summary>
    /// 추가할 아이템 종류 수입니다.
    /// </summary>
    [SerializeField] private int randomItemCount = 30;

    /// <summary>
    /// 시작 시 랜덤 아이템을 Storage에 추가하고 InventoryUI를 갱신합니다.
    /// </summary>
    private void Start()
    {
        // 데이터 유효성 확인
        if (itemDatabase == null || itemDatabase.allItems == null || itemDatabase.allItems.Count == 0)
        {
            Debug.LogWarning("[TestStorageItemAdder] itemDatabase 또는 allItems가 비어있습니다.");
            return;
        }

        // Storage 찾기
        Storage storage = FindObjectOfType<Storage>();
        if (storage == null)
        {
            Debug.LogWarning("[TestStorageItemAdder] Storage를 찾을 수 없습니다.");
            return;
        }

        // 지정된 수만큼 랜덤 아이템 추가
        for (int i = 0; i < randomItemCount; i++)
        {
            TradingItemData randomItem = itemDatabase.allItems[Random.Range(0, itemDatabase.allItems.Count)];
            bool added = storage.AddItem(randomItem, 1);

            if (added)
                Debug.Log($"[TestStorageItemAdder] Storage에 랜덤 아이템 추가: {randomItem.itemName} x1");
            else
                Debug.LogWarning($"[TestStorageItemAdder] {randomItem.itemName} 추가 실패 (적재 초과 가능성)");
        }

        // Inventory UI 갱신
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.PopulateInventory();
        }
    }
}
