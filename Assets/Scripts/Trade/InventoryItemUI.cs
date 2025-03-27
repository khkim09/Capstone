using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    // UI 참조 (프리팹 내부에 배치한 텍스트, 이미지 등)
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text quantityText;
    // (아이콘이 있다면) [SerializeField] private Image itemIcon;

    // 현재 이 슬롯에 연결된 StoredItem 정보를 저장
    private StoredItem currentStoredItem;

    /// <summary>
    /// StoredItem 데이터를 받아 UI를 초기화합니다.
    /// </summary>
    public void Setup(StoredItem storedItem)
    {
        Debug.Log($"[InventoryItemUI] Setup called for {storedItem.item.itemName}");
        currentStoredItem = storedItem;

        if (itemNameText != null)
            itemNameText.text = storedItem.item.itemName;

        if (priceText != null)
            priceText.text = storedItem.item.basePrice.ToString();

        if (quantityText != null)
            quantityText.text = storedItem.quantity.ToString();
        // 아이콘 설정이 필요하다면, storedItem.item에 아이콘 정보가 있어야 하며 여기서 적용합니다.
    }

    /// <summary>
    /// 이 슬롯이 클릭되었을 때 호출되는 함수입니다.
    /// Inspector에서 Button의 OnClick 이벤트에 이 함수를 연결하세요.
    /// </summary>
    public void OnItemClicked()
    {
        // MiddlePanelUI를 찾아서 선택된 아이템을 전달
        MiddlePanelUI middlePanel = FindObjectOfType<MiddlePanelUI>();
        if (middlePanel != null && currentStoredItem != null)
        {
            middlePanel.SetSelectedItem(currentStoredItem.item);
        }
    }
}
