using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Collections.Generic;

/// <summary>
/// 장비 관련 UI 담당 Handler
/// </summary>
public class EquipmentUIHandler : MonoBehaviour
{
    /// <summary>
    /// 장비 구매 팝업 패널입니다.
    /// </summary>
    [Header("Tip Panel References")] public GameObject itemBuyPanel;

    /// <summary>
    /// 팝업 내 장비 아이콘 이미지입니다.
    /// </summary>
    public Image tipItemImage;

    /// <summary>
    /// 팝업 내 장비 이름 텍스트입니다.
    /// </summary>
    public TextMeshProUGUI tipItemName;

    /// <summary>
    /// 팝업 내 장비 가격 텍스트입니다.
    /// </summary>
    public TextMeshProUGUI tipItemPrice;

    /// <summary>
    /// 팝업 내 장비 설명 텍스트입니다.
    /// </summary>
    public TextMeshProUGUI tipItemDetails;

    /// <summary>
    /// 팝업 내 현재 보유 재화 표시 텍스트입니다.
    /// </summary>
    public TextMeshProUGUI tipCurrencyText;

    /// <summary>
    /// 구매 버튼입니다.
    /// </summary>
    public Button buyButton;

    /// <summary>
    /// 뒤로 가기 버튼입니다.
    /// </summary>
    public Button backButton;

    /// <summary>
    /// 장비 구매 완료 시 버튼에 적용할 색상입니다.
    /// </summary>
    [Header("Color Settings")] public Color purchasedButtonColor = Color.gray;

    /// <summary>
    /// 장비 버튼의 기본 색상입니다.
    /// </summary>
    public Color defaultButtonColor = Color.white;

    /// <summary>
    /// 현재 선택된 장비 데이터입니다.
    /// </summary>
    public EquipmentItem currentSelectedItem;

    /// <summary>
    /// 현재 선택된 장비 버튼입니다.
    /// </summary>
    public EquipmentButton currentSelectedButton;

    /// <summary>
    /// 구매한 장비 목록입니다.
    /// 중복 구매를 방지하기 위해 HashSet으로 관리됩니다.
    /// </summary>
    public HashSet<EquipmentItem> purchasedItems = new();


    /// <summary>
    /// 임시로 관리하는 플레이어 보유 재화(COMAs)입니다.
    /// </summary>
    public int playerCOMA = 10000;

    /// <summary>
    /// 시작 시 장비 팝업을 비활성화합니다.
    /// </summary>
    private void Start()
    {
        // 팝업 비활성화
        itemBuyPanel.SetActive(false);
    }

    /// <summary>
    /// 장비 버튼 클릭 시 팝업을 띄우고 UI 요소를 채웁니다.
    /// 이미 구매했거나 재화 부족 시 구매 불가 처리도 포함됩니다.
    /// </summary>
    /// <param name="eqItem">선택된 장비 아이템.</param>
    /// <param name="eqButton">선택된 장비 버튼.</param>
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

    /// <summary>
    /// 구매 버튼 클릭 시 호출됩니다.
    /// 재화를 차감하고, 장비 효과를 적용하며, 버튼 색상도 변경됩니다.
    /// </summary>
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

    /// <summary>
    /// 장비 구매 팝업을 닫습니다.
    /// </summary>
    public void OnClickBack()
    {
        // 팝업 닫기
        itemBuyPanel.SetActive(false);
    }
}
