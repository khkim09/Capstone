using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// InventoryItemSellUI는 인벤토리 항목의 정보를 UI로 표시하는 컴포넌트입니다.
/// 아이템의 이름, 가격, 수량, 그리고 물품의 종류(분류)를 모두 표시하며,
/// 마우스 오버 시 네 가지 텍스트의 색상을 강조 색상으로 변경하고,
/// 마우스가 떠나면 원래 색상으로 복구합니다.
/// 또한, 슬롯을 클릭하면 MiddlePanelUI에 해당 아이템의 상세 정보를 전달하며,
/// 한 번 선택된 항목은 다른 항목을 클릭할 때까지 강조(테두리)를 유지합니다.
/// (단, 아이템 수량이 0이 되어 실제 창고에서 삭제되면 선택은 해제됩니다.)
/// </summary>
public class InventoryItemSellUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region UI References

    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text categoryText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text quantityText;

    /// <summary>
    /// 선택 상태를 표시하는 테두리 강조 오브젝트입니다.
    /// Inspector에서 프리팹 내부의 테두리 역할 오브젝트(예: Image 컴포넌트가 있는 Panel)를 연결하고,
    /// 기본 상태에서 비활성화(Off)되어 있어야 합니다.
    /// </summary>
    [SerializeField] private GameObject borderHighlight;

    #endregion

    #region Data

    private StoredItem currentStoredItem;

    // static 변수로 현재 선택된 슬롯을 전역 관리합니다.
    private static InventoryItemSellUI currentSelectedItem = null;
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
    /// StoredItem 데이터를 받아 UI를 초기화합니다.
    /// 인벤토리 갱신 시, 만약 현재 선택된 아이템 이름과 일치하고 수량이 0보다 크면 선택 상태를 복원합니다.
    /// </summary>
    /// <param name="storedItem">UI 설정에 사용되는 StoredItem 데이터</param>
    public void Setup(StoredItem storedItem)
    {
        Debug.Log($"[InventoryItemSellUI] Setup called for {storedItem.item.itemName}");
        currentStoredItem = storedItem;

        if (itemNameText != null)
        {
            itemNameText.text = storedItem.item.itemName;
            itemNameOriginalColor = itemNameText.color;
        }
        if (priceText != null)
        {
            // 현재 가격은 변동폭이 반영된 값으로 표시합니다.
            priceText.text = storedItem.item.GetCurrentPrice().ToString("F2");
            priceOriginalColor = priceText.color;
        }
        if (quantityText != null)
        {
            quantityText.text = storedItem.quantity.ToString();
            quantityOriginalColor = quantityText.color;
        }
        if (categoryText != null)
        {
            categoryText.text = storedItem.item.category.ToString();
            categoryOriginalColor = categoryText.color;
        }

        // 초기에는 선택 효과 없이 테두리 강조는 비활성화
        if (borderHighlight != null)
            borderHighlight.SetActive(false);
        isSelected = false;

        // 만약 이 아이템이 이전에 선택된 것과 동일하고, 수량이 남아 있다면 선택 상태 복원
        if (currentlySelectedItemName == storedItem.item.itemName && storedItem.quantity > 0)
        {
            SetSelected(true);
            currentSelectedItem = this;
        }
    }

    /// <summary>
    /// 마우스 오버 시 호출되어 선택되지 않은 경우에만 텍스트 색상을 강조 색상으로 변경합니다.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
        {
            if (itemNameText != null)
                itemNameText.color = highlightColor;
            if (priceText != null)
                priceText.color = highlightColor;
            if (quantityText != null)
                quantityText.color = highlightColor;
            if (categoryText != null)
                categoryText.color = highlightColor;
        }
    }

    /// <summary>
    /// 마우스가 슬롯에서 벗어나면, 선택되지 않은 경우 텍스트 색상을 원래 색상으로 복구합니다.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
        {
            if (itemNameText != null)
                itemNameText.color = itemNameOriginalColor;
            if (priceText != null)
                priceText.color = priceOriginalColor;
            if (quantityText != null)
                quantityText.color = quantityOriginalColor;
            if (categoryText != null)
                categoryText.color = categoryOriginalColor;
        }
    }

    /// <summary>
    /// 슬롯 클릭 시 호출되며, 현재 슬롯을 선택하거나 해제합니다.
    /// 다른 슬롯이 선택되어 있다면 먼저 해제하고, 현재 슬롯에 대해 MiddlePanelUI에 상세 정보를 전달합니다.
    /// 만약 이미 선택된 슬롯이 재클릭되었는데, 수량이 0보다 크면 선택 상태를 유지합니다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
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
                currentSelectedItem = null;
            }
        }
        else
        {
            // 다른 슬롯이 선택되어 있다면 해제
            if (currentSelectedItem != null && currentSelectedItem != this)
            {
                currentSelectedItem.SetSelected(false);
            }
            SetSelected(true);
            currentlySelectedItemName = currentStoredItem.item.itemName;
            currentSelectedItem = this;

            // 선택된 경우 MiddlePanelUI에 상세 정보 전달
            MiddlePanelUI middlePanel = FindObjectOfType<MiddlePanelUI>();
            if (middlePanel != null && currentStoredItem != null)
            {
                middlePanel.SetSelectedItem(currentStoredItem.item);
            }
        }
    }

    /// <summary>
    /// 선택 상태에 따라 테두리 강조 오브젝트와 텍스트 색상을 업데이트합니다.
    /// </summary>
    /// <param name="selected">선택되었으면 true, 아니면 false</param>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (borderHighlight != null)
            borderHighlight.SetActive(selected);

        if (selected)
        {
            if (itemNameText != null)
                itemNameText.color = highlightColor;
            if (priceText != null)
                priceText.color = highlightColor;
            if (quantityText != null)
                quantityText.color = highlightColor;
            if (categoryText != null)
                categoryText.color = highlightColor;
        }
        else
        {
            if (itemNameText != null)
                itemNameText.color = itemNameOriginalColor;
            if (priceText != null)
                priceText.color = priceOriginalColor;
            if (quantityText != null)
                quantityText.color = quantityOriginalColor;
            if (categoryText != null)
                categoryText.color = categoryOriginalColor;
        }
    }
}
