using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text itemNameText;       // 아이템 이름 표시
    [SerializeField] private TMP_Text categoryText;       // 분류 표시
    [SerializeField] private TMP_Text priceText;          // 현재 가격 표시
    [SerializeField] private TMP_Text maxStackText;       // 최대 적층량 표시
    [SerializeField] private TMP_Text descriptionText;    // 아이템 설명 표시
    [SerializeField] private TMP_InputField quantityInputField; // 구매/판매 수량 입력
    [SerializeField] private Button buyButton;        // 구매 버튼
    [SerializeField] private Button sellButton;       // 판매 버튼

    // 데이터 참조
    private TradableItem tradableItem;
    private TradeManager tradeManager;
    private TradeUI tradeUI;

    /// <summary>
    /// TradeUI에서 호출하여 이 UI 요소를 초기화합니다.
    /// </summary>
    /// <param name="item">TradableItem 데이터</param>
    /// <param name="manager">TradeManager 참조</param>
    /// <param name="ui">TradeUI 참조 (재화 업데이트용)</param>
    public void Setup(TradableItem item, TradeManager manager, TradeUI ui)
    {
        tradableItem = item;
        tradeManager = manager;
        tradeUI = ui;

        // UI 텍스트 초기화
        if (itemNameText != null)
            itemNameText.text = tradableItem.itemName;
        //  if (categoryText != null)
        //   categoryText.text = tradableItem.category;
        if (maxStackText != null)
            maxStackText.text = "Max Stack: " + tradableItem.maxStackAmount.ToString();
        if (descriptionText != null)
            descriptionText.text = tradableItem.description;

        UpdatePriceText();

        // 버튼 클릭 이벤트 등록
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        if (sellButton != null)
            sellButton.onClick.AddListener(OnSellButtonClicked);
    }

    /// <summary>
    /// 아이템의 현재 가격을 가져와 UI에 표시합니다.
    /// </summary>
    private void UpdatePriceText()
    {
        if (priceText != null && tradableItem != null)
        {
            float currentPrice = tradableItem.GetCurrentPrice();
            priceText.text = "Price: " + currentPrice.ToString("F2") + " coma/kg";
        }
    }

    /// <summary>
    /// 구매 버튼 클릭 이벤트 처리
    /// </summary>
    private void OnBuyButtonClicked()
    {
        int quantity = 1;
        if (quantityInputField != null) int.TryParse(quantityInputField.text, out quantity);
        if (tradeManager.BuyItem(tradableItem, quantity))
            // 구매 성공 시 재화 업데이트
            tradeUI.UpdatePlayerCOMA();
    }

    /// <summary>
    /// 판매 버튼 클릭 이벤트 처리
    /// </summary>
    private void OnSellButtonClicked()
    {
        int quantity = 1;
        if (quantityInputField != null) int.TryParse(quantityInputField.text, out quantity);
        if (tradeManager.SellItem(tradableItem, quantity))
            // 판매 성공 시 재화 업데이트
            tradeUI.UpdatePlayerCOMA();
    }
}
