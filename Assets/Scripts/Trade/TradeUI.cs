using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class TradeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform itemListContainer; // Scroll View Content (왼쪽 목록)
    [SerializeField] private GameObject tradeItemPrefab;    // TradeItemUI 프리팹 (개별 아이템 슬롯, 여기서는 InventoryItemUI와 다름)
    [SerializeField] private TMP_Text playerCOMAText;         // 플레이어 재화 표시

    [Header("System References")]
    [SerializeField] private TradeManager tradeManager;
    [SerializeField] private TradeDataLoader tradeDataLoader;

    private void Start()
    {
        if (tradeManager == null)
            tradeManager = FindObjectOfType<TradeManager>();
        if (tradeDataLoader == null)
            tradeDataLoader = FindObjectOfType<TradeDataLoader>();

        PopulateTradeItemList();
        UpdatePlayerCOMA();
    }

    private void PopulateTradeItemList()
    {
        foreach (Transform child in itemListContainer)
            Destroy(child.gameObject);

        foreach (TradableItem item in tradeDataLoader.tradableItems)
        {
            GameObject obj = Instantiate(tradeItemPrefab, itemListContainer);
            TradeItemUI itemUI = obj.GetComponent<TradeItemUI>();
            if (itemUI != null)
                itemUI.Setup(item, tradeManager, this);
            else
                Debug.LogWarning("TradeItemUI 컴포넌트가 프리팹에 없습니다.");
        }
    }

    public void UpdatePlayerCOMA()
    {
        if (playerCOMAText != null && tradeManager != null)
            playerCOMAText.text = "COMA: " + tradeManager.GetPlayerCOMA();
    }
}
