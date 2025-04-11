using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>
/// TestPlayerItems는 테스트 목적으로 플레이어에게 임의의 아이템을 지급하는 기능을 제공합니다.
/// TradeDataLoader로부터 무역 아이템 데이터를 로드하고, Storage 컴포넌트를 통해 아이템을 창고에 추가합니다.
/// </summary>
public class TestSell : MonoBehaviour
{
    /// <summary>
    /// 무역 아이템 데이터를 로드하는 TradeDataLoader 컴포넌트입니다.
    /// </summary>
    [SerializeField] private TradeDataLoader tradeDataLoader;

    /// <summary>
    /// 플레이어 창고를 관리하는 Storage 컴포넌트입니다.
    /// </summary>
    [SerializeField] private Storage storage;

    /// <summary>
    /// 지급할 아이템의 개수입니다.
    /// </summary>
    [SerializeField] private int numberOfItemsToGive = 20;

    /// <summary>
    /// MonoBehaviour의 Start 메서드로, 필요한 컴포넌트를 확인하고 무작위 아이템을 창고에 지급합니다.
    /// </summary>
    private void Start()
    {
        // tradeDataLoader와 warehouse가 할당되어 있는지 확인
        if (tradeDataLoader == null)
            tradeDataLoader = FindObjectOfType<TradeDataLoader>();

        if (storage == null)
            storage = FindObjectOfType<Storage>();

        if (tradeDataLoader == null || storage == null)
        {
            Debug.LogError("TestPlayerItems: 필요한 컴포넌트가 할당되지 않았습니다.");
            return;
        }

        List<TradableItem> allItems = tradeDataLoader.tradableItems;
        if (allItems == null || allItems.Count == 0)
        {
            Debug.LogError("TestPlayerItems: 무역 아이템 데이터가 로드되지 않았습니다.");
            return;
        }

        // 무작위로 numberOfItemsToGive 개수 만큼 아이템 지급
        for (int i = 0; i < numberOfItemsToGive; i++)
        {
            TradableItem randomItem = allItems[Random.Range(0, allItems.Count)];
            // 임시로 한개씩 지급
            int quantityToGive = Mathf.Max(1, 1);
            bool added = storage.AddItem(randomItem, quantityToGive);
            Debug.Log($"TestPlayerItems: {randomItem.itemName} x {quantityToGive} 지급 - 성공 여부: {added}");
        }
        InventoryUI invUI = FindObjectOfType<InventoryUI>();
        if (invUI != null)
        {
            invUI.PopulateInventory();
        }
    }
}
