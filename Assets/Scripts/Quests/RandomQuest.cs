using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 랜덤 퀘스트 정보 관련 클래스
/// </summary>
[CreateAssetMenu(fileName = "NewRandomQuest", menuName = "Quest/Random Quest")]
public class RandomQuest : ScriptableObject
{
    public string title;
    public string description;
    public QuestStatus status = QuestStatus.Active;
    public List<QuestObjective> objectives = new List<QuestObjective>();
    public List<QuestReward> rewards = new List<QuestReward>();

    /// <summary>
    /// 퀘스트 상태
    /// </summary>
    public enum QuestStatus
    {
        Active,
        Completed,
        Failed
    }

    /// <summary>
    /// 퀘스트 설명, 현재 횟수, 필요 횟수, 완료 여부
    /// </summary>
    [System.Serializable]
    public class QuestObjective
    {
        public string description;
        public int currentAmount;
        public int requiredAmount;
        public bool isCompleted;
    }

    /// <summary>
    /// 퀘스트 보상 종류, 양, 아이템 종류
    /// </summary>
    [System.Serializable]
    public class QuestReward
    {
        public RewardType type;
        public int amount;
        public string itemId;
        // 필요에 따라 ResourceType 등 추가할 수 있습니다.
    }

    /// <summary>
    ///  보상의 종류
    /// </summary>
    public enum RewardType
    {
        Resource,
        Item,
        Crew,
        ShipPart
    }

    /// <summary>
    /// 퀘스트 형 변환 함수
    /// </summary>
    /// <returns></returns>
    public QuestManager.Quest ToQuest()
    {
        var quest = new QuestManager.Quest();
        // 적절한 식별자 할당 (예: this.name 사용)
        quest.id = this.name;
        quest.title = this.title;
        quest.description = this.description;
        // 상태는 기본적으로 Active로 설정 (필요 시 다른 로직 추가)
        quest.status = QuestManager.QuestStatus.Active;

        // 목표(objectives) 변환
        quest.objectives = new List<QuestManager.QuestObjective>();
        foreach (var obj in this.objectives)
        {
            quest.objectives.Add(new QuestManager.QuestObjective
            {
                description = obj.description,
                currentAmount = obj.currentAmount,
                requiredAmount = obj.requiredAmount,
                isCompleted = obj.isCompleted
            });
        }

        // 보상(rewards) 변환
        quest.rewards = new List<QuestManager.QuestReward>();
        foreach (var rew in this.rewards)
        {
            quest.rewards.Add(new QuestManager.QuestReward
            {
                // RewardType 변환 시 Enum.Parse를 사용하거나, 직접 매핑할 수 있음
                type = (QuestManager.RewardType)Enum.Parse(typeof(QuestManager.RewardType), rew.type.ToString()),
                amount = rew.amount,
                itemId = rew.itemId,
                // 만약 ResourceType도 있다면 추가 처리 필요
            });
        }

        return quest;
    }
}
