using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// TradeItemUI는 무역 아이템의 정보를 UI에 표시하고,
/// 구매 및 판매 버튼 이벤트를 처리하여 거래 동작을 수행하는 컴포넌트입니다.
/// </summary>
public class TradeItemUI : MonoBehaviour
{
    #region UI References

    /// <summary>
    /// 아이템 이름을 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [Header("UI References")]
    [SerializeField] private TMP_Text itemNameText;

    /// <summary>
    /// 아이템의 현재 가격을 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TMP_Text priceText;

    #endregion

    #region Data References

    /// <summary>
    /// 현재 UI에 표시될 아이템 데이터입니다.
    /// </summary>
    private TradingItemData itemData;

    /// <summary>
    /// 거래 관련 기능을 수행하는 TradeManager의 참조입니다.
    /// </summary>
    private TradeManager tradeManager;

    /// <summary>
    /// TradeUI의 참조로, 재화 업데이트 등의 작업에 사용됩니다.
    /// </summary>
    private TradeUI tradeUI;

    #endregion

    /// <summary>
    /// TradeUI에서 호출하여 이 UI 요소를 초기화합니다.
    /// 아이템 데이터와 거래 관리, UI 업데이트를 위한 참조를 설정하고,
    /// 각 UI 요소의 초기 텍스트를 설정합니다.
    /// </summary>
    /// <param name="data">표시할 아이템 데이터</param>
    /// <param name="manager">거래 처리를 위한 TradeManager 참조</param>
    /// <param name="ui">UI 업데이트를 위한 TradeUI 참조</param>
    public void Setup(TradingItemData data, TradeManager manager, TradeUI ui)
    {
        itemData = data;
        tradeManager = manager;
        tradeUI = ui;

        if (itemNameText != null)
            itemNameText.text = itemData.itemName;

        UpdatePriceText();
    }

    /// <summary>
    /// 아이템의 현재 가격을 가져와 UI에 표시합니다.
    /// 가격은 TradeManager의 가격 계산 로직을 통해 얻습니다.
    /// </summary>
    private void UpdatePriceText()
    {
        if (priceText != null && itemData != null)
        {
            float currentPrice = tradeManager.CalculatePrice(itemData);
            priceText.text = currentPrice.ToString("F2");
        }
    }
}
