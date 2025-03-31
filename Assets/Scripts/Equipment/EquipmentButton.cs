using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 각 장비 버튼입니다.
/// </summary>
public class EquipmentButton : MonoBehaviour
{
    /// <summary>
    /// 장비 별 이미지, 장비명, 가격
    /// </summary>
    [Header("UI Elements")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button myButton;

    /// <summary>
    /// ScriptableObject(장비 데이터) 연결
    /// </summary>
    [Header("Equipment Data")]
    public EquipmentItem linkedItem; // ScriptableObject(장비 데이터) 연결

    /// <summary>
    /// 장비 관련 UI Handler
    /// </summary>
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

    /// <summary>
    /// UI를 자동으로 갱신합니다.
    /// </summary>
    private void UpdateUI()
    {
        if (linkedItem == null)
            return;

        // ScriptableObject의 데이터를 UI에 반영
        iconImage.sprite = linkedItem.eqIcon;
        nameText.text = linkedItem.eqName;
        priceText.text = linkedItem.eqPrice.ToString();
    }

    /// <summary>
    /// 장비 버튼 클릭 시 호출될 함수
    /// </summary>
    public void OnClickItem()
    {
        uiHandler.ShowItemTip(linkedItem, this);
    }

    /// <summary>
    /// 구매 후 구매를 했음을 알 수 있도록 표시
    /// </summary>
    /// <param name="purchasedColor"></param>
    public void MarkAsPurchased(Color purchasedColor)
    {
        ColorBlock cb = myButton.colors;
        cb.normalColor = purchasedColor;
        cb.highlightedColor = purchasedColor;
        cb.pressedColor = purchasedColor * 0.9f;
        cb.selectedColor = purchasedColor;
        myButton.colors = cb;
    }

    /// <summary>
    /// 다시 처음과 같은 상태로 되돌림림
    /// </summary>
    /// <param name="defaultColor"></param>
    public void MarkAsDefault(Color defaultColor)
    {
        ColorBlock cb = myButton.colors;
        cb.normalColor = defaultColor;
        cb.highlightedColor = defaultColor;
        cb.pressedColor = defaultColor * 0.9f;
        cb.selectedColor = defaultColor;
        myButton.colors = cb;
    }

    /// <summary>
    /// 연결될 장비 호출
    /// </summary>
    /// <returns></returns>
    public EquipmentItem GetLinkedItem()
    {
        return linkedItem;
    }
}
