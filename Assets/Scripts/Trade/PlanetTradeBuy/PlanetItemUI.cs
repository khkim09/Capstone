using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// PlanetItemUI는 행성 상점(왼쪽 패널)에서 판매되는 각 아이템 슬롯을 관리합니다.
/// TradingItemData 데이터를 받아 이름, 가격을 UI에 표시하고, 클릭 시 MiddlePanelUI로 상세 정보를 전달합니다.
/// 선택/비선택 상태에 따라 색상을 동적으로 변경합니다.
/// </summary>
public class PlanetItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region UI References

    /// <summary>
    /// 아이템 아이콘을 표시하는 Image 컴포넌트입니다.
    /// </summary>
    [SerializeField] private Image iconImage;

    /// <summary>
    /// 아이템 모양을 시각적으로 표시할 Image 컴포넌트입니다.
    /// </summary>
    [SerializeField] private Image itemShapeImage;

    /// <summary>
    /// 아이템 모양 관련 List 컴포넌트입니다.
    /// </summary>
    [SerializeField] private List<Sprite> itemShapeSprites;

    /// <summary>
    /// 아이템 이름을 표시하는 TextMeshProUGUI 컴포넌트입니다.
    /// </summary>
    [SerializeField] private TextMeshProUGUI itemNameText;

    /// <summary>
    /// 아이템 종류를 표시하는 TextMeshProUGUI 컴포넌트입니다.
    /// </summary>
    [SerializeField] private TextMeshProUGUI categoryText;

    /// <summary>
    /// 아이템 최대 적재량을 표시하는 TextMeshProUGUI 컴포넌트입니다.
    /// </summary>
    [SerializeField] private TextMeshProUGUI capacityText;

    /// <summary>
    /// 아이템 가격을 표시하는 TextMeshProUGUI 컴포넌트입니다.
    /// </summary>
    [SerializeField] private TextMeshProUGUI priceText;

    /// <summary>
    /// 행성 물품과 연동된 Panel 컴포넌트 입니다.
    /// </summary>
    [SerializeField] private MiddlePanelUI middlePanel;

    #endregion

    #region Data & State

    private TradingItemData itemData;
    private static PlanetItemUI _currentSelectedItem = null;
    private static string _currentlySelectedItemName = "";
    private bool isSelected = false;

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
    /// PlanetItemUI를 초기화하고 텍스트 표시와 색상 초기 상태를 저장합니다.
    /// </summary>
    public void Setup(TradingItemData data)
    {
        itemData = data;

        if (iconImage != null)
            iconImage.sprite = itemData.itemSprite;

        if (itemShapeImage != null && itemData != null)
        {
            int shapeIndex = (int)itemData.shape;

            if (shapeIndex >= 0 && shapeIndex < itemShapeSprites.Count)
            {
                itemShapeImage.sprite = itemShapeSprites[shapeIndex];
                itemShapeImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"PlanetItemUI: itemShape index {shapeIndex} is out of range for {itemData.itemName}");
                itemShapeImage.enabled = false;
            }
        }

        if (itemNameText != null)
        {
            itemNameText.text = itemData.itemName.Localize();
            itemNameOriginalColor = itemNameText.color;
        }

        if (categoryText != null)
            categoryText.text = itemData.type.ToString();

        if (capacityText != null)
            capacityText.text = $"{itemData.capacity}";

        if (priceText != null)
        {
            priceText.text = itemData.costBase.ToString("F2");
            priceOriginalColor = priceText.color;
        }

        isSelected = false;

        if (_currentlySelectedItemName == data.itemName)
        {
            SetSelected(true);
            _currentSelectedItem = this;
        }
    }


    /// <summary>
    /// 사용자가 이 슬롯을 클릭했을 때 호출됩니다.
    /// MiddlePanelUI에 상세 정보를 전달하고, 구매창 서브 패널을 활성화합니다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (TradeUIManager.Instance != null)
        {
            TradeUIManager.Instance.OnBuyItemSelected();
        }
        if (_currentSelectedItem != null && _currentSelectedItem != this)
        {
            _currentSelectedItem.SetSelected(false);
        }

        // 중복 선택 방지 처리
        if (_currentSelectedItem != null && _currentSelectedItem != this)
        {
            _currentSelectedItem.SetSelected(false);
        }

        SetSelected(true);
        _currentlySelectedItemName = itemData.itemName.Localize();
        _currentSelectedItem = this;

        if (middlePanel != null)
        {
            middlePanel.gameObject.SetActive(true);
            middlePanel.SetSelectedItem(itemData);
        }
    }

    /// <summary>
    /// 마우스가 이 슬롯 위에 들어왔을 때 호출되며, 아이템 이름과 가격 텍스트의 색상을 강조 색상으로 변경합니다.
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
            ApplyColor(highlightColor);
    }

    /// <summary>
    /// 마우스가 이 슬롯을 벗어났을 때 호출되며, 아이템 이름과 가격 텍스트의 색상을 원래 색상으로 복구합니다.
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
            ApplyOriginalColor();
    }

    /// <summary>
    /// 선택된 물품의 색상을 변경하는 함수입니다.
    /// </summary>
    /// <param name="selected"></param>
    private void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selected)
            ApplyColor(highlightColor);
        else
            ApplyOriginalColor();
    }

    /// <summary>
    /// 색상 변경을 적용하는 함수입니다.
    /// </summary>
    /// <param name="color"></param>
    private void ApplyColor(Color color)
    {
        if (itemNameText != null)
            itemNameText.color = color;
        if (categoryText != null)
            categoryText.color = color;
        if (capacityText != null)
            capacityText.color = color;
        if (priceText != null)
            priceText.color = color;
    }

    /// <summary>
    /// 원래의 텍스트 색상으로 되돌리는 함수입니다.
    /// </summary>
    private void ApplyOriginalColor()
    {
        if (itemNameText != null)
            itemNameText.color = itemNameOriginalColor;
        if (categoryText != null)
            categoryText.color = itemNameOriginalColor;
        if (capacityText != null)
            capacityText.color = itemNameOriginalColor;
        if (priceText != null)
            priceText.color = priceOriginalColor;
    }
}
