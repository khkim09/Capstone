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

            // Storage에 아이템을 추가하는 로직 (warehouse가 올바른 Storage 인스턴스를 참조하고 있어야 합니다)
            Storage warehouse = FindObjectOfType<Storage>();
            if (warehouse != null)
            {
                bool added = warehouse.AddItem(item, quantity);
                if (!added)
                {
                    Debug.LogWarning("Failed to add item to storage.");
                    return false;
                }
            }
            else
            {
                Debug.LogError("No Storage found.");
                return false;
            }

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
        // Storage를 찾습니다.
        Storage warehouse = FindObjectOfType<Storage>();
        if (warehouse == null)
        {
            Debug.LogError("No Storage found.");
            return false;
        }

        // 창고에 아이템이 충분히 있는지 확인합니다.
        if (!warehouse.HasItem(item, quantity))
        {
            Debug.Log("Not enough items in storage to sell " + item.itemName);
            return false;
        }

        // 판매 단가를 현재 가격의 90%로 가정합니다.
        float sellUnitPrice = item.GetCurrentPrice() * 0.9f;
        float totalRevenue = sellUnitPrice * quantity;

        // 창고에서 아이템 제거
        bool removed = warehouse.RemoveItem(item, quantity);
        if (!removed)
        {
            Debug.LogWarning("Failed to remove item from storage.");
            return false;
        }

        // 플레이어의 재화를 증가시킵니다.
        // 예를 들어, playerCOMA에 판매 금액을 더합니다.
        playerCOMA += Mathf.RoundToInt(totalRevenue);
        Debug.Log($"Sold {quantity} of {item.itemName} for {totalRevenue} COMA. New COMA total: {playerCOMA}");
        return true;
    }

    public int GetPlayerCOMA()
    {
        return playerCOMA;
    }
}
