using UnityEngine;

/// <summary>
/// RandomPlanetArrival은 게임 시작 시 Resources 폴더 내의 "ScriptableObject/Trade/PlanetInfo" 경로에서
/// 모든 PlanetTradeData 에셋을 로드하고, 무작위로 하나의 행성 데이터를 선택하여 TradeManager와 TradeUI에 반영합니다.
/// 이를 통해 플레이어가 해당 행성에 도착한 것으로 설정하여, 그 행성에서 판매되는 물품들을 무역 UI에 표시할 수 있습니다.
/// </summary>
public class RandomPlanetArrival : MonoBehaviour
{
    /// <summary>
    /// Resources 폴더 내의 PlanetTradeData 에셋이 있는 경로입니다.
    /// (예: Assets/Resources/ScriptableObject/Trade/PlanetInfo)
    /// </summary>
    [Tooltip("Resources 폴더 내부의 PlanetTradeData 에셋 경로 (예: ScriptableObject/Trade/PlanetInfo)")]
    [SerializeField] private string planetDataPath = "ScriptableObject/Trade/PlanetInfo";

    /// <summary>
    /// 현재 행성의 판매 데이터를 설정할 TradeManager의 참조입니다.
    /// </summary>
    [Tooltip("현재 행성 판매 데이터를 설정할 TradeManager 참조")]
    [SerializeField] private TradeManager tradeManager;

    /// <summary>
    /// 현재 행성의 판매 데이터를 표시할 TradeUI의 참조입니다.
    /// </summary>
    [Tooltip("현재 행성 판매 데이터를 표시할 TradeUI 참조")]
    [SerializeField] private TradeUI tradeUI;

    /// <summary>
    /// 게임 시작 시 호출되어, Resources.LoadAll을 통해 지정된 경로에서 PlanetTradeData 에셋들을 불러온 후,
    /// 무작위로 한 행성을 선택하여 TradeManager와 TradeUI에 설정합니다.
    /// </summary>
    private void Start()
    {
        // SIS 에셋을 직접 로드합니다.
        PlanetTradeData sisData = Resources.Load<PlanetTradeData>("ScriptableObject/Trade/PlanetInfo/SIS");
        if (sisData != null)
        {
            Debug.Log("SIS 행성 데이터가 성공적으로 로드되었습니다: " + sisData.planetCode);
            // TradeManager나 TradeUI에 SIS 데이터를 설정하는 로직 추가
            if (tradeManager != null)
            {
                tradeManager.SetCurrentPlanetTradeData(sisData);
            }
            if (tradeUI != null)
            {
                tradeUI.PopulateStore(sisData);
            }
        }
        else
        {
            Debug.LogError("SIS 에셋을 로드하지 못했습니다.");
        }
    }

}
