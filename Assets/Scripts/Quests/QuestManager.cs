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
    public delegate void QuestChangedHandler(RandomQuest quest);


    [SerializeField] private List<RandomQuest> activeQuests = new();
    [SerializeField] private List<RandomQuest> completedQuests = new();
    [SerializeField] private List<RandomQuest> failedQuests = new List<RandomQuest>();

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

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnYearChanged += CheckQuestExpiration;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnYearChanged -= CheckQuestExpiration;
    }

    /// <summary>퀘스트가 새로 추가될 때 발생하는 이벤트</summary>
    public event QuestChangedHandler OnQuestAdded;

    /// <summary>퀘스트가 갱신될 때 발생하는 이벤트</summary>
    public event QuestChangedHandler OnQuestUpdated;

    /// <summary>퀘스트가 완료될 때 발생하는 이벤트</summary>
    public event QuestChangedHandler OnQuestCompleted;

    /// <summary>퀘스트가 실패할 때 발생하는 이벤트</summary>
    public event QuestChangedHandler OnQuestFailed;

    /// <summary>
    /// 새로운 퀘스트를 추가합니다.
    /// </summary>
    /// <param name="quest">추가할 퀘스트</param>
    public void AddQuest(RandomQuest quest)
    {
        quest.status = QuestStatus.Active;
        quest.questAcceptedYear = GameManager.Instance.CurrentYear;
        activeQuests.Add(quest);
        OnQuestAdded?.Invoke(quest);
        Debug.Log($"New quest added: {quest.title}");
    }

    /// <summary>
    /// 외부에서 퀘스트 완료 처리를 강제로 호출할 때 사용합니다.
    /// </summary>
    /// <param name="quest">완료된 퀘스트</param>
    public void TriggerQuestCompleted(RandomQuest quest)
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
        RandomQuest quest = activeQuests.Find(q => q.questId == questId);
        if (quest != null && objectiveIndex >= 0 && objectiveIndex < quest.objectives.Count)
        {
            QuestObjective objective = quest.objectives[objectiveIndex];
            objective.currentAmount += amount;
            objective.isCompleted = objective.currentAmount >= objective.amount;

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
    /// 완료 시 UI 패널 상태를 기억하고 완료 후 원래대로 복구합니다.
    /// </summary>
    /// <param name="quest">완료된 퀘스트</param>
    private void CompleteQuest(RandomQuest quest)
    {
        // 현재 UI 상태 저장
        QuestListUI questListUI = FindObjectOfType<QuestListUI>();
        QuestUIManager questUIManager = FindObjectOfType<QuestUIManager>();

        // 원래 Complete 처리
        quest.status = QuestStatus.Completed;
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        if (questUIManager != null)
            questUIManager.ShowCompletion(quest);

        OnQuestCompleted?.Invoke(quest);
        Debug.Log($"Quest completed: {quest.title}");
    }

    /// <summary>
    /// 퀘스트 보상을 지급하는 함수입니다.
    /// </summary>
    /// <param name="quest"></param>
    public void GrantRewardForQuest(RandomQuest quest)
    {
        if (quest.status == QuestStatus.Completed)
        {
            foreach (QuestReward reward in quest.rewards)
                if (reward.questRewardType == QuestRewardType.COMA)
                    ResourceManager.Instance.ChangeResource(ResourceType.COMA, reward.amount);

            Debug.Log($"Quest reward granted for: {quest.title}");
        }
    }

    /// <summary>
    /// 매년 호출되어 20년 이상 지난 퀘스트를 자동 실패 처리합니다.
    /// </summary>
    /// <param name="currentYear">현재 연도</param>
    private void CheckQuestExpiration(int currentYear)
    {
        List<RandomQuest> expiredQuests = new();
        foreach (RandomQuest quest in activeQuests)
        {
            if (quest.questAcceptedYear >= 0 &&
                currentYear - quest.questAcceptedYear >= 20)
                expiredQuests.Add(quest);
        }

        foreach (RandomQuest quest in expiredQuests)
            FailQuest(quest);
    }

    /// <summary>
    /// 퀘스트를 실패 처리합니다.
    /// </summary>
    /// <param name="quest">실패할 퀘스트</param>
    private void FailQuest(RandomQuest quest)
    {
        quest.status = QuestStatus.Failed;
        activeQuests.Remove(quest);
        failedQuests.Add(quest);

        OnQuestFailed?.Invoke(quest);
        Debug.Log($"Quest failed due to expiration: {quest.title}");
    }

    /// <summary>
    /// 외부에서 호출할 수 있는 퀘스트 실패 처리 함수입니다.
    /// </summary>
    public void RequestFailQuest(RandomQuest quest)
    {
        FailQuest(quest);
    }

    /// <summary>
    /// 현재 진행 중인 퀘스트 목록을 반환합니다.
    /// </summary>
    /// <returns>진행 중인 퀘스트 리스트</returns>
    public List<RandomQuest> GetActiveQuests()
    {
        return activeQuests;
    }

    /// <summary>
    /// 완료된 퀘스트 목록을 반환합니다.
    /// </summary>
    /// <returns>완료된 퀘스트 리스트</returns>
    public List<RandomQuest> GetCompletedQuests()
    {
        return completedQuests;
    }

    /// <summary>
    /// 실패한 퀘스트 목록을 반환합니다.
    /// </summary>
    /// <returns>실패한 퀘스트 리스트</returns>
    public List<RandomQuest> GetFailedQuests()
    {
        return failedQuests;
    }
}
