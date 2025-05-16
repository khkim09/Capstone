using UnityEngine;

/// <summary>
/// 테스트용: 게임 시작 시 TradingItemDataBase에서 랜덤 아이템 여러 개를 Storage에 추가합니다.
/// </summary>
public class TestStorageItemAdder : MonoBehaviour
{
    /// <summary>랜덤 아이템을 뽑을 TradingItemDataBase</summary>
    public TradingItemDataBase itemDatabase;

    /// <summary>추가할 아이템 종류 수</summary>
    public int randomItemCount = 30;

    private void Start()
    {
        if (itemDatabase == null || itemDatabase.allItems == null || itemDatabase.allItems.Count == 0)
        {
            Debug.LogWarning("itemDatabase 또는 allItems가 비어있습니다.");
            return;
        }

        Storage storage = FindObjectOfType<Storage>();
        if (storage == null)
        {
            Debug.LogWarning("Storage를 찾을 수 없습니다.");
            return;
        }

        for (int i = 0; i < randomItemCount; i++)
        {
            // allItems에서 랜덤으로 하나 선택
            TradingItemData randomItem = itemDatabase.allItems[Random.Range(0, itemDatabase.allItems.Count)];

            // Storage에 1개 추가
            storage.AddItem(randomItem, 1);
            Debug.Log($"Storage에 랜덤 아이템 추가: {randomItem.itemName} x1");
        }
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.PopulateInventory(); // 또는 UpdateItemList(), Draw(), Setup(), 네 구조에 맞는 함수
        }

    }
}
