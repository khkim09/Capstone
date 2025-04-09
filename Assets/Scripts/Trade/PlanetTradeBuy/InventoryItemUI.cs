using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// InventoryItemUI는 인벤토리 항목의 정보를 UI로 표시하는 컴포넌트입니다.
/// 아이템의 이름, 가격, 수량 및 (필요한 경우) 아이콘을 표시합니다.
/// </summary>
public class InventoryItemUI : MonoBehaviour
{
    /// <summary>
    /// 아이템 이름을 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TMP_Text itemNameText;
    /// <summary>
    /// 아이템 가격을 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TMP_Text priceText;
    /// <summary>
    /// 아이템 수량을 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TMP_Text quantityText;
    // (아이콘이 있다면) [SerializeField] private Image itemIcon;

    /// <summary>
    /// 현재 이 슬롯에 연결된 StoredItem 정보를 저장합니다.
    /// </summary>
    private StoredItem currentStoredItem;

    /// <summary>
    /// StoredItem 데이터를 받아 UI를 초기화합니다.
    /// </summary>
    /// <param name="storedItem">UI를 설정하는 데 사용되는 StoredItem 데이터입니다.</param>
    public void Setup(StoredItem storedItem)
    {
        Debug.Log($"[InventoryItemUI] Setup called for {storedItem.item.itemName}");
        currentStoredItem = storedItem;

        if (itemNameText != null)
            itemNameText.text = storedItem.item.itemName;

        // 인벤토리 가격은 원본 가격으로 보이게 설정합니다.
        if (priceText != null)
            priceText.text = storedItem.item.GetBasePrice().ToString("F2");

        if (quantityText != null)
            quantityText.text = storedItem.quantity.ToString();
        // 아이콘 설정이 필요하다면, storedItem.item에 아이콘 정보가 있어야 하며 여기서 적용합니다.
    }

}
