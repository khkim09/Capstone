using UnityEngine;

/// <summary>
/// 퀘스트의 유형을 정의합니다.
/// </summary>
public enum QuestType
{
    PirateHunt,
    ItemDelivery,
    ItemAcquisition
}

/// <summary>
/// Quest는 게임 내에서 수행 가능한 퀘스트 정보를 나타냅니다.
/// </summary>
[System.Serializable]
public class Quest
{
    /// <summary>
    /// 퀘스트의 제목입니다.
    /// </summary>
    public string questName;

    /// <summary>
    /// 퀘스트에 대한 설명입니다.
    /// </summary>
    public string description;

    /// <summary>
    /// 퀘스트의 종류입니다.
    /// </summary>
    public QuestType type;

    /// <summary>
    /// 퀘스트 목표 개수입니다. (예: 처치 수, 수집 수)
    /// </summary>
    public int targetCount;

    /// <summary>
    /// 현재까지 달성한 수입니다.
    /// </summary>
    public int currentCount;

    /// <summary>
    /// 퀘스트에 필요한 아이템입니다. (기존 TradableItem → TradingItemData)
    /// </summary>
    public TradingItemData requiredItem;

    /// <summary>
    /// 퀘스트의 목적지 행성입니다.
    /// </summary>
    public Planet destinationPlanet;

    /// <summary>
    /// 퀘스트 완료 시 보상으로 지급되는 스크랩 수치입니다.
    /// </summary>
    public int rewardScrap;

    /// <summary>
    /// 퀘스트 진행 상황을 업데이트합니다.
    /// </summary>
    /// <param name="count">증가시킬 수치 (기본값: 1)</param>
    public void UpdateProgress(int count = 1)
    {
        currentCount += count;
        if (currentCount > targetCount) currentCount = targetCount;
    }

    /// <summary>
    /// 퀘스트 완료 여부를 반환합니다.
    /// </summary>
    /// <returns>목표를 달성했으면 true, 그렇지 않으면 false</returns>
    public bool IsCompleted()
    {
        return currentCount >= targetCount;
    }

    /// <summary>
    /// 퀘스트 정보를 문자열로 반환합니다.
    /// </summary>
    /// <returns>퀘스트 정보 문자열</returns>
    public string GetQuestInfo()
    {
        string info = $"{questName}\n";
        info += $"{description}\n";
        info += $"Progress: {currentCount}/{targetCount}\n";
        info += $"Reward: {rewardScrap} Scrap";

        return info;
    }
}
