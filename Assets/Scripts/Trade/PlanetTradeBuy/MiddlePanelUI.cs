using System.Collections.Generic;
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
    [SerializeField] private TextMeshProUGUI selectedItemNameText;

    /// <summary>
    /// 선택된 아이템 설명을 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TextMeshProUGUI selectedItemDescriptionText;

    /// <summary>
    /// 선택된 아이템 가격을 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TextMeshProUGUI priceText;

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
    /// 아이템 형태를 보여줄 UI Image입니다.
    /// </summary>
    [SerializeField] private Image itemShapeImage;

    /// <summary>
    /// 아이템 모양에 해당하는 스프라이트 리스트입니다. itemShape enum 순서에 맞춰 등록해야 합니다.
    /// </summary>
    [SerializeField] private List<Sprite> itemShapeSprites;

    /// <summary>
    /// 임시 테스트용 코마 관리 텍스트입니다. TODO
    /// </summary>
    [SerializeField] private TextMeshProUGUI playerCOMAText;

    #endregion

    #region Private Fields

    /// <summary>
    /// 현재 선택된 아이템 데이터입니다.
    /// </summary>
    private TradingItemData selectedItemData;

    /// <summary>
    /// 거래 관련 로직을 담당하는 TradeManager 컴포넌트입니다.
    /// </summary>
    private TradeManager tradeManager;

    #endregion

    /// <summary>
    /// 시작시 비활성화 상태로 두는 코드입니다.
    /// </summary>
    private void Awake()
    {
        gameObject.SetActive(false); // 시작 시 자기 자신 비활성화
    }

    /// <summary>
    /// 초기화 작업을 수행하고 버튼 이벤트를 등록하는 메서드입니다.
    /// </summary>
    private void Start()
    {
        tradeManager = Object.FindFirstObjectByType<TradeManager>();
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);
        if (sellButton != null)
            sellButton.onClick.AddListener(OnSellClicked);

        UpdatePlayerComa();
    }



    /// <summary>
    /// 선택된 아이템의 상세 정보를 UI에 표시하는 메서드입니다.
    /// </summary>
    /// <param name="itemData">UI에 표시할 선택된 아이템 데이터입니다.</param>
    public void SetSelectedItem(TradingItemData itemData)
    {
        selectedItemData = itemData;

        if (selectedItemNameText != null)
            selectedItemNameText.text = itemData.itemName.Localize();

        if (selectedItemDescriptionText != null)
            selectedItemDescriptionText.text = itemData.description.Localize();


        if (priceText != null)
            priceText.text = itemData.costBase.ToString("F2") + " COMA";

        if (tradeNumInputField != null)
            tradeNumInputField.text = "1";

        if (itemShapeImage != null && itemShapeSprites != null && itemShapeSprites.Count > 0)
        {
            int shapeIndex = (int)itemData.shape;

            if (shapeIndex >= 0 && shapeIndex < itemShapeSprites.Count)
            {
                itemShapeImage.sprite = itemShapeSprites[shapeIndex];
                itemShapeImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"[MiddlePanelUI] itemShape index {shapeIndex} out of range for {itemData.itemName}");
                itemShapeImage.enabled = false;
            }
        }
    }

    /// <summary>
    /// 구매 버튼 클릭 시 호출되는 메서드입니다.
    /// 선택된 아이템의 구매를 시도하고, 성공 시 인벤토리 UI를 갱신합니다.
    /// </summary>
    private void OnBuyClicked()
    {
        if (selectedItemData == null) return;

        int quantity = 1;
        int.TryParse(tradeNumInputField.text, out quantity);

        if (tradeManager.BuyItem(selectedItemData, quantity))
        {
            Debug.Log($"구매 성공: {selectedItemData.itemName} x {quantity}");
            UpdatePlayerComa();

            InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
            if (invUI != null)
                invUI.PopulateInventory();
        }
    }

    /// <summary>
    /// 판매 버튼 클릭 시 호출되는 메서드입니다.
    /// 선택된 아이템의 판매를 시도하고, 성공 시 인벤토리 UI를 갱신합니다.
    /// </summary>
    private void OnSellClicked()
    {
        if (selectedItemData == null) return;

        int quantity = 1;
        int.TryParse(tradeNumInputField.text, out quantity);

        if (tradeManager.SellItem(selectedItemData, quantity))
        {
            Debug.Log($"판매 성공: {selectedItemData.itemName} x {quantity}");
            UpdatePlayerComa();

            InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
            if (invUI != null)
                invUI.PopulateInventory();
        }
    }

    /// <summary>
    /// 플레이어의 COMA(재화)를 UI에 갱신하는 메서드입니다.
    /// </summary>
    public void UpdatePlayerComa()
    {
        if (playerCOMAText != null)
        {
            playerCOMAText.text = "COMA: " + ResourceManager.Instance.COMA;
        }
    }

}
