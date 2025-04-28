using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트의 등록, 진행도 갱신, 완료 처리 등을 담당하는 퀘스트 매니저.
/// Singleton 방식으로 동작하며 퀘스트 변경 이벤트도 제공합니다.
/// </summary>
public class QuestManager : MonoBehaviour
{
    /// <summary>
    /// 퀘스트 변경 관련 델리게이트입니다.
    /// </summary>
    public delegate void QuestChangedHandler(Quest quest);

    /// <summary>
    /// 퀘스트 상태를 나타내는 열거형입니다.
    /// </summary>
    public enum QuestStatus
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }

    /// <summary>
    /// 퀘스트 보상 타입을 정의하는 열거형입니다.
    /// </summary>
    public enum RewardType
    {
        Resource,
        Item,
        Crew,
        ShipPart
    }

    /// <summary>
    /// 현재 진행 중인 퀘스트 리스트입니다.
    /// </summary>
    [SerializeField] private List<Quest> activeQuests = new();

    /// <summary>
    /// 완료된 퀘스트 리스트입니다.
    /// </summary>
    [SerializeField] private List<Quest> completedQuests = new();

    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static QuestManager Instance { get; private set; }

    /// <summary>
    /// 인스턴스를 초기화하고 싱글턴을 설정합니다.
    /// </summary>
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


    /// <summary>
    /// 퀘스트 추가 시 호출되는 이벤트입니다.
    /// </summary>
    public event QuestChangedHandler OnQuestAdded;

    /// <summary>
    /// 퀘스트 목표가 갱신될 때 호출되는 이벤트입니다.
    /// </summary>
    public event QuestChangedHandler OnQuestUpdated;

    /// <summary>
    /// 퀘스트 완료 시 호출되는 이벤트입니다.
    /// </summary>
    public event QuestChangedHandler OnQuestCompleted;

    /// <summary>
    /// 새로운 퀘스트를 추가하고 이벤트를 발생시킵니다.
    /// </summary>
    /// <param name="quest">추가할 퀘스트.</param>
    public void AddQuest(Quest quest)
    {
        activeQuests.Add(quest);
        OnQuestAdded?.Invoke(quest);

        Debug.Log($"New quest added: {quest.title}");
    }

    /// <summary>
    /// 외부에서 퀘스트 완료 처리를 트리거할 때 사용됩니다.
    /// </summary>
    /// <param name="quest">완료된 퀘스트.</param>
    public void TriggerQuestCompleted(Quest quest)
    {
        OnQuestCompleted?.Invoke(quest);
    }

    /// <summary>
    /// 퀘스트의 특정 목표 진행도를 업데이트하고, 완료 여부를 검사합니다.
    /// </summary>
    /// <param name="questId">퀘스트 ID.</param>
    /// <param name="objectiveIndex">목표 인덱스.</param>
    /// <param name="amount">추가된 진행도.</param>
    public void UpdateQuestObjective(string questId, int objectiveIndex, int amount)
    {
        Quest quest = activeQuests.Find(q => q.id == questId);
        if (quest != null && objectiveIndex >= 0 && objectiveIndex < quest.objectives.Count)
        {
            QuestObjective objective = quest.objectives[objectiveIndex];
            objective.currentAmount += amount;
            objective.isCompleted = objective.currentAmount >= objective.requiredAmount;

            OnQuestUpdated?.Invoke(quest);

            // 모든 목표가 완료되었는지 확인
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

    /// <summary>
    /// 퀘스트를 완료 처리하고 보상을 지급합니다.
    /// </summary>
    /// <param name="quest">완료된 퀘스트.</param>
    private void CompleteQuest(Quest quest)
    {
        quest.status = QuestStatus.Completed;
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        // 보상 지급
        foreach (QuestReward reward in quest.rewards)
            switch (reward.type)
            {
                case RewardType.Resource:
                    ResourceManager.Instance.ChangeResource(reward.resourceType, reward.amount);
                    break;

                case RewardType.Item:
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

    /// <summary>
    /// 현재 진행 중인 퀘스트 목록을 반환합니다.
    /// </summary>
    public List<Quest> GetActiveQuests()
    {
        return activeQuests;
    }

    /// <summary>
    /// 완료된 퀘스트 목록을 반환합니다.
    /// </summary>
    public List<Quest> GetCompletedQuests()
    {
        return completedQuests;
    }


    /// <summary>
    /// 개별 퀘스트의 정보입니다.
    /// ID, 제목, 설명, 상태, 목표, 보상 정보를 포함합니다.
    /// </summary>
    [Serializable]
    public class Quest
    {
        /// <summary>
        /// 퀘스트 고유 ID입니다.
        /// </summary>
        public string id;

        /// <summary>
        /// 퀘스트 제목입니다.
        /// </summary>
        public string title;

        /// <summary>
        /// 퀘스트 설명입니다.
        /// </summary>
        public string description;

        /// <summary>
        /// 퀘스트의 현재 상태입니다. (진행 중, 완료, 실패 등)
        /// </summary>
        public QuestStatus status = QuestStatus.Active;

        /// <summary>
        /// 퀘스트의 목표 리스트입니다.
        /// </summary>
        public List<QuestObjective> objectives = new();

        /// <summary>
        /// 퀘스트 완료 시 지급될 보상 리스트입니다.
        /// </summary>
        public List<QuestReward> rewards = new();
    }

    /// <summary>
    /// 퀘스트의 개별 목표입니다.
    /// 현재 진행량, 필요량, 완료 여부를 포함합니다.
    /// </summary>
    [Serializable]
    public class QuestObjective
    {
        /// <summary>
        /// 목표 설명입니다.
        /// </summary>
        public string description;

        /// <summary>
        /// 현재까지 달성한 수치입니다.
        /// </summary>
        public int currentAmount;

        /// <summary>
        /// 목표 완료를 위한 필요 수치입니다.
        /// </summary>
        public int requiredAmount;

        /// <summary>
        /// 해당 목표가 완료되었는지 여부입니다.
        /// </summary>
        public bool isCompleted;
    }

    /// <summary>
    /// 퀘스트 완료 시 지급되는 보상입니다.
    /// 리소스, 아이템, 승무원, 선박 부품 등 다양하게 설정 가능합니다.
    /// </summary>
    [Serializable]
    public class QuestReward
    {
        /// <summary>
        /// 보상의 종류입니다. (자원, 아이템, 승무원, 부품 등)
        /// </summary>
        public RewardType type;

        /// <summary>
        /// 자원 보상의 경우 적용될 자원 타입입니다.
        /// (type이 Resource일 때만 유효)
        /// </summary>
        public ResourceType resourceType;

        /// <summary>
        /// 자원 또는 기타 수치형 보상의 양입니다.
        /// </summary>
        public int amount;

        /// <summary>
        /// 아이템 보상의 경우 해당 아이템의 ID입니다.
        /// </summary>
        public string itemId;
    }
}
