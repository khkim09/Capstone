using UnityEngine;
using UnityEngine.Serialization;

public class TradeManager : MonoBehaviour
{
    // TradeDataLoader를 통해 로드된 데이터 참조
    [SerializeField] private TradeDataLoader tradeDataLoader;

    // 플레이어의 현재 재화 (COMA)
    [SerializeField] private int playerCOMA = 1000;

    private void Start()
    {
        if (tradeDataLoader == null)
            tradeDataLoader = FindObjectOfType<TradeDataLoader>();
    }

    public float GetTotalPrice(TradableItem item, int quantity)
    {
        float unitPrice = item.GetCurrentPrice();
        return unitPrice * quantity;
    }

    public bool BuyItem(TradableItem item, int quantity)
    {
        float totalCost = GetTotalPrice(item, quantity);

        if (playerCOMA >= totalCost)
        {
            playerCOMA -= Mathf.RoundToInt(totalCost);
            // Storage에 아이템 추가 등 추가 로직을 여기에 넣을 수 있습니다.
            Debug.Log($"Purchased {quantity} of {item.itemName} for {totalCost} COMA. Remaining COMA: {playerCOMA}");
            return true;
        }
        else
        {
            Debug.Log("Not enough COMA to purchase " + item.itemName);
            return false;
        }
    }

    public bool SellItem(TradableItem item, int quantity)
    {
        // 판매 로직 구현 (예: 90% 가격 등)
        return true;
    }

    public int GetPlayerCOMA()
    {
        return playerCOMA;
    }
}
