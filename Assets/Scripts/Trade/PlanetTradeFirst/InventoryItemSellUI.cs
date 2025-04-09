using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// InventoryItemSellUI는 인벤토리 항목의 정보를 UI로 표시하는 컴포넌트입니다.
/// 아이템의 이름, 가격, 수량 및 물품의 종류(분류)를 모두 표시하며,
/// 마우스 오버 시 이 네 가지 텍스트 컴포넌트의 색상을 강조 색상으로 변경하고,
/// 마우스가 떠나면 원래 색상으로 복구합니다.
/// </summary>
public class InventoryItemSellUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region UI References

    /// <summary>
    /// 아이템 이름을 표시하는 TextMeshProUGUI 컴포넌트입니다.
    /// </summary>
    [SerializeField] private TMP_Text itemNameText;

    /// <summary>
    /// 아이템 카테고리(분류)를 표시하는 TextMeshProUGUI 컴포넌트입니다.
    /// </summary>
    [SerializeField] private TMP_Text categoryText;

    /// <summary>
    /// 아이템 가격을 표시하는 TextMeshProUGUI 컴포넌트입니다.
    /// </summary>
    [SerializeField] private TMP_Text priceText;

    /// <summary>
    /// 아이템 수량을 표시하는 TextMeshProUGUI 컴포넌트입니다.
    /// </summary>
    [SerializeField] private TMP_Text quantityText;

    #endregion

    #region Data

    /// <summary>
    /// 현재 이 슬롯에 연결된 StoredItem 정보를 저장합니다.
    /// </summary>
    private StoredItem currentStoredItem;

    #endregion

    #region Highlight Settings

    /// <summary>
    /// 마우스 오버 시 적용할 강조 색상입니다.
    /// </summary>
    [SerializeField] private Color highlightColor = Color.yellow;

    /// <summary>
    /// 아이템 이름 텍스트의 원래 색상입니다.
    /// </summary>
    private Color itemNameOriginalColor;

    /// <summary>
    /// 아이템 가격 텍스트의 원래 색상입니다.
    /// </summary>
    private Color priceOriginalColor;

    /// <summary>
    /// 아이템 수량 텍스트의 원래 색상입니다.
    /// </summary>
    private Color quantityOriginalColor;

    /// <summary>
    /// 아이템 카테고리 텍스트의 원래 색상입니다.
    /// </summary>
    private Color categoryOriginalColor;

    #endregion

    /// <summary>
    /// StoredItem 데이터를 받아 UI를 초기화합니다.
    /// </summary>
    /// <param name="storedItem">UI를 설정하는 데 사용되는 StoredItem 데이터입니다.</param>
    public void Setup(StoredItem storedItem)
    {
        Debug.Log($"[InventoryItemSellUI] Setup called for {storedItem.item.itemName}");
        currentStoredItem = storedItem;

        if (itemNameText != null)
        {
            itemNameText.text = storedItem.item.itemName;
            itemNameOriginalColor = itemNameText.color;
        }

        // 인벤토리 가격은 현재 행성 가격으로 표시합니다.(수정중)
        // TODO
        if (priceText != null)
        {
            priceText.text = storedItem.item.GetCurrentPrice().ToString("F2");
            priceOriginalColor = priceText.color;
        }

        if (quantityText != null)
        {
            quantityText.text = storedItem.quantity.ToString();
            quantityOriginalColor = quantityText.color;
        }

        // 물품의 종류(분류)도 표시합니다.
        if (categoryText != null)
        {
            categoryText.text = storedItem.item.category.ToString();
            categoryOriginalColor = categoryText.color;
        }
    }

    /// <summary>
    /// 마우스가 이 슬롯 위로 들어오면 호출되며, 모든 텍스트 컴포넌트의 색상을 강조 색상으로 변경합니다.
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터</param>
    public void OnPointerEnter(PointerEventData eventData)
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

    /// <summary>
    /// 마우스가 이 슬롯에서 벗어나면 호출되며, 모든 텍스트 컴포넌트의 색상을 원래 색상으로 복구합니다.
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터</param>
    public void OnPointerExit(PointerEventData eventData)
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
