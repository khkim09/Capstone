using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>
/// TradeUI는 무역 시스템의 UI를 관리하는 클래스입니다.
/// 아이템 목록을 표시하고, 플레이어의 현재 재화(COMA)를 갱신하는 기능을 제공합니다.
/// </summary>
public class TradeUI : MonoBehaviour
{
    #region UI References

    /// <summary>
    /// 아이템 목록 컨테이너입니다.
    /// Scroll View의 Content로 사용되며, TradeItemUI 프리팹이 동적으로 추가됩니다.
    /// </summary>
    [Header("UI References")]
    [SerializeField] private Transform itemListContainer;

    /// <summary>
    /// TradeItemUI 프리팹입니다.
    /// 개별 아이템 슬롯을 생성하는 데 사용됩니다.
    /// </summary>
    [SerializeField] private GameObject tradeItemPrefab;

    /// <summary>
    /// 플레이어의 현재 재화(COMA)를 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TMP_Text playerCOMAText;

    #endregion

    #region System References

    /// <summary>
    /// 거래 관리 기능을 수행하는 TradeManager의 참조입니다.
    /// </summary>
    [Header("System References")]
    [SerializeField] private TradeManager tradeManager;

    /// <summary>
    /// 거래 가능한 아이템 데이터를 로드하는 TradeDataLoader의 참조입니다.
    /// </summary>
    [SerializeField] private TradeDataLoader tradeDataLoader;

    #endregion

    /// <summary>
    /// MonoBehaviour의 Start 메서드입니다.
    /// TradeManager와 TradeDataLoader가 할당되어 있지 않은 경우 씬에서 찾아 할당하고,
    /// 아이템 목록을 초기화하며 플레이어의 재화를 갱신합니다.
    /// </summary>
    private void Start()
    {
        if (tradeManager == null)
            tradeManager = FindObjectOfType<TradeManager>();
        if (tradeDataLoader == null)
            tradeDataLoader = FindObjectOfType<TradeDataLoader>();

        PopulateTradeItemList();
        UpdatePlayerCOMA();
    }

    /// <summary>
    /// TradeDataLoader에서 로드한 아이템 데이터를 기반으로 TradeItemUI를 동적으로 생성하여
    /// 아이템 목록 컨테이너에 추가합니다.
    /// 기존에 존재하는 자식 오브젝트들은 모두 제거됩니다.
    /// </summary>
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

    /// <summary>
    /// 플레이어의 현재 COMA(재화)를 TradeManager에서 가져와 UI 텍스트에 업데이트합니다.
    /// </summary>
    public void UpdatePlayerCOMA()
    {
        if (playerCOMAText != null && tradeManager != null)
            playerCOMAText.text = "COMA: " + tradeManager.GetPlayerCOMA();
    }
}
