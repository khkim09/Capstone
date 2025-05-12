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
    public void AddQuest(RandomQuest quest)
    {
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
    /// </summary>
    /// <param name="quest">완료된 퀘스트</param>
    private void CompleteQuest(RandomQuest quest)
    {
        quest.status = QuestStatus.Completed;
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        foreach (QuestReward reward in quest.rewards)
            if (reward.questRewardType == QuestRewardType.COMA)
                ResourceManager.Instance.ChangeResource(ResourceType.COMA, reward.amount);

        QuestUIManager ui = FindObjectOfType<QuestUIManager>();
        if (ui != null) ui.ShowCompletion(quest);

        OnQuestCompleted?.Invoke(quest);
        Debug.Log($"Quest completed: {quest.title}");
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
}
