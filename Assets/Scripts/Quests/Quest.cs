using UnityEngine;

public enum QuestType
{
    PirateHunt,
    ItemDelivery,
    ItemAcquisition
}

[System.Serializable]
public class Quest
{
    public string questName;
    public string description;
    public QuestType type;

    public int targetCount; // 목표 개수
    public int currentCount; // 현재 진행도

    public TradableItem requiredItem; // 필요한 아이템
    public Planet destinationPlanet; // 목적지 행성

    public int rewardScrap; // 보상 스크랩

    // 퀘스트 진행
    public void UpdateProgress(int count = 1)
    {
        currentCount += count;
        if (currentCount > targetCount) currentCount = targetCount;
    }

    // 퀘스트 완료 여부
    public bool IsCompleted()
    {
        return currentCount >= targetCount;
    }

    // 퀘스트 정보 문자열
    public string GetQuestInfo()
    {
        string info = $"{questName}\n";
        info += $"{description}\n";
        info += $"Progress: {currentCount}/{targetCount}\n";
        info += $"Reward: {rewardScrap} Scrap";

        return info;
    }
}
