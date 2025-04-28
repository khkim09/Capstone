using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트 정의를 담는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NewQuestDefinition", menuName = "Game/Quest Definition")]
public class RandomQuest : ScriptableObject
{
    [Header("식별 및 상태")]
    public string questId;                                 // 고유 ID
    [HideInInspector] public QuestStatus status = QuestStatus.NotStarted;

    [Header("기본 정보")]
    public string title;                                   // 퀘스트 제목
    [TextArea] public string description;                  // 퀘스트 설명
    public Sprite questIcon;                               // 퀘스트 아이콘

    [Header("조건 (Collect Item 전용)")]
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

    /// <summary>
    /// 수집형 퀘스트 목표
    /// </summary>
    [Serializable]
    public class QuestObjective
    {
        public string targetId;                   // 검사할 아이템 레퍼런스
        public string description;                        // 목표 설명
        public int requiredAmount;                        // 요구 수량

        [HideInInspector] public int currentAmount;       // 현재 수량 (런타임에 자동 갱신)
        [HideInInspector] public bool isCompleted;        // 완료 여부 (런타임에 자동 체크)
    }

    public enum RewardType
    {
        Resource,
        Item,
        Crew,
        ShipPart
    }

    /// <summary>
    /// 퀘스트 보상
    /// </summary>
    [Serializable]
    public class QuestReward
    {
        public RewardType type;                           // 보상 종류
        public ResourceType resourceType;                 // Resource 타입일 때
        public int amount;                                // 수량
        public string itemId;                             // Item 타입일 때
        public int itemQuantity = 1;                      // 아이템 수량
    }

    /// <summary>
    /// 수락 시 호출
    /// </summary>
    public void Accept()
    {
        if (status != QuestStatus.NotStarted) return;
        status = QuestStatus.Active;
    }

    /// <summary>
    /// 거절 시 호출
    /// </summary>
    public void Decline()
    {
        if (status != QuestStatus.NotStarted) return;
        status = QuestStatus.Failed;
    }

    /// <summary>
    /// 목표 달성 여부 자동 체크
    /// </summary>
    public void CheckCompletion()
    {
        if (status != QuestStatus.Active) return;
        foreach (var o in objectives)
            if (!o.isCompleted) return;
        status = QuestStatus.Completed;
    }

    /// <summary>
    /// 런타임 QuestManager로 전달할 QuestData 생성
    /// </summary>
    public QuestManager.Quest ToQuest()
    {
        var q = new QuestManager.Quest
        {
            id          = questId,
            title       = title,
            description = description,
            status      = QuestManager.QuestStatus.Active
        };

        // 목표 복사
        foreach (var o in objectives)
        {
            q.objectives.Add(new QuestManager.QuestObjective
            {
                description    = o.description,
                currentAmount  = 0,
                requiredAmount = o.requiredAmount,
                isCompleted    = false
            });
        }

        // 보상 복사
        foreach (var r in rewards)
        {
            q.rewards.Add(new QuestManager.QuestReward
            {
                type         = (QuestManager.RewardType)r.type,
                resourceType = r.resourceType,
                amount       = r.amount,
                itemId       = r.itemId
            });
        }

        return q;
    }
}
