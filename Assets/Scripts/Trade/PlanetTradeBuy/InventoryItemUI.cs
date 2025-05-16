using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;


/// <summary>
/// InventoryItemUI는 인벤토리 항목의 정보를 UI로 표시하는 컴포넌트입니다.
/// 아이템의 이름, 가격, 수량 및 (필요한 경우) 아이콘을 표시합니다.
/// </summary>
public class InventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    /// <summary>이 슬롯이 클릭 및 하이라이트 가능한지 여부입니다.</summary>
    public bool isInteractable = true;

    /// <summary>
    /// 아이템 이미지를 보여주는 UI 요소입니다.
    /// </summary>
    [SerializeField] private UnityEngine.UI.Image itemImage;

    /// <summary>
    /// 아이템 이름을 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TextMeshProUGUI itemNameText;

    /// <summary>
    /// 아이템 종류를 표시하는 택스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TextMeshProUGUI categoryText;

    /// <summary>
    /// 아이템 가격을 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TextMeshProUGUI priceText;

    /// <summary>
    /// 아이템 수량을 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TextMeshProUGUI quantityText;
    // (아이콘이 있다면) [SerializeField] private Image itemIcon;

    /// <summary>
    /// 현재 이 슬롯에 연결된 StoredItem 정보를 저장합니다.
    /// </summary>
    #region Data

    private StoredItem currentStoredItem;

    // static 변수로 현재 선택된 슬롯을 전역 관리합니다.
    private static InventoryItemUI _currentSelectedItem = null;
    public static string currentlySelectedItemName = "";

    // 이 슬롯의 선택 상태
    private bool isSelected = false;

    #endregion

    #region Highlight Settings

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
        if (itemNameText != null)
            itemNameText.color = targetColor;
        if (priceText != null)
            priceText.color = targetColor;
        if (quantityText != null)
            quantityText.color = targetColor;
        if (categoryText != null)
            categoryText.color = targetColor;
    }

    /// <summary>
    /// StoredItem 데이터를 받아 UI를 초기화합니다.
    /// 인벤토리 갱신 시, 만약 현재 선택된 아이템 이름과 일치하고 수량이 0보다 크면 선택 상태를 복원합니다.
    /// </summary>
    /// <param name="storedItem">UI 설정에 사용되는 StoredItem 데이터</param>
    public void Setup(StoredItem storedItem)
    {
        Debug.Log($"[InventoryItemUI] Setup called for {storedItem.itemData.itemName}");
        currentStoredItem = storedItem;

        if (itemImage != null)
        {
            if (storedItem.itemData.itemSprite != null)
            {
                itemImage.sprite = storedItem.itemData.itemSprite;
                itemImage.enabled = true;
            }
            else
            {
                itemImage.enabled = false;
            }
        }

        if (itemNameText != null)
        {
            itemNameText.text = storedItem.itemData.itemName.Localize();
            itemNameOriginalColor = itemNameText.color;
        }

        if (categoryText != null)
        {
            categoryText.text = storedItem.itemData.type.ToString();
            categoryOriginalColor = categoryText.color;
        }


        if (priceText != null)
        {
            // 현재 가격은 변동폭이 반영된 값으로 표시합니다.
            // priceText.text = storedItem.itemData.GetCurrentPrice().ToString("F2");
            priceText.text = storedItem.itemData.costBase.ToString("F2");
            priceOriginalColor = priceText.color;
        }

        if (quantityText != null)
        {
            quantityText.text = storedItem.quantity.ToString();
            quantityOriginalColor = quantityText.color;
        }
        isSelected = false;

        // 만약 이 아이템이 이전에 선택된 것과 동일하고, 수량이 남아 있다면 선택 상태 복원
        if (currentlySelectedItemName == storedItem.itemData.itemName && storedItem.quantity > 0)
        {
            SetSelected(true);
            _currentSelectedItem = this;
        }
    }

    /// <summary>
    /// 이 슬롯에 연결된 StoredItem의 아이템 이름을 반환합니다.
    /// TradeUIManager에서 "GetStoredItemName" 으로 호출할 수 있게 해 줍니다.
    /// </summary>
    public string GetStoredItemName()
    {
        // null 체크 후 이름 반환
        if (currentStoredItem != null && currentStoredItem.itemData != null)
            return currentStoredItem.itemData.itemName;
        return string.Empty;
    }

    /// <summary>
    /// 마우스 오버 시 호출되어 선택되지 않은 경우에만 텍스트 색상을 강조 색상으로 변경합니다.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable || isSelected) return;
        ApplyColor(highlightColor);
    }

    /// <summary>
    /// 마우스가 슬롯에서 벗어나면, 선택되지 않은 경우 텍스트 색상을 원래 색상으로 복구합니다.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable || isSelected) return;
        ApplyColor(itemNameOriginalColor);
    }

    /// <summary>
    /// 슬롯 클릭 시 호출되며, 현재 슬롯을 선택하거나 해제합니다.
    /// 다른 슬롯이 선택되어 있다면 먼저 해제하고, 현재 슬롯에 대해 MiddlePanelUI에 상세 정보를 전달합니다.
    /// 만약 이미 선택된 슬롯이 재클릭되었는데, 수량이 0보다 크면 선택 상태를 유지합니다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable) return;

        // TradeUIManager에 판매 상세 패널 열라고 알림
        if (TradeUIManager.Instance != null)
        {
            TradeUIManager.Instance.OnSellItemSelected();
        }

        // 이미 선택되어 있고, 수량이 남아 있다면 아무런 처리를 하지 않고 선택 상태 유지
        if (isSelected)
        {
            if (currentStoredItem.quantity > 0)
            {
                // 선택 상태 유지 (즉, 아무런 토글 동작을 하지 않음)
                return;
            }
            else
            {
                // 수량이 0이면 선택 해제
                SetSelected(false);
                currentlySelectedItemName = "";
                _currentSelectedItem = null;
                Object.FindFirstObjectByType<StorageHighlightManager>()?.ClearHighlights();
            }
        }
        else
        {
            // 다른 슬롯이 선택되어 있다면 해제
            if (_currentSelectedItem != null && _currentSelectedItem != this)
            {
                _currentSelectedItem.SetSelected(false);
                Object.FindFirstObjectByType<StorageHighlightManager>()?.ClearHighlights();
            }

            SetSelected(true);
            currentlySelectedItemName = currentStoredItem.itemData.itemName;
            _currentSelectedItem = this;

            // 선택된 경우 MiddlePanelUI에 상세 정보 전달
            MiddlePanelUI middlePanel = Object.FindFirstObjectByType<MiddlePanelUI>();
            if (middlePanel != null && currentStoredItem != null)
            {
                middlePanel.UpdatePlayerComa();
                middlePanel.SetSelectedItem(currentStoredItem.itemData);
            }
        }
        // 선택된 아이템 이름을 StorageHighlightManager에 전달해서
        // 창고 그리드 상의 TradingItem들을 강조(on) 합니다.
        var highlighter = Object.FindFirstObjectByType<StorageHighlightManager>();
        if (highlighter != null && currentStoredItem != null)
        {
            highlighter.HighlightItem(currentStoredItem.itemData.itemName);
        }
    }

    /// <summary>
    /// 선택 상태에 따라 테두리 강조 오브젝트와 텍스트 색상을 업데이트합니다.
    /// </summary>
    /// <param name="selected">선택되었으면 true, 아니면 false</param>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        ApplyColor(selected ? highlightColor : itemNameOriginalColor);
    }

    /// <summary>
    /// 현재 선택된 아이템을 반환하는 함수입니다.
    /// </summary>
    /// <returns>현재 선택된 아이템을 반환합니다.</returns>
    public static InventoryItemUI GetCurrentSelectedItem()
    {
        return _currentSelectedItem;
    }
    /// <summary>
    /// 현재 슬롯에 연결된 StoredItem을 반환합니다.
    /// </summary>
    public StoredItem GetStoredItem()
    {
        return currentStoredItem;
    }

}
