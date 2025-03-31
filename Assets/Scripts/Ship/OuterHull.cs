using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 외갑판 업그레이드를 나타내는 클래스.
/// 방처럼 배치되지는 않지만, 전체 함선 스탯에 영향을 주는 구성 요소입니다.
/// </summary>
public class OuterHull : MonoBehaviour, IShipStatContributor
{
    /// <summary>
    /// 외갑판 데이터 (레벨별 스탯 포함).
    /// </summary>
    [SerializeField] private OuterHullData outerHullData;

    /// <summary>
    /// 현재 외갑판의 레벨입니다.
    /// </summary>
    [SerializeField] private int currentLevel = 1;

    /// <summary>
    /// 현재 레벨에 해당하는 외갑판 상세 데이터입니다.
    /// </summary>
    private OuterHullData.OuterHullLevel currentLevelData;

    /// <summary>
    /// 컴포넌트가 활성화될 때 외갑판을 초기화합니다.
    /// </summary>
    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// 외갑판 레벨 데이터를 초기화합니다.
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
    /// 외갑판이 함선에 기여하는 스탯을 반환합니다.
    /// 현재는 피해 감소(DamageReduction) 스탯만 제공됩니다.
    /// </summary>
    /// <returns>스탯 종류와 수치가 담긴 딕셔너리.</returns>
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
    /// 외갑판을 한 단계 업그레이드합니다.
    /// 다음 레벨의 데이터가 존재하면 업그레이드에 성공합니다.
    /// </summary>
    /// <returns>업그레이드 성공 시 true, 실패 시 false.</returns>
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

    /// <summary>
    /// 현재 외갑판의 레벨을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨 값.</returns>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
}
