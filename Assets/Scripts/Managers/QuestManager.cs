using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    // 퀘스트 변경 이벤트
    public delegate void QuestChangedHandler(Quest quest);

    public enum QuestStatus
    {
        Active,
        Completed,
        Failed
    }

    public enum RewardType
    {
        Resource,
        Item,
        Crew,
        ShipPart
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

    public void UpdateQuestObjective(string questId, int objectiveIndex, int amount)
    {
        var quest = activeQuests.Find(q => q.id == questId);
        if (quest != null && objectiveIndex >= 0 && objectiveIndex < quest.objectives.Count)
        {
            var objective = quest.objectives[objectiveIndex];
            objective.currentAmount += amount;
            objective.isCompleted = objective.currentAmount >= objective.requiredAmount;

            OnQuestUpdated?.Invoke(quest);

            // 모든 목표가 완료되었는지 확인
            var allCompleted = true;
            foreach (var obj in quest.objectives)
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

        // 보상 지급
        foreach (var reward in quest.rewards)
            switch (reward.type)
            {
                case RewardType.Resource:
                    ResourceManager.Instance.ChangeResource(reward.resourceType, reward.amount);
                    break;

                case RewardType.Item:
                    if (InventoryManager.Instance != null) InventoryManager.Instance.AddItem(reward.itemId);
                    break;

                case RewardType.Crew:
                    // 새 승무원 추가 로직
                    break;

                case RewardType.ShipPart:
                    // 선박 부품 추가 로직
                    break;
            }

        OnQuestCompleted?.Invoke(quest);
        Debug.Log($"Quest completed: {quest.title}");
    }

    public List<Quest> GetActiveQuests()
    {
        return activeQuests;
    }

    public List<Quest> GetCompletedQuests()
    {
        return completedQuests;
    }

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
        public string description;
        public int currentAmount;
        public int requiredAmount;
        public bool isCompleted;
    }

    [Serializable]
    public class QuestReward
    {
        public RewardType type;
        public ResourceType resourceType;
        public int amount;
        public string itemId;
    }
}
