using UnityEngine;

/// <summary>
/// TradeManager는 무역 거래를 관리하는 클래스입니다.
/// 플레이어의 재화(COMA)를 관리하며, 아이템의 구매 및 판매 기능을 제공합니다.
/// </summary>
public class TradeManager : MonoBehaviour
{
    /// <summary>
    /// 현재 행성의 판매 데이터를 저장하는 변수입니다.
    /// 이 변수는 플레이어가 도착한 행성의 PlanetTradeData 에셋을 참조합니다.
    /// </summary>
    [SerializeField] private PlanetTradeData currentPlanetTradeData;

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
    /// 지정된 아이템과 수량에 대한 총 가격을 계산합니다.
    /// </summary>
    /// <param name="itemData">거래할 TradingItemData</param>
    /// <param name="quantity">수량</param>
    /// <returns>총 가격(COMAs)</returns>
    public float GetTotalPrice(TradingItemData itemData, int quantity)
    {
        float unitPrice = CalculatePrice(itemData);
        return unitPrice * quantity;
    }

    /// <summary>
    /// 아이템의 현재 단가를 계산합니다. (기본은 basePrice 그대로 반환)
    /// 추후 이벤트 또는 행성 특성에 따라 가중치를 부여할 수 있습니다.
    /// </summary>
    /// <param name="itemData">가격 계산할 아이템</param>
    /// <returns>계산된 단가</returns>
    public float CalculatePrice(TradingItemData itemData)
    {
        return itemData.costBase;
    }

    /// <summary>
    /// 지정한 아이템을 구매 시도합니다.
    /// 성공 시 COMA를 차감하고 아이템을 창고에 추가합니다.
    /// </summary>
    /// <param name="itemData">구매할 아이템 데이터</param>
    /// <param name="quantity">구매 수량</param>
    /// <returns>구매 성공 여부</returns>
    public bool BuyItem(TradingItemData itemData, int quantity)
    {
        Storage storage =  Object.FindFirstObjectByType<Storage>();
        if (storage == null)
        {
            Debug.LogError("TradeManager: Storage 컴포넌트를 찾을 수 없습니다.");
            return false;
        }

        if (!storage.CanAddItem(itemData, quantity))
        {
            Debug.Log("TradeManager: 적재 한도를 초과하여 구매 불가.");
            return false;
        }

        float totalCost = GetTotalPrice(itemData, quantity);
        if (playerCOMA >= totalCost)
        {
            playerCOMA -= Mathf.RoundToInt(totalCost);
            bool added = storage.AddItem(itemData, quantity);
            if (!added)
            {
                playerCOMA += Mathf.RoundToInt(totalCost); // 환불
                Debug.LogWarning("TradeManager: 아이템 추가 실패로 COMA 환불됨.");
                return false;
            }

            Debug.Log($"TradeManager: {itemData.itemName} x{quantity} 구매 완료. 잔여 COMA: {playerCOMA}");
            return true;
        }
        else
        {
            Debug.Log("TradeManager: 잔여 COMA 부족으로 구매 실패.");
            return false;
        }
    }

    /// <summary>
    /// 지정한 아이템을 판매 시도합니다.
    /// 성공 시 아이템을 제거하고 COMA를 증가시킵니다.
    /// </summary>
    /// <param name="itemData">판매할 아이템 데이터</param>
    /// <param name="quantity">판매 수량</param>
    /// <returns>판매 성공 여부</returns>
    public bool SellItem(TradingItemData itemData, int quantity)
    {
        Storage storage =  Object.FindFirstObjectByType<Storage>();
        if (storage == null)
        {
            Debug.LogError("TradeManager: Storage 컴포넌트를 찾을 수 없습니다.");
            return false;
        }

        if (!storage.HasItem(itemData, quantity))
        {
            Debug.Log("TradeManager: 보유 수량 부족으로 판매 실패.");
            return false;
        }

        float unitPrice = CalculatePrice(itemData);
        float totalRevenue = unitPrice * quantity;

        bool removed = storage.RemoveItem(itemData, quantity);
        if (!removed)
        {
            Debug.LogWarning("TradeManager: 아이템 제거 실패.");
            return false;
        }

        playerCOMA += Mathf.RoundToInt(totalRevenue);
        Debug.Log($"TradeManager: {itemData.itemName} x{quantity} 판매 완료. 획득 COMA: {totalRevenue}");
        return true;
    }

    /// <summary>
    /// 현재 행성의 무역 데이터를 설정합니다.
    /// </summary>
    /// <param name="data">행성별 무역 데이터</param>
    public void SetCurrentPlanetTradeData(PlanetTradeData data)
    {
        currentPlanetTradeData = data;
        Debug.Log("TradeManager: 행성 무역 데이터 설정 완료: " + data.planetCode);
    }
}
