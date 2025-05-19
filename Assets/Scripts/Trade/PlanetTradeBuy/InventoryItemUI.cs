using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// InventoryItemUI는 인벤토리 항목의 정보를 UI로 표시하고
/// 마우스 입력에 따라 하이라이트 및 선택 기능을 수행합니다.
/// </summary>
public class InventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    /// <summary>이 슬롯이 클릭 및 하이라이트 가능한지 여부입니다.</summary>
    public bool isInteractable = true;

    /// <summary>아이템 모양을 표시할 Image 컴포넌트입니다.</summary>
    [SerializeField] private Image itemShapeImage;

    /// <summary>아이템 모양별로 사용할 스프라이트 리스트입니다. shape enum 순서에 맞춰야 합니다.</summary>
    [SerializeField] private List<Sprite> itemShapeSprites;

    /// <summary>아이템 이미지를 보여주는 UI 요소입니다.</summary>
    [SerializeField] private Image itemImage;

    /// <summary>아이템 이름을 표시하는 텍스트 UI 요소입니다.</summary>
    [SerializeField] private TextMeshProUGUI itemNameText;

    /// <summary>아이템 종류를 표시하는 텍스트 UI 요소입니다.</summary>
    [SerializeField] private TextMeshProUGUI categoryText;

    /// <summary>아이템 수량을 표시하는 텍스트 UI 요소입니다.</summary>
    [SerializeField] private TextMeshProUGUI quantityText;

    /// <summary>아이템 가격을 표시하는 텍스트 UI 요소입니다.</summary>
    [SerializeField] private TextMeshProUGUI priceText;

    /// <summary>아이템 선택 시 열리는 MiddlePanel UI입니다.</summary>
    [SerializeField] private MiddlePanelUI middlePanel;

    #region Data

    /// <summary>현재 슬롯에 연결된 아이템 데이터입니다.</summary>
    private TradingItemData currentItemData;

    /// <summary>현재 슬롯의 수량입니다.</summary>
    private int currentQuantity;

    /// <summary>현재 슬롯의 구매 당시 가격입니다.</summary>
    private float currentPurchasePrice;

    /// <summary>현재 선택된 슬롯 인스턴스입니다.</summary>
    private static InventoryItemUI _currentSelectedItem = null;

    /// <summary>전역으로 저장된 선택된 아이템 이름입니다.</summary>
    public static string currentlySelectedItemName = "";

    /// <summary>현재 슬롯의 선택 상태입니다.</summary>
    private bool isSelected = false;

    #endregion

    #region Highlight Settings

    /// <summary>하이라이트 시 적용할 색상입니다.</summary>
    [SerializeField] private Color highlightColor = Color.yellow;

    private Color itemNameOriginalColor;
    private Color priceOriginalColor;
    private Color quantityOriginalColor;
    private Color categoryOriginalColor;

    #endregion

    /// <summary>
    /// 4개 텍스트 색상을 모두 지정된 색으로 설정합니다.
    /// </summary>
    private void ApplyColor(Color targetColor)
    {
        if (itemNameText != null) itemNameText.color = targetColor;
        if (priceText != null) priceText.color = targetColor;
        if (quantityText != null) quantityText.color = targetColor;
        if (categoryText != null) categoryText.color = targetColor;
    }

    /// <summary>
    /// 아이템 데이터와 수량, 구매 당시 가격을 받아 UI를 초기화합니다.
    /// </summary>
    /// <param name="itemData">아이템 데이터</param>
    /// <param name="quantity">아이템 수량</param>
    /// <param name="purchasePrice">구매 당시 가격 (kg당)</param>
    public void Setup(TradingItemData itemData, int quantity, float purchasePrice)
    {
        Debug.Log($"[InventoryItemUI] Setup called for {itemData.itemName}");

        currentItemData = itemData;
        currentQuantity = quantity;
        currentPurchasePrice = purchasePrice;

        if (itemShapeImage != null && itemShapeSprites != null && itemShapeSprites.Count > 0)
        {
            int shapeIndex = itemData.shape;
            if (shapeIndex >= 0 && shapeIndex < itemShapeSprites.Count)
            {
                itemShapeImage.sprite = itemShapeSprites[shapeIndex];
                itemShapeImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"[InventoryItemUI] shape index {shapeIndex} is out of range for {itemData.itemName}");
                itemShapeImage.enabled = false;
            }
        }

        if (itemImage != null)
        {
            itemImage.sprite = itemData.itemSprite;
            itemImage.enabled = itemData.itemSprite != null;
        }

        if (itemNameText != null)
        {
            itemNameText.text = itemData.itemName.Localize();
            itemNameOriginalColor = itemNameText.color;
        }

        if (priceText != null)
        {
            priceText.text = $"{purchasePrice:F2} → {itemData.costBase:F2}";
            priceOriginalColor = priceText.color;
        }

        if (quantityText != null)
        {
            quantityText.text = quantity.ToString();
            quantityOriginalColor = quantityText.color;
        }

        if (categoryText != null)
        {
            categoryText.text = itemData.type.ToString();
            categoryOriginalColor = categoryText.color;
        }

        isSelected = false;

        if (currentlySelectedItemName == itemData.itemName && quantity > 0)
        {
            SetSelected(true);
            _currentSelectedItem = this;
        }
    }

    /// <summary>이 슬롯에 연결된 아이템 이름을 반환합니다.</summary>
    public string GetStoredItemName()
    {
        return currentItemData?.itemName ?? string.Empty;
    }

    /// <summary>마우스 오버 시 강조 색상 적용</summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable || isSelected) return;
        ApplyColor(highlightColor);
    }

    /// <summary>마우스 벗어날 때 원래 색상 복구</summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable || isSelected) return;
        ApplyColor(itemNameOriginalColor);
    }

    /// <summary>슬롯 클릭 시 선택 토글 및 MiddlePanel/하이라이터 갱신</summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable) return;

        TradeUIManager.Instance?.OnSellItemSelected();

        if (isSelected)
        {
            if (currentQuantity > 0) return;

            SetSelected(false);
            currentlySelectedItemName = "";
            _currentSelectedItem = null;
            Object.FindFirstObjectByType<StorageHighlightManager>()?.ClearHighlights();
        }
        else
        {
            if (_currentSelectedItem != null && _currentSelectedItem != this)
            {
                _currentSelectedItem.SetSelected(false);
                Object.FindFirstObjectByType<StorageHighlightManager>()?.ClearHighlights();
            }

            SetSelected(true);
            currentlySelectedItemName = currentItemData.itemName;
            _currentSelectedItem = this;

            if (middlePanel != null)
            {
                middlePanel.gameObject.SetActive(true);
                middlePanel.UpdatePlayerComa();
                middlePanel.SetSelectedItem(currentItemData);
            }
        }

        var highlighter = Object.FindFirstObjectByType<StorageHighlightManager>();
        highlighter?.HighlightItem(currentItemData.itemName);
    }

    /// <summary>현재 슬롯의 선택 상태를 설정합니다.</summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        ApplyColor(selected ? highlightColor : itemNameOriginalColor);
    }

    /// <summary>현재 선택된 InventoryItemUI 슬롯을 반환합니다.</summary>
    public static InventoryItemUI GetCurrentSelectedItem()
    {
        return _currentSelectedItem;
    }

    /// <summary>현재 슬롯의 아이템 데이터를 반환합니다.</summary>
    public TradingItemData GetItemData()
    {
        return currentItemData;
    }

    /// <summary>현재 슬롯의 수량을 반환합니다.</summary>
    public int GetQuantity()
    {
        return currentQuantity;
    }
}
