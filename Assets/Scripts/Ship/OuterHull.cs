using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 외갑판 업그레이드를 나타내는 클래스
/// 방처럼 배치되지 않고 전체 스탯에 영향을 주는 구성 요소
/// </summary>
public class OuterHull : MonoBehaviour, IShipStatContributor
{
    [SerializeField] private OuterHullData outerHullData;
    [SerializeField] private int currentLevel = 1;

    private OuterHullData.OuterHullLevel currentLevelData;

    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// 외갑판 레벨 데이터 초기화
    /// </summary>
    public void Initialize()
    {
        currentLevel = 1;
        if (outerHullData == null)
        {
            Debug.LogError("OuterHullData is not assigned.");
            return;
        }

        currentLevelData = outerHullData.GetOuterHullData(currentLevel);
        if (currentLevelData == null)
            Debug.LogWarning($"No outer hull data found for level {currentLevel}");
    }

    /// <summary>
    /// 외갑판이 함선에 기여하는 스탯 반환
    /// </summary>
    public Dictionary<ShipStat, float> GetStatContributions()
    {
        Dictionary<ShipStat, float> contributions = new();

        if (currentLevelData == null)
        {
            Debug.LogWarning("OuterHullLevel data is null. Returning empty stat contribution.");
            return contributions;
        }

        // 외갑판은 피해 감소 스탯만 기여
        contributions[ShipStat.DamageReduction] = currentLevelData.damageReduction;

        return contributions;
    }

    /// <summary>
    /// 외갑판 레벨 업그레이드
    /// </summary>
    public bool Upgrade()
    {
        int nextLevel = currentLevel + 1;
        OuterHullData.OuterHullLevel nextData = outerHullData.GetOuterHullData(nextLevel);

        if (nextData == null)
            return false;

        currentLevel = nextLevel;
        currentLevelData = nextData;

        return true;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }
}
