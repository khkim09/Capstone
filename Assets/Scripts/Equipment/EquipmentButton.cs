using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class EquipmentButton : MonoBehaviour
{
    [Header("UI Elements")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button myButton;

    [Header("Equipment Data")]
    public EquipmentItem linkedItem; // ScriptableObject(장비 데이터) 연결

    [Header("UI Handler")]
    public EquipmentUIHandler uiHandler;

    private void Awake()
    {
        if (linkedItem)
            UpdateUI();
    }

    private void OnValidate()
    {
        // 에디터에서 변경 사항이 있을 때 자동 업데이트
        if (linkedItem)
        {
            UpdateUI();
        }
    }

    // UI를 자동으로 갱신하는 함수
    private void UpdateUI()
    {
        if (linkedItem == null)
            return;

        // ScriptableObject의 데이터를 UI에 반영
        iconImage.sprite = linkedItem.eqIcon;
        nameText.text = linkedItem.eqName;
        priceText.text = linkedItem.eqPrice.ToString();
    }

    // 버튼 클릭 시 호출될 함수
    public void OnClickItem()
    {
        uiHandler.ShowItemTip(linkedItem, this);
    }

    public void MarkAsPurchased(Color purchasedColor)
    {
        ColorBlock cb = myButton.colors;
        cb.normalColor = purchasedColor;
        cb.highlightedColor = purchasedColor;
        cb.pressedColor = purchasedColor * 0.9f;
        cb.selectedColor = purchasedColor;
        myButton.colors = cb;
    }

    public void MarkAsDefault(Color defaultColor)
    {
        ColorBlock cb = myButton.colors;
        cb.normalColor = defaultColor;
        cb.highlightedColor = defaultColor;
        cb.pressedColor = defaultColor * 0.9f;
        cb.selectedColor = defaultColor;
        myButton.colors = cb;
    }

    public EquipmentItem GetLinkedItem()
    {
        return linkedItem;
    }
}
