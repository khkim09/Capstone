using UnityEngine;
using System.Collections.Generic;

public class TestPlayerItems : MonoBehaviour
{
    [SerializeField] private TradeDataLoader tradeDataLoader;
    [SerializeField] private Storage warehouse; // 플레이어 창고 컴포넌트
    [SerializeField] private int numberOfItemsToGive = 3;

    private void Start()
    {
        // tradeDataLoader와 warehouse가 할당되어 있는지 확인
        if (tradeDataLoader == null)
            tradeDataLoader = FindObjectOfType<TradeDataLoader>();

        if (warehouse == null)
            warehouse = FindObjectOfType<Storage>();

        if (tradeDataLoader == null || warehouse == null)
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
            // 임시로 최대 적층량의 절반 정도 수량을 지급 (테스트 용도)
            int quantityToGive = Mathf.Max(1, 1);
            bool added = warehouse.AddItem(randomItem, quantityToGive);
            Debug.Log($"TestPlayerItems: {randomItem.itemName} x {quantityToGive} 지급 - 성공 여부: {added}");
        }
        InventoryUI invUI = FindObjectOfType<InventoryUI>();
        if (invUI != null)
        {
            invUI.PopulateInventory();
        }
    }
}
