using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class BuyCheckPanel : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private TextMeshProUGUI categoryText;

    [SerializeField] private TextMeshProUGUI nameText;

    [SerializeField] private Image shapeImage;
    [SerializeField] private List<Sprite> itemShapeSprites;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;

    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button leftLeftButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button rightRightButton;

    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private TradeUIController tradeUIController;

    private TradingItemData currentItem;

    private int currentAmount;
    private int minAmount = 1;
    private int maxAmount = 0;

    public void Initialize(TradingItemData item)
    {
        currentItem = item;

        categoryText.text = currentItem.type.Localize();
        shapeImage.sprite = itemShapeSprites[currentItem.shape];
        nameText.text = currentItem.itemName.Localize();
        descriptionText.text = currentItem.description.Localize();
        int currentPrice = GameManager.Instance.WhereIAm().GetItemPrice(currentItem);
        currentItem.boughtCost = currentPrice;
        priceText.text =
            $"{"ui.trade.currentprice".Localize()} : {currentPrice.ToString()}";

        maxAmount = currentItem.capacity;
        minAmount = 1;

        currentAmount = (int)((maxAmount + minAmount) / 2);
        UpdateAmount();
    }

    public void OnLeftLeftButtonClicked()
    {
        currentAmount -= 10;
        currentAmount = Mathf.Clamp(currentAmount, minAmount, maxAmount);
        UpdateAmount();
    }

    public void OnLeftButtonClicked()
    {
        currentAmount--;
        currentAmount = Mathf.Clamp(currentAmount, minAmount, maxAmount);
        UpdateAmount();
    }

    public void OnRightButtonClicked()
    {
        currentAmount++;
        currentAmount = Mathf.Clamp(currentAmount, minAmount, maxAmount);
        UpdateAmount();
    }

    public void OnRightRightButtonClicked()
    {
        currentAmount += 10;
        currentAmount = Mathf.Clamp(currentAmount, minAmount, maxAmount);
        UpdateAmount();
    }

    public void UpdateAmount()
    {
        amountText.text = currentAmount.ToString();
        currentItem.amount = currentAmount;
    }

    public void OnBuyButtonClicked()
    {
        tradeUIController.OnBuyCheckBuyButtonClicked(currentItem);
    }
}
