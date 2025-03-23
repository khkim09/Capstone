using UnityEngine;
using UnityEngine.Serialization;

public class TradeManager : MonoBehaviour
{
    // TradeDataLoader를 통해 로드된 데이터 참조
    [SerializeField] private TradeDataLoader tradeDataLoader;

    // 플레이어의 현재 재화 (COMA)
    [SerializeField] private int playerCOMA = 1000;

    // 창고(Storage) 시스템 참조 (Storage.cs에 AddItem, HasItem, RemoveItem 메서드가 구현되어 있다고 가정)
    // TODO: 배에 중앙화하는 패턴 구현 중이라, 배에서 창고 물품 가져오게 배를 참조하면 될 것 같아.
    //[SerializeField] private Storage warehouse;
    private void Start()
    {
        // TradeDataLoader가 할당되지 않았다면 씬에서 찾아봅니다.
        if (tradeDataLoader == null) tradeDataLoader = FindObjectOfType<TradeDataLoader>();

        // // 창고 참조도 확인합니다.
        // if (warehouse == null)
        // {
        //     warehouse = FindObjectOfType<Storage>();
        // }
    }

    /// <summary>
    /// 특정 아이템과 수량에 대한 총 구매 비용을 계산합니다.
    /// 아이템의 GetCurrentPrice() 메서드를 사용하여 가격 변동을 반영합니다.
    /// </summary>
    /// <param name="item">구매할 무역 아이템</param>
    /// <param name="quantity">구매 수량</param>
    /// <returns>총 구매 비용 (COMA)</returns>
    public float GetTotalPrice(TradableItem item, int quantity)
    {
        float unitPrice = item.GetCurrentPrice();
        return unitPrice * quantity;
    }

    /// <summary>
    /// 지정한 아이템을 구매 시도합니다.
    /// 플레이어의 COMA가 충분하고, 창고에 공간(적재 한도 등)이 있다면 구매를 진행합니다.
    /// </summary>
    /// <param name="item">구매할 아이템</param>
    /// <param name="quantity">구매 수량</param>
    /// <returns>구매 성공 여부</returns>
    public bool BuyItem(TradableItem item, int quantity)
    {
        float totalCost = GetTotalPrice(item, quantity);

        if (playerCOMA >= totalCost)
        {
            // 재화 차감
            playerCOMA -= Mathf.RoundToInt(totalCost);

            // // 창고에 아이템 추가 (창고 내 최대 적층량 등의 체크는 Storage 내부에서 처리)
            // if (warehouse != null)
            // {
            //     warehouse.AddItem(item, quantity);
            // }

            Debug.Log($"Purchased {quantity} of {item.itemName} for {totalCost} COMA. Remaining coma: {playerCOMA}");
            return true;
        }
        else
        {
            Debug.Log("Not enough COMA to purchase " + item.itemName);
            return false;
        }
    }

    /// <summary>
    /// 지정한 아이템을 판매 시도합니다.
    /// 판매 가격은 현재 가격의 90%로 가정합니다.
    /// 창고에서 해당 아이템 수량이 존재하는지 체크 후 판매합니다.
    /// </summary>
    /// <param name="item">판매할 아이템</param>
    /// <param name="quantity">판매 수량</param>
    /// <returns>판매 성공 여부</returns>
    public bool SellItem(TradableItem item, int quantity)
    {
        // // 창고가 할당되어 있지 않으면 오류 처리
        // if (warehouse == null)
        // {
        //     Debug.LogError("Warehouse is not assigned.");
        //     return false;
        // }
        //
        // // 창고에 아이템이 충분한지 확인합니다.
        // if (!warehouse.HasItem(item, quantity))
        // {
        //     Debug.Log("Not enough items in storage to sell " + item.itemName);
        //     return false;
        // }
        //
        // // 판매 단가는 현재 가격의 90%로 계산 (10% 할인)
        // float sellUnitPrice = item.GetCurrentPrice() * 0.9f;
        // float totalRevenue = sellUnitPrice * quantity;
        //
        // // 창고에서 아이템 제거
        // warehouse.RemoveItem(item, quantity);
        //
        // // 판매 대금으로 재화 추가
        // playerCOMA += Mathf.RoundToInt(totalRevenue);
        // Debug.Log($"Sold {quantity} of {item.itemName} for {totalRevenue} COMA. New coma total: {playerCOMA}");
        return true;
    }

    /// <summary>
    /// 플레이어의 현재 재화(COMA) 양을 반환합니다.
    /// </summary>
    public int GetPlayerCOMA()
    {
        return playerCOMA;
    }
}
