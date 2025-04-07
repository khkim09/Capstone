using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// PlanetItemUI는 행성 상점(왼쪽 패널)에서 판매되는 각 아이템 슬롯을 관리하는 컴포넌트입니다.
/// 이 스크립트는 TradableItem 데이터를 받아 해당 아이템의 이름과 가격을 UI에 표시하고,
/// 슬롯이 클릭되면 MiddlePanelUI로 해당 아이템의 상세 정보를 전달합니다.
/// </summary>
public class PlanetItemUI : MonoBehaviour, IPointerClickHandler
{
    #region UI References

    /// <summary>
    /// 행성 상점 슬롯에 표시할 아이템 이름을 위한 텍스트 컴포넌트입니다.
    /// </summary>
    [SerializeField]
    private TMP_Text itemNameText;

    /// <summary>
    /// 행성 상점 슬롯에 표시할 아이템 가격을 위한 텍스트 컴포넌트입니다.
    /// </summary>
    [SerializeField]
    private TMP_Text priceText;

    #endregion

    #region Data

    /// <summary>
    /// 이 슬롯에 할당된 TradableItem 데이터입니다.
    /// </summary>
    private TradableItem tradableItem;

    #endregion

    /// <summary>
    /// PlanetItemUI를 초기화하고, 아이템 이름과 가격을 UI에 표시합니다.
    /// </summary>
    /// <param name="item">표시할 TradableItem 데이터</param>
    public void Setup(TradableItem item)
    {
        tradableItem = item;
        if (itemNameText != null)
        {
            itemNameText.text = tradableItem.itemName;
        }
        if (priceText != null)
        {
            // GetCurrentPrice()를 통해 변동폭이 반영된 가격을 표시합니다.
            priceText.text = tradableItem.GetCurrentPrice().ToString("F2");
        }
    }

    /// <summary>
    /// 사용자가 이 슬롯을 클릭했을 때 호출됩니다.
    /// 클릭 이벤트를 통해 MiddlePanelUI의 SetSelectedItem 메서드를 호출하여,
    /// 중앙 패널에 아이템의 상세 정보(이름, 설명 등)를 표시하도록 합니다.
    /// </summary>
    /// <param name="eventData">포인터 클릭 이벤트 데이터</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        MiddlePanelUI middlePanel = FindObjectOfType<MiddlePanelUI>();
        if (middlePanel != null && tradableItem != null)
        {
            middlePanel.SetSelectedItem(tradableItem);
        }
    }
}
