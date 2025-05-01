using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트의 등록, 진행도 갱신, 완료 처리 등을 담당하는 퀘스트 매니저.
/// Singleton 방식으로 동작하며 퀘스트 변경 이벤트도 제공합니다.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public delegate void QuestChangedHandler(Quest quest);

    public enum QuestStatus
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }

    public enum RewardType
    {
        Resource,
        Item,
        Crew
    }

    [SerializeField] private List<Quest> activeQuests = new();
    [SerializeField] private List<Quest> completedQuests = new();

    public static QuestManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public event QuestChangedHandler OnQuestAdded;
    public event QuestChangedHandler OnQuestUpdated;
    public event QuestChangedHandler OnQuestCompleted;

    public void AddQuest(Quest quest)
    {
        activeQuests.Add(quest);
        OnQuestAdded?.Invoke(quest);
        Debug.Log($"New quest added: {quest.title}");
    }

    public void TriggerQuestCompleted(Quest quest)
    {
        OnQuestCompleted?.Invoke(quest);
    }

    public void UpdateQuestObjective(string questId, int objectiveIndex, int amount)
    {
        Quest quest = activeQuests.Find(q => q.id == questId);
        if (quest != null && objectiveIndex >= 0 && objectiveIndex < quest.objectives.Count)
        {
            QuestObjective objective = quest.objectives[objectiveIndex];
            objective.currentAmount += amount;
            objective.isCompleted = objective.currentAmount >= objective.requiredAmount;

            OnQuestUpdated?.Invoke(quest);

            bool allCompleted = true;
            foreach (QuestObjective obj in quest.objectives)
                if (!obj.isCompleted)
                {
                    allCompleted = false;
                    break;
                }

            if (allCompleted) CompleteQuest(quest);
        }
    }

    private void CompleteQuest(Quest quest)
    {
        quest.status = QuestStatus.Completed;
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        foreach (QuestReward reward in quest.rewards)
        {
            if (reward.type == RewardType.Resource && reward.resourceType == ResourceType.COMA)
            {
                ResourceManager.Instance.ChangeResource(ResourceType.COMA, reward.amount);
            }
        }

        OnQuestCompleted?.Invoke(quest);
        Debug.Log($"Quest completed: {quest.title}");
    }

    public List<Quest> GetActiveQuests() => activeQuests;
    public List<Quest> GetCompletedQuests() => completedQuests;

    [Serializable]
    public class Quest
    {
        public string id;
        public string title;
        public string description;
        public QuestStatus status = QuestStatus.Active;
        public List<QuestObjective> objectives = new();
        public List<QuestReward> rewards = new();
    }

    [Serializable]
    public class QuestObjective
    {
        public RandomQuest.QuestObjectiveType objectiveType;
        public string description;
        public int currentAmount;
        public int requiredAmount;
        public string destinationPlanetId;
        public bool isCompleted;
    }

    [Serializable]
    public class QuestReward
    {
        public RewardType type;
        public ResourceType resourceType;
        public int amount;
    }
}
