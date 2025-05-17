using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    [SerializeField] private TradingItemDataBase itemDatabase;

    /// <summary>
    /// 플레이어의 재화(Coma)를 표시하는 텍스트 UI 요소입니다.
    /// </summary>
    [SerializeField] private TMP_Text playerComaText;

    #endregion

    #region System References

    /// <summary>
    /// 거래 관리 기능을 수행하는 TradeManager의 참조입니다.
    /// </summary>
    [Header("System References")]
    [SerializeField] private TradeManager tradeManager;

    #endregion

    /// <summary>
    /// Start 시 TradeManager와 TradeDataLoader를 찾고,
    /// 무역 아이템 목록을 초기화합니다.
    /// </summary>
    private void Start()
    {
        if (tradeManager == null)
            tradeManager = Object.FindFirstObjectByType<TradeManager>();

        PopulateTradeItemList();
    }

    /// <summary>
    /// TradeDataLoader에서 로드된 전체 아이템 중 현재 행성과 일치하는 아이템만 UI로 표시합니다.
    /// </summary>
    private void PopulateTradeItemList()
    {
        foreach (Transform child in itemListContainer)
            Destroy(child.gameObject);

        string currentPlanetCode = tradeManager != null && tradeManager.CurrentPlanetTradeData != null
            ? tradeManager.CurrentPlanetTradeData.planetCode
            : "SIS"; // 기본값

        foreach (TradingItemData item in itemDatabase.allItems)
        {
            if (item.planet.ToString() == currentPlanetCode)
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
    /// 선택한 PlanetTradeData를 기반으로 상점 UI를 갱신합니다.
    /// 현재는 디버그 출력만 수행합니다.
    /// </summary>
    /// <param name="data">선택한 행성의 무역 아이템 데이터</param>
    public void PopulateStore(PlanetTradeData data)
    {
        Debug.Log("TradeUI: 상점 UI가 갱신되었습니다. 행성: " + data.planetCode);
    }

    /// <summary>
    /// 패널을 닫는 함수입니다.
    /// </summary>
    public void CloseSelfPanel()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
