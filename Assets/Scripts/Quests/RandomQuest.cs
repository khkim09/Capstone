using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuestDefinition", menuName = "Game/Quest Definition")]
public class RandomQuest : ScriptableObject
{
    [Header("식별 및 상태")]
    public string questId;
    [HideInInspector] public QuestStatus status = QuestStatus.NotStarted;

    [Header("수락한 행성 ID")]
    public string acceptingPlanetId;

    [Header("기본 정보")]
    public string title;
    [TextArea] public string description;
    public Sprite questIcon;

    [Header("조건")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("보상")]
    public List<QuestReward> rewards = new List<QuestReward>();

    public enum QuestStatus
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }

    public enum QuestObjectiveType
    {
        PirateHunt,
        ItemTransport,
        ItemProcurement,
        CrewTransport
    }

    [Serializable]
    public class QuestObjective
    {
        public QuestObjectiveType objectiveType;
        public string targetId;
        public string destinationPlanetId;
        public string description;
        public int requiredAmount;
        public int killCount;

        [HideInInspector] public int currentAmount;
        [HideInInspector] public bool isCompleted;
    }

    [Serializable]
    public class QuestReward
    {
        public int amount; // 항상 COMA 고정
    }

    public void Accept()
    {
        if (status != QuestStatus.NotStarted) return;
        status = QuestStatus.Active;
    }

    public void Decline()
    {
        if (status != QuestStatus.NotStarted) return;
        status = QuestStatus.Failed;
    }

    public void CheckCompletion()
    {
        if (status != QuestStatus.Active) return;

        foreach (var o in objectives)
            if (!o.isCompleted) return;

        status = QuestStatus.Completed;
    }

    public QuestManager.Quest ToQuest()
    {
        var q = new QuestManager.Quest
        {
            id = questId,
            title = title,
            description = description,
            status = QuestManager.QuestStatus.Active
        };

        foreach (var o in objectives)
        {
            // 🔧 여기 수정
            int required = o.objectiveType == QuestObjectiveType.CrewTransport || o.objectiveType == QuestObjectiveType.PirateHunt
                ? o.killCount
                : o.requiredAmount;

            q.objectives.Add(new QuestManager.QuestObjective
            {
                objectiveType = o.objectiveType,
                description = o.description,
                currentAmount = 0,
                requiredAmount = required,
                destinationPlanetId = o.destinationPlanetId,
                isCompleted = false
            });
        }

        foreach (var r in rewards)
        {
            q.rewards.Add(new QuestManager.QuestReward
            {
                type = QuestManager.RewardType.Resource,
                resourceType = ResourceType.COMA,
                amount = r.amount
            });
        }

        return q;
    }

}
