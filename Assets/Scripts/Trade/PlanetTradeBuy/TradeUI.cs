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
    }

    /// <summary>
    /// TradeDataLoader에서 로드한 아이템 데이터 중, 현재 행성 데이터에 해당하는 아이템만 필터링하여
    /// PlanetItemUI 프리팹을 동적으로 생성, 아이템 목록 컨테이너에 추가합니다.
    /// </summary>
    private void PopulateTradeItemList()
    {
        // 기존 자식 오브젝트 제거
        foreach (Transform child in itemListContainer)
            Destroy(child.gameObject);

        // 현재 행성 코드를 TradeManager에서 가져온다고 가정
        string currentPlanetCode = tradeManager != null && tradeManager.CurrentPlanetTradeData != null
            ? tradeManager.CurrentPlanetTradeData.planetCode
            : "SIS"; // 기본값

        foreach (TradableItem item in tradeDataLoader.tradableItems)
        {
            // 아이템의 행성 코드가 현재 행성과 일치하는 경우만 생성
            if (item.planet.Equals(currentPlanetCode))
            {
                GameObject obj = Instantiate(tradeItemPrefab, itemListContainer);
                PlanetItemUI itemUI = obj.GetComponent<PlanetItemUI>();
                if (itemUI != null)
                    itemUI.Setup(item);
                else
                    Debug.LogWarning("PlanetItemUI 컴포넌트가 프리팹에 없습니다.");
            }
        }
    }
    /// <summary>
    /// 선택한 행성의 판매 데이터를 기반으로 상점 UI를 갱신합니다.
    /// </summary>
    /// <param name="data">선택한 행성의 판매 물품 데이터</param>
    public void PopulateStore(PlanetTradeData data)
    {
        // 기존 상점 UI 항목들을 제거하고, data.items 목록을 기반으로 새로운 UI 항목(PlanetItemUI)을 Instantiate 합니다.
        // 예: foreach (TradableItem item in data.items) { Instantiate(planetItemPrefab, contentPanel).Setup(item); }
        Debug.Log("TradeUI: 상점 UI가 갱신되었습니다. 행성: " + data.planetCode);
    }

}
