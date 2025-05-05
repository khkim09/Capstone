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
    /// 퀘스트 변경 이벤트 핸들러
    /// </summary>
    public delegate void QuestChangedHandler(Quest quest);

    /// <summary>
    /// 퀘스트의 상태를 나타냅니다.
    /// </summary>
    public enum QuestStatus
    {
        /// <summary>퀘스트를 아직 시작하지 않음</summary>
        NotStarted,
        /// <summary>퀘스트 진행 중</summary>
        Active,
        /// <summary>퀘스트 완료</summary>
        Completed,
        /// <summary>퀘스트 실패</summary>
        Failed
    }

    /// <summary>
    /// 보상 유형을 정의합니다.
    /// </summary>
    public enum RewardType
    {
        /// <summary>자원 (예: COMA)</summary>
        Resource,
        /// <summary>아이템 보상</summary>
        Item,
        /// <summary>크루 보상</summary>
        Crew
    }

    [SerializeField] private List<Quest> activeQuests = new();
    [SerializeField] private List<Quest> completedQuests = new();

    /// <summary>
    /// 퀘스트 매니저의 싱글톤 인스턴스
    /// </summary>
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

    /// <summary>퀘스트가 새로 추가될 때 발생하는 이벤트</summary>
    public event QuestChangedHandler OnQuestAdded;

    /// <summary>퀘스트가 갱신될 때 발생하는 이벤트</summary>
    public event QuestChangedHandler OnQuestUpdated;

    /// <summary>퀘스트가 완료될 때 발생하는 이벤트</summary>
    public event QuestChangedHandler OnQuestCompleted;

    /// <summary>
    /// 새로운 퀘스트를 추가합니다.
    /// </summary>
    /// <param name="quest">추가할 퀘스트</param>
    public void AddQuest(Quest quest)
    {
        activeQuests.Add(quest);
        OnQuestAdded?.Invoke(quest);
        Debug.Log($"New quest added: {quest.title}");
    }

    /// <summary>
    /// 외부에서 퀘스트 완료 처리를 강제로 호출할 때 사용합니다.
    /// </summary>
    /// <param name="quest">완료된 퀘스트</param>
    public void TriggerQuestCompleted(Quest quest)
    {
        OnQuestCompleted?.Invoke(quest);
    }

    /// <summary>
    /// 특정 퀘스트 목표의 진행도를 갱신합니다.
    /// </summary>
    /// <param name="questId">퀘스트 ID</param>
    /// <param name="objectiveIndex">목표 인덱스</param>
    /// <param name="amount">추가할 수치</param>
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

    /// <summary>
    /// 퀘스트를 완료 처리합니다.
    /// </summary>
    /// <param name="quest">완료된 퀘스트</param>
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
        QuestUIManager ui = FindObjectOfType<QuestUIManager>();
        if (ui != null)
        {
            ui.ShowCompletion(quest); // ScriptableObject가 아니라 Quest 인스턴스 넘김
        }

        OnQuestCompleted?.Invoke(quest);
        Debug.Log($"Quest completed: {quest.title}");
    }

    /// <summary>
    /// 현재 진행 중인 퀘스트 목록을 반환합니다.
    /// </summary>
    /// <returns>진행 중인 퀘스트 리스트</returns>
    public List<Quest> GetActiveQuests() => activeQuests;

    /// <summary>
    /// 완료된 퀘스트 목록을 반환합니다.
    /// </summary>
    /// <returns>완료된 퀘스트 리스트</returns>
    public List<Quest> GetCompletedQuests() => completedQuests;

    /// <summary>
    /// 퀘스트 정보 클래스
    /// </summary>
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

    /// <summary>
    /// 퀘스트 목표 하나를 정의하는 클래스
    /// </summary>
    [Serializable]
    public class QuestObjective
    {
        /// <summary>목표 타입</summary>
        public RandomQuest.QuestObjectiveType objectiveType;
        /// <summary>설명 텍스트</summary>
        public string description;
        /// <summary>현재 달성 수치</summary>
        public int currentAmount;
        /// <summary>필요 달성 수치</summary>
        public int requiredAmount;
        /// <summary>목표 도달 행성 ID (해당 시만)</summary>
        public string destinationPlanetId;
        /// <summary>달성 여부</summary>
        public bool isCompleted;
    }

    /// <summary>
    /// 퀘스트 보상을 정의하는 클래스
    /// </summary>
    [Serializable]
    public class QuestReward
    {
        /// <summary>보상 타입</summary>
        public RewardType type;
        /// <summary>자원 보상일 경우 자원 타입</summary>
        public ResourceType resourceType;
        /// <summary>보상 수치</summary>
        public int amount;
    }
}
