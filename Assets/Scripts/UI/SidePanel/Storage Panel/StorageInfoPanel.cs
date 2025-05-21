using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StorageInfoPanel : TooltipPanelBase
{
    [SerializeField] private Button panelButton;
    [SerializeField] private Image panelBackground;

    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemCategory;
    [SerializeField] private TextMeshProUGUI itemAmount;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color slightlyDamagedColor = Color.Lerp(Color.white, Color.red, 0.33f);
    [SerializeField] private Color damagedColor = Color.Lerp(Color.white, Color.red, 0.66f);
    [SerializeField] private Color unsellableColor = Color.red;

    private Color originalColor = Color.white;
    [SerializeField] private Color hightlightFontColor = Color.yellow;

    [SerializeField]
    private Color selectedBackgroundColor = new(0.2f, 0.4f, 0.8f, 0.3f); // Background color when selected

    private TradingItemData currentItem;
    public TradingItemData CurrentItem => currentItem;
    private SlidePanelController slidePanelController;
    private bool isSelected = false;

    protected override void Start()
    {
        base.Start();
        slidePanelController = GameObject.FindWithTag("SlidePanel").GetComponent<SlidePanelController>();

        if (panelBackground == null)
        {
            panelBackground = GetComponent<Image>();
            if (panelBackground == null)
            {
                panelBackground = gameObject.AddComponent<Image>();
                panelBackground.color = new Color(0, 0, 0, 0); // Transparent by default
            }
        }
    }

    public void Initialize(TradingItem item)
    {
        currentItem = item.GetItemData();
        itemName.text = item.GetItemData().itemName.Localize();
        itemCategory.text = item.GetItemData().type.Localize();
        itemImage.sprite = item.GetItemSprite();

        switch (item.GetItemState())
        {
            case ItemState.Normal:
                itemImage.color = normalColor;
                break;
            case ItemState.SlightlyDamaged:
                itemImage.color = slightlyDamagedColor;
                break;
            case ItemState.Damaged:
                itemImage.color = damagedColor;
                break;
            case ItemState.Unsellable:
                itemImage.color = unsellableColor;
                break;
        }

        panelButton.onClick.RemoveAllListeners();
        panelButton.onClick.AddListener(() =>
        {
            slidePanelController.OnStorageInfoPanelButtonClicked(currentItem);
            slidePanelController.SetSelectedStoragePanel(this);
        });

        // Reset selection state
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selected)
        {
            panelBackground.color = selectedBackgroundColor;
            if (itemName != null) itemName.color = hightlightFontColor;
            if (itemCategory != null) itemCategory.color = hightlightFontColor;
            if (itemAmount != null) itemAmount.color = hightlightFontColor;
        }
        else
        {
            panelBackground.color = new Color(0, 0, 0, 0);
            if (itemName != null) itemName.color = originalColor;
            if (itemCategory != null) itemCategory.color = originalColor;
            if (itemAmount != null) itemAmount.color = originalColor;
        }
    }

    protected override void OnMouseEnter(PointerEventData eventData)
    {
        base.OnMouseEnter(eventData);

        if (!isSelected)
        {
            if (itemName != null) itemName.color = hightlightFontColor;
            if (itemCategory != null) itemCategory.color = hightlightFontColor;
            if (itemAmount != null) itemAmount.color = hightlightFontColor;
        }
    }

    protected override void OnMouseExit(PointerEventData eventData)
    {
        base.OnMouseExit(eventData);

        if (!isSelected)
        {
            if (itemName != null) itemName.color = originalColor;
            if (itemCategory != null) itemCategory.color = originalColor;
            if (itemAmount != null) itemAmount.color = originalColor;
        }
    }

    protected override void SetToolTipText()
    {
    }
}
