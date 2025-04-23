using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>
/// TestPlayerItems는 테스트 목적으로 플레이어에게 무작위 아이템을 지급하는 기능을 제공합니다.
/// TradeDataLoader로부터 무역 아이템 데이터를 불러와 Storage에 아이템을 추가합니다.
/// </summary>
public class TestPlayerItems : MonoBehaviour
{
    /// <summary>
    /// 무역 아이템 데이터를 로드하는 TradeDataLoader 컴포넌트입니다.
    /// </summary>
    [SerializeField] private TradeDataLoader tradeDataLoader;

    /// <summary>
    /// 플레이어의 창고를 담당하는 Storage 컴포넌트입니다.
    /// </summary>
    [SerializeField] private Storage storage;

    /// <summary>
    /// 지급할 아이템 수량입니다.
    /// </summary>
    [SerializeField] private int numberOfItemsToGive = 20;

    [SerializeField] private TradingItemDataBase itemDatabase;

    /// <summary>
    /// 시작 시 TradeDataLoader에서 무작위 아이템을 가져와 창고에 지급하고 인벤토리를 갱신합니다.
    /// </summary>
    private void Start()
    {
        if (tradeDataLoader == null)
            tradeDataLoader = FindObjectOfType<TradeDataLoader>();

        if (storage == null)
            storage = FindObjectOfType<Storage>();

        if (tradeDataLoader == null || storage == null)
        {
            Debug.LogError("TestPlayerItems: 필요한 컴포넌트가 할당되지 않았습니다.");
            return;
        }

        List<TradingItemData> allItems = itemDatabase.allItems;
        if (allItems == null || allItems.Count == 0)
        {
            Debug.LogError("TestPlayerItems: 무역 아이템 데이터가 로드되지 않았습니다.");
            return;
        }

        for (int i = 0; i < numberOfItemsToGive; i++)
        {
            TradingItemData randomItem = allItems[Random.Range(0, allItems.Count)];
            int quantityToGive = Mathf.Max(1, 1);
            bool added = storage.AddItem(randomItem, quantityToGive);
            Debug.Log($"TestPlayerItems: {randomItem.itemName} x {quantityToGive} 지급 - 성공 여부: {added}");
        }

        InventoryUI invUI = FindObjectOfType<InventoryUI>();
        if (invUI != null)
        {
            invUI.PopulateInventory();
        }

        // 흑백 처리 (선택적으로 주석 해제하여 사용 가능)
        /*
        Ship playerShip = FindObjectOfType<Ship>();
        if (playerShip == null)
        {
            Debug.LogWarning("[TestPlayerItems] 씬에서 Ship을 찾을 수 없습니다.");
            return;
        }

        StorageSystem storageSystem = playerShip.StorageSystem;
        if (storageSystem == null)
        {
            Debug.LogWarning("[TestPlayerItems] Ship에 StorageSystem이 할당되지 않았습니다.");
            return;
        }

        storageSystem.SetOtherRoomsGray();
        */
    }
}
