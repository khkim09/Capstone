using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class EquipmentUIHandler : MonoBehaviour
{
    [Header("Tip Panel References")]
    public GameObject itemTipPanel;
    public Image tipItemImage;
    public TextMeshProUGUI tipItemName;
    public TextMeshProUGUI tipItemPrice;
    public TextMeshProUGUI tipItemDetaiils;
    public TextMeshProUGUI tipCurrencyText;
    public Button buyButton;
    public Button backButton;

    [Header("Color Settings")]
    public Color purchasedButtonColor = Color.gray;
    public Color defaultButtonColor = Color.white;

    // 현재 선택된 장비
    private EquipmentItem currentSelectedItem;
    private EquipmentButton currentSelectedButton;

    // 구매한 아이템 목록
    private HashSet<EquipmentItem> purchasedItems = new HashSet<EquipmentItem>();

    // 임시로 플레이어 보유 재화를 관리 (실제론 GameManager 등 다른 스크립트에서 관리 가능)
    public int playerCOMA = 10000;

    private void Start()
    {
        // 팝업 비활성화
        itemTipPanel.SetActive(false);
    }


    public void ShowItemTip(EquipmentItem eqItem, EquipmentButton eqButton)
    {
        currentSelectedItem = eqItem;
        currentSelectedButton = eqButton;

        // 팝업 UI 채우기
        tipItemImage.sprite = eqItem.eqIcon;
        tipItemName.text = eqItem.eqName;
        tipItemPrice.text = eqItem.eqPrice.ToString();
        // tipItemDetaiils = eqItem.Details;
        tipCurrencyText.text = "COMA: " + playerCOMA;

        // 이미 구매한 아이템이면 buy 버튼 비활성화
        if (purchasedItems.Contains(eqItem))
        {
            buyButton.interactable = false;
        }
        else
        {
            // 구매 가능 여부 확인
            buyButton.interactable = (playerCOMA >= eqItem.eqPrice);
        }

        // 팝업 표시
        itemTipPanel.SetActive(true);
    }

    public void OnClickBuy()
    {
        // 이미 구매했거나 골드 부족하면 return
        if (purchasedItems.Contains(currentSelectedItem))
            return;
        if (playerCOMA < currentSelectedItem.eqPrice)
            return;

        // 구매 처리
        playerCOMA -= currentSelectedItem.eqPrice;
        purchasedItems.Add(currentSelectedItem);


        // 팝업 닫기
        itemTipPanel.SetActive(false);

        // 버튼 색 변경
        currentSelectedButton.MarkAsPurchased(purchasedButtonColor);

        Debug.Log($"{currentSelectedItem.eqName} 구매 완료! 남은 골드: {playerCOMA}");
    }

    public void OnClickBack()
    {
        // 팝업 닫기
        itemTipPanel.SetActive(false);
    }
}
