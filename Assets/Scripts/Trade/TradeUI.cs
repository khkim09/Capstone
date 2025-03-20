using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class TradeUI : MonoBehaviour
{
    [Header("UI References")]
    // 무역 아이템 목록을 표시할 컨테이너 (ScrollView Content 등)
    [SerializeField] private Transform itemListContainer;
    // 각 무역 아이템 정보를 표시할 프리팹 (하위에 TradeItemUI 스크립트가 포함되어 있어야 함)
    [SerializeField] private GameObject tradeItemPrefab;
    // 플레이어 재화(COMA)를 표시할 텍스트
    [SerializeField] private Text playerCOMAText;

    [Header("System References")]
    // TradeManager와 TradeDataLoader, Storage는 씬 내에 존재하거나 인스펙터에 할당
    [SerializeField] private TradeManager tradeManager;
    [SerializeField] private TradeDataLoader tradeDataLoader;

    private void Start()
    {
        // 참조가 없다면 씬에서 찾아봅니다.
        if (tradeManager == null)
            tradeManager = FindObjectOfType<TradeManager>();
        if (tradeDataLoader == null)
            tradeDataLoader = FindObjectOfType<TradeDataLoader>();

        PopulateTradeItemList();
        UpdatePlayerCOMA();
    }

    /// <summary>
    /// TradeDataLoader에서 로드된 TradableItem 데이터를 기반으로, 아이템 목록 UI를 동적으로 생성합니다.
    /// 각 아이템 프리팹에는 TradeItemUI 컴포넌트가 있어야 하며, 이를 통해 아이템 정보 표시 및 구매/판매 버튼 이벤트를 처리합니다.
    /// </summary>
    private void PopulateTradeItemList()
    {
        // 기존 항목 초기화 (혹은 재생성)
        foreach (Transform child in itemListContainer)
        {
            Destroy(child.gameObject);
        }

        // 모든 TradableItem에 대해 UI 프리팹 인스턴스 생성
        foreach (TradableItem item in tradeDataLoader.tradableItems)
        {
            GameObject obj = Instantiate(tradeItemPrefab, itemListContainer);
            TradeItemUI itemUI = obj.GetComponent<TradeItemUI>();
            if (itemUI != null)
            {
                // TradeItemUI.Setup() 메서드에서 아이템 데이터, TradeManager, TradeUI(부모)를 전달하여 초기화를 진행합니다.
                itemUI.Setup(item, tradeManager, this);
            }
            else
            {
                Debug.LogWarning("TradeItemUI 컴포넌트가 프리팹에 없습니다.");
            }
        }
    }

    /// <summary>
    /// 플레이어의 현재 재화(COMA) 값을 UI 텍스트에 업데이트합니다.
    /// </summary>
    public void UpdatePlayerCOMA()
    {
        if (playerCOMAText != null && tradeManager != null)
        {
            playerCOMAText.text = "COMA: " + tradeManager.GetPlayerCOMA();
        }
    }
}
