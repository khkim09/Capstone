using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentButton : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button myButton;

    public EquipmentItem linkedItem; // ScriptableObject(장비 데이터) 연결

    // 팝업을 띄우는 매니저(혹은 UI 핸들러)에 대한 참조
    public EquipmentUIHandler uiHandler;

    // 이 버튼이 어떤 장비를 나타내는지 설정하는 함수
    public void SetItem(EquipmentItem eqItem, EquipmentUIHandler handler)
    {
        linkedItem = eqItem;
        uiHandler = handler;

        // UI 갱신
        iconImage.sprite = eqItem.eqIcon;           // 아이콘 이미지
        nameText.text = eqItem.eqName;     // 장비명
        priceText.text = eqItem.eqPrice.ToString(); // 가격
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
