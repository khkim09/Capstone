using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// TradeManager는 무역 거래를 관리하는 클래스입니다.
/// 플레이어의 재화(COMA)를 관리하며, 아이템의 구매 및 판매 기능을 제공합니다.
/// </summary>
public class TradeManager : MonoBehaviour
{
    /// <summary>
    /// TradeDataLoader를 통해 로드된 데이터의 참조입니다.
    /// </summary>
    [SerializeField] private TradeDataLoader tradeDataLoader;

    /// <summary>
    /// 현재 행성의 판매 데이터를 저장하는 변수입니다.
    /// 이 변수는 플레이어가 도착한 행성의 PlanetTradeData 에셋을 참조합니다.
    /// </summary>
    [SerializeField]
    private PlanetTradeData currentPlanetTradeData;


    /// <summary>
    /// 플레이어의 현재 재화(COMA)입니다. 초기값은 1000입니다.
    /// </summary>
    [SerializeField] private float playerCOMA = 1000.00f;

    /// <summary>
    /// 현재 행성 판매 데이터를 외부에서 읽을 수 있도록 하는 프로퍼티입니다.
    /// </summary>
    public PlanetTradeData CurrentPlanetTradeData
    {
        get { return currentPlanetTradeData; }
    }


    /// <summary>
    /// MonoBehaviour의 Start 메서드입니다.
    /// TradeDataLoader가 할당되지 않은 경우 씬에서 찾아 할당합니다.
    /// </summary>
    private void Start()
    {
        if (tradeDataLoader == null)
            tradeDataLoader = FindObjectOfType<TradeDataLoader>();
    }

    /// <summary>
    /// 지정된 아이템과 수량에 대한 총 가격을 계산합니다.
    /// </summary>
    /// <param name="item">거래할 TradableItem 객체</param>
    /// <param name="quantity">아이템 수량</param>
    /// <returns>총 가격 (COMA 단위)</returns>
    public float GetTotalPrice(TradableItem item, int quantity)
    {
        float unitPrice = item.GetCurrentPrice();
        return unitPrice * quantity;
    }

    /// <summary>
    /// 지정된 아이템을 구매하는 메서드입니다.
    /// 구매가 가능하면 아이템을 창고에 추가하고, 플레이어의 COMA를 차감합니다.
    /// </summary>
    /// <param name="item">구매할 TradableItem 객체</param>
    /// <param name="quantity">구매할 수량</param>
    /// <returns>구매 성공 시 true, 실패 시 false</returns>
    public bool BuyItem(TradableItem item, int quantity)
    {
        Storage warehouse = FindObjectOfType<Storage>();
        if (warehouse == null)
        {
            Debug.LogError("No Storage found.");
            return false;
        }

        // 구매 가능한지 미리 체크 (방법 1)
        if (!warehouse.CanAddItem(item, quantity))
        {
            Debug.Log("Cannot purchase " + item.itemName + " because it would exceed the maximum stack amount.");
            return false;
        }

        float totalCost = GetTotalPrice(item, quantity);
        if (playerCOMA >= totalCost)
        {
            playerCOMA -= Mathf.RoundToInt(totalCost);
            bool added = warehouse.AddItem(item, quantity);
            if (!added)
            {
                // 만약 추가가 실패하면, 돈 환불 처리 (옵션)
                playerCOMA += Mathf.RoundToInt(totalCost);
                Debug.LogWarning("Failed to add item to storage, money refunded.");
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

    /// <summary>
    /// 지정된 아이템을 판매하는 메서드입니다.
    /// 창고에 충분한 아이템이 있는 경우 판매하고, 판매 금액만큼 플레이어의 COMA를 증가시킵니다.
    /// </summary>
    /// <param name="item">판매할 TradableItem 객체</param>
    /// <param name="quantity">판매할 수량</param>
    /// <returns>판매 성공 시 true, 실패 시 false</returns>
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
        float sellUnitPrice = item.GetCurrentPrice();
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

    /// <summary>
    /// 현재 플레이어의 COMA(재화)를 반환합니다.
    /// </summary>
    /// <returns>플레이어의 현재 COMA 값</returns>
    public float GetPlayerCOMA()
    {
        return playerCOMA;
    }
    /// <summary>
    /// 현재 행성 판매 데이터를 설정합니다. 이 데이터는 무역 거래 시 사용됩니다.
    /// </summary>
    /// <param name="data">선택한 행성의 판매 물품 데이터</param>
    public void SetCurrentPlanetTradeData(PlanetTradeData data)
    {
        // TradeManager 내부에서 현재 행성 데이터를 저장하고, 필요에 따라 무역 로직에 반영합니다.
        currentPlanetTradeData = data;
        Debug.Log("TradeManager: 현재 행성 데이터가 설정되었습니다: " + data.planetCode);
    }

}
