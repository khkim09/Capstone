using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// PlanetItemUI는 행성 상점(왼쪽 패널)에서 판매되는 각 아이템 슬롯을 관리하는 컴포넌트입니다.
/// 이 스크립트는 TradableItem 데이터를 받아 해당 아이템의 이름과 가격을 UI에 표시하고,
/// 슬롯이 클릭되면 MiddlePanelUI로 해당 아이템의 상세 정보를 전달하며, 마우스 오버 시 텍스트 색상이 강조됩니다.
/// </summary>
public class PlanetItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region UI References

    /// <summary>
    /// 아이템 이름을 표시하는 pointerclickTextMeshProUGUI 컴포넌트입니다.
    /// </summary>
    [SerializeField] private TMP_Text itemNameText;

    /// <summary>
    /// 아이템 가격을 표시하는 TextMeshProUGUI 컴포넌트입니다.
    /// </summary>
    [SerializeField] private TMP_Text priceText;

    /// <summary>MiddlePanelUI를 Inspector에서 할당받습니다.</summary>
    [SerializeField] private MiddlePanelUI middlePanel;

    #endregion

    #region Data

    /// <summary>
    /// 이 슬롯에 할당된 TradableItem 데이터입니다.
    /// </summary>
    private TradableItem tradableItem;

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

    #endregion

    /// <summary>
    /// PlanetItemUI를 초기화하고, 아이템 이름과 가격을 UI에 표시합니다.
    /// 또한, 텍스트 컴포넌트의 원래 색상을 저장합니다.
    /// </summary>
    /// <param name="item">표시할 TradableItem 데이터</param>
    public void Setup(TradableItem item)
    {
        tradableItem = item;
        if (itemNameText != null)
        {
            itemNameText.text = tradableItem.itemName;
            itemNameOriginalColor = itemNameText.color;
        }
        if (priceText != null)
        {
            priceText.text = tradableItem.GetCurrentPrice().ToString("F2");
            priceOriginalColor = priceText.color;
        }
    }

    /// <summary>
    /// 사용자가 이 슬롯을 클릭했을 때 호출됩니다.
    /// MiddlePanelUI에 상세 정보를 전달하고, 구매창 서브 패널을 활성화합니다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1) MiddlePanelUI 에 아이템 정보 전달 (원래 있던 코드)
        MiddlePanelUI middlePanel = FindObjectOfType<MiddlePanelUI>();
        if (middlePanel != null && tradableItem != null)
        {
            middlePanel.SetSelectedItem(tradableItem);
        }

        // 2) TradeUIManager 에 구매 상세·storage 패널 켜라고 알림
        if (TradeUIManager.Instance != null)
        {
            TradeUIManager.Instance.OnBuyItemSelected();
        }
        if (middlePanel != null)
            middlePanel.SetSelectedItem(tradableItem);
    }

    /// <summary>
    /// 마우스가 이 슬롯 위에 들어왔을 때 호출되며, 아이템 이름과 가격 텍스트의 색상을 강조 색상으로 변경합니다.
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemNameText != null)
        {
            itemNameText.color = highlightColor;
        }
        if (priceText != null)
        {
            priceText.color = highlightColor;
        }
    }

    /// <summary>
    /// 마우스가 이 슬롯을 벗어났을 때 호출되며, 아이템 이름과 가격 텍스트의 색상을 원래 색상으로 복구합니다.
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (itemNameText != null)
        {
            itemNameText.color = itemNameOriginalColor;
        }
        if (priceText != null)
        {
            priceText.color = priceOriginalColor;
        }
    }
}
