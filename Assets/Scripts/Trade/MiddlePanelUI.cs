using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiddlePanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text selectedItemNameText;     // 선택된 아이템 이름
    [SerializeField] private TMP_Text selectedItemDescriptionText; // 선택된 아이템 설명
    [SerializeField] private TMP_InputField tradeNumInputField;    // 거래 수량 입력 필드
    [SerializeField] private Button buyButton;                   // 구매 버튼
    [SerializeField] private Button sellButton;                  // 판매 버튼
    [SerializeField] private TMP_Text playerComaText;            // 플레이어 재화 텍스트

    private TradableItem selectedItem;
    private TradeManager tradeManager;

    private void Start()
    {
        tradeManager = FindObjectOfType<TradeManager>();
        // 버튼 이벤트 등록
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);
        if (sellButton != null)
            sellButton.onClick.AddListener(OnSellClicked);

        UpdatePlayerComa();
    }

    /// <summary>
    /// 왼쪽 인벤토리 아이템 클릭 시 호출되어 선택된 아이템의 상세정보를 표시합니다.
    /// </summary>
    /// <param name="item">선택된 TradableItem</param>
    public void SetSelectedItem(TradableItem item)
    {
        selectedItem = item;
        if (selectedItemNameText != null)
            selectedItemNameText.text = item.itemName;
        if (selectedItemDescriptionText != null)
            selectedItemDescriptionText.text = item.description;
        if (tradeNumInputField != null)
            tradeNumInputField.text = "1"; // 기본 거래 수량 1
    }

    private void OnBuyClicked()
    {
        if (selectedItem == null) return;

        int quantity = 1;
        int.TryParse(tradeNumInputField.text, out quantity);

        if (tradeManager.BuyItem(selectedItem, quantity))
        {
            Debug.Log($"구매 성공: {selectedItem.itemName} x {quantity}");
            UpdatePlayerComa();
            // 인벤토리 UI 갱신
            InventoryUI invUI = FindObjectOfType<InventoryUI>();
            if (invUI != null)
            {
                invUI.PopulateInventory();
            }
        }
    }

    private void OnSellClicked()
    {
        if (selectedItem == null) return;

        int quantity = 1;
        int.TryParse(tradeNumInputField.text, out quantity);

        if (tradeManager.SellItem(selectedItem, quantity))
        {
            Debug.Log($"판매 성공: {selectedItem.itemName} x {quantity}");
            UpdatePlayerComa();
            // 인벤토리 UI 갱신
            InventoryUI invUI = FindObjectOfType<InventoryUI>();
            if (invUI != null)
            {
                invUI.PopulateInventory();
            }
        }
    }
    public void UpdatePlayerComa()
    {
        if (playerComaText != null && tradeManager != null)
        {
            playerComaText.text = "COMA: " + tradeManager.GetPlayerCOMA();
        }
    }

}
