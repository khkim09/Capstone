using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 장비 하나를 나타내는 UI 버튼 클래스.
/// 이미지, 이름, 가격 등의 정보를 표시하고, 클릭 시 장비 상세 정보를 보여줍니다.
/// </summary>
public class EquipmentButton : MonoBehaviour
{
    /// <summary>
    /// 장비 아이콘 이미지입니다.
    /// </summary>
    [Header("UI Elements")] public Image iconImage;

    /// <summary>
    /// 장비 이름 텍스트입니다.
    /// </summary>
    public TextMeshProUGUI nameText;

    /// <summary>
    /// 장비 가격 텍스트입니다.
    /// </summary>
    public TextMeshProUGUI priceText;

    /// <summary>
    /// 장비 버튼 자체입니다.
    /// </summary>
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

    /// <summary>
    /// 시작 시 연결된 장비가 있다면 UI를 초기화합니다.
    /// </summary>
    private void Awake()
    {
        if (linkedItem)
            UpdateUI();
    }

    /// <summary>
    /// 에디터에서 값이 변경될 경우 자동으로 UI를 갱신합니다.
    /// </summary>
    private void OnValidate()
    {
        // 에디터에서 변경 사항이 있을 때 자동 업데이트
        if (linkedItem)
        {
            UpdateUI();
        }
    }

    /// <summary>
    /// 연결된 장비 데이터를 바탕으로 UI를 갱신합니다.
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
