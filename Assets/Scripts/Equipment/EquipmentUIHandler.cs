using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Collections.Generic;

public class EquipmentUIHandler : MonoBehaviour
{
    [Header("Tip Panel References")] public GameObject itemBuyPanel;
    public Image tipItemImage;
    public TextMeshProUGUI tipItemName;
    public TextMeshProUGUI tipItemPrice;
    public TextMeshProUGUI tipItemDetails;
    public TextMeshProUGUI tipCurrencyText;
    public Button buyButton;
    public Button backButton;

    [Header("Color Settings")] public Color purchasedButtonColor = Color.gray;
    public Color defaultButtonColor = Color.white;

    // 현재 선택된 장비
    public EquipmentItem currentSelectedItem;
    public EquipmentButton currentSelectedButton;

    // 구매한 아이템 목록
    public HashSet<EquipmentItem> purchasedItems = new();


    // 임시로 플레이어 보유 재화를 관리 (실제론 GameManager 등 다른 스크립트에서 관리 가능)
    public int playerCOMA = 10000;

    private void Start()
    {
        // 팝업 비활성화
        itemBuyPanel.SetActive(false);
    }

    public void ShowItemTip(EquipmentItem eqItem, EquipmentButton eqButton)
    {
        currentSelectedItem = eqItem;
        currentSelectedButton = eqButton;

        // 팝업 UI 채우기
        tipItemImage.sprite = eqItem.eqIcon;
        tipItemName.text = eqItem.eqName;
        tipItemPrice.text = eqItem.eqPrice.ToString();
        // tipItemDetails = eqItem.Details;
        tipCurrencyText.text = "COMA: " + playerCOMA;

        // 이미 구매한 아이템이면 buy 버튼 비활성화
        if (purchasedItems.Contains(eqItem))
            buyButton.interactable = false;
        else
            // 구매 가능 여부 확인
            buyButton.interactable = playerCOMA >= eqItem.eqPrice;

        // 팝업 표시
        itemBuyPanel.SetActive(true);
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

        EquipmentItem eq = currentSelectedItem;

        // 장비 타입 확인 (global, personal)
        if (eq.isGlobalEquip) // global
        {
            Debug.Log("장비 전체 적용 명령");
            EquipmentManager.Instance.PurchaseAndEquipGlobal(eq); // 오류

            // 전체 crewList에 장비 적용 UI 추가 필요 (discord 참조)
        }
        else // personal
        {
            CrewBase selectedCrew = CrewManager.Instance.GetSelectedCrew();

            if (selectedCrew == null)
            {
                // 장비 착용할 선원 경고 UI 필요
                Debug.LogWarning("장비 착용 실패, 선택된 선원 X");
                return;
            }

            EquipmentManager.Instance.PurchaseAndEquipPersonal(selectedCrew, eq);
        }

        // 팝업 닫기
        itemBuyPanel.SetActive(false);

        // 버튼 색 변경
        currentSelectedButton.MarkAsPurchased(purchasedButtonColor);
    }

    public void OnClickBack()
    {
        // 팝업 닫기
        itemBuyPanel.SetActive(false);
    }
}
