using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuyItemInfoPanel : TooltipPanelBase, IPointerClickHandler
{
    [SerializeField] private Button panelButton;
    [SerializeField] private Image panelBackground;

    [SerializeField] private Image itemShape;
    [SerializeField] private List<Sprite> itemShapeSprites;

    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemCategory;
    [SerializeField] private TextMeshProUGUI itemAmount;
    [SerializeField] private TextMeshProUGUI itemPrice;

    private Color originalColor = Color.white;
    [SerializeField] private float hightlightAlpha = 180f;

    [SerializeField]
    private Color selectedBackgroundColor = new(0.2f, 0.4f, 0.8f, 0.3f); // Background color when selected

    private TradingItemData currentItem;
    public TradingItemData CurrentItem => currentItem;
    private TradeUIController tradeUIController;
    private bool isSelected = false;

    protected override void Start()
    {
        base.Start();
        tradeUIController = GameObject.FindWithTag("TradeUIController").GetComponent<TradeUIController>();

        if (panelBackground == null)
        {
            panelBackground = GetComponent<Image>();
            if (panelBackground == null)
            {
                panelBackground = gameObject.AddComponent<Image>();
                panelBackground.color = new Color(0, 0, 0, 0); // Transparent by default
            }
        }

        // 텍스트 원래 색상 저장
        if (itemName != null) originalColor = itemName.color;
    }

    public void Initialize(TradingItemData item)
    {
        currentItem = item;
        itemShape.sprite = itemShapeSprites[item.shape];
        itemName.text = item.itemName.Localize();
        itemCategory.text = item.type.Localize();
        itemImage.sprite = item.itemSprite;
        itemAmount.text = item.capacity.ToString();


        itemPrice.text = GameManager.Instance.WhereIAm().GetItemPrice(item).ToString();
    }

    protected override void OnMouseEnter(PointerEventData eventData)
    {
        base.OnMouseEnter(eventData);

        // 호버 시 아이템 맵 표시
        if (currentItem != null && tradeUIController != null) tradeUIController.ShowBuyItemMap(currentItem);

        // 선택되지 않은 상태일 때만 호버 효과 적용
        if (!isSelected)
        {
            // panelBackground 알파값 변경 (180/255)
            Color backgroundColor = panelBackground.color;
            backgroundColor.a = hightlightAlpha / 255f;
            panelBackground.color = backgroundColor;
        }
    }

    protected override void OnMouseExit(PointerEventData eventData)
    {
        base.OnMouseExit(eventData);

        // 호버에서 벗어날 때 아이템 맵 숨김
        if (tradeUIController != null) tradeUIController.HideItemMap();

        // 선택되지 않은 상태일 때만 호버 효과 제거
        if (!isSelected)
        {
            // panelBackground 알파값 0으로 변경 (완전 투명)
            Color backgroundColor = panelBackground.color;
            backgroundColor.a = 0f;
            panelBackground.color = backgroundColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 마우스 우클릭 감지
        if (eventData.button == PointerEventData.InputButton.Left)
            if (currentItem != null && tradeUIController != null)
                tradeUIController.ShowBuyCheckPanel(currentItem);
    }

    protected override void SetToolTipText()
    {
        // 필요한 경우 툴팁 텍스트 설정 가능
    }
}
