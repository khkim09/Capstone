using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MiddlePanelUI는 중간 패널의 UI를 관리하며, 선택된 아이템의 상세 정보와 거래(구매, 판매) 기능을 제공합니다.
/// </summary>
public class MiddlePanelUI : MonoBehaviour
{
    #region UI References

    /// <summary>
    /// 선택된 아이템 이름을 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [Header("UI References")]
    [SerializeField] private TMP_Text selectedItemNameText;

    /// <summary>
    /// 선택된 아이템 설명을 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TMP_Text selectedItemDescriptionText;
    /// <summary>
    /// 거래 수량을 입력받는 텍스트 입력 필드입니다.
    /// </summary>
    [SerializeField] private TMP_InputField tradeNumInputField;
    /// <summary>
    /// 구매 버튼입니다.
    /// </summary>
    [SerializeField] private Button buyButton;
    /// <summary>
    /// 판매 버튼입니다.
    /// </summary>
    [SerializeField] private Button sellButton;
    /// <summary>
    /// 플레이어의 재화(Coma)를 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TMP_Text playerComaText;

    #endregion

    #region Private Fields

    /// <summary>
    /// 현재 선택된 TradableItem입니다.
    /// </summary>
    private TradableItem selectedItem;
    /// <summary>
    /// 거래 관련 로직을 담당하는 TradeManager 컴포넌트입니다.
    /// </summary>
    private TradeManager tradeManager;

    #endregion

    /// <summary>
    /// 초기화 작업을 수행하고 버튼 이벤트를 등록하는 메서드입니다.
    /// </summary>
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
    /// 선택된 아이템의 상세 정보를 UI에 표시하는 메서드입니다.
    /// </summary>
    /// <param name="item">UI에 표시할 선택된 TradableItem입니다.</param>
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

    /// <summary>
    /// 구매 버튼 클릭 시 호출되는 메서드입니다.
    /// 선택된 아이템의 구매를 시도하고, 성공 시 인벤토리 UI를 갱신합니다.
    /// </summary>
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

    /// <summary>
    /// 판매 버튼 클릭 시 호출되는 메서드입니다.
    /// 선택된 아이템의 판매를 시도하고, 성공 시 인벤토리 UI를 갱신합니다.
    /// </summary>
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
    /// <summary>
    /// 플레이어의 COMA(재화)를 UI에 갱신하는 메서드입니다.
    /// </summary>
    public void UpdatePlayerComa()
    {
        if (playerComaText != null && tradeManager != null)
        {
            playerComaText.text = "COMA: " + tradeManager.GetPlayerCOMA();
        }
    }

}
