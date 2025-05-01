using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject 기반의 랜덤 퀘스트 정의 클래스입니다.
/// 퀘스트의 상태, 조건, 보상 정보를 포함하며 퀘스트 인스턴스로 변환하는 기능을 가집니다.
/// </summary>
[CreateAssetMenu(fileName = "NewQuestDefinition", menuName = "Game/Quest Definition")]
public class RandomQuest : ScriptableObject
{
    /// <summary>퀘스트 ID</summary>
    [Header("식별 및 상태")]
    public string questId;

    /// <summary>현재 퀘스트 상태 (NotStarted, Active 등)</summary>
    [HideInInspector] public QuestStatus status = QuestStatus.NotStarted;

    /// <summary>퀘스트 수락 당시의 행성 ID</summary>
    [Header("수락한 행성 ID")]
    public string acceptingPlanetId;

    /// <summary>퀘스트 제목</summary>
    [Header("기본 정보")]
    public string title;

    /// <summary>퀘스트 설명</summary>
    [TextArea] public string description;

    /// <summary>퀘스트 아이콘 이미지</summary>
    public Sprite questIcon;

    /// <summary>퀘스트 조건 목록</summary>
    [Header("조건")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

    /// <summary>퀘스트 보상 목록</summary>
    [Header("보상")]
    public List<QuestReward> rewards = new List<QuestReward>();

    /// <summary>
    /// 퀘스트의 상태를 정의합니다.
    /// </summary>
    public enum QuestStatus
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }

    /// <summary>
    /// 퀘스트 목표의 유형을 정의합니다.
    /// </summary>
    public enum QuestObjectiveType
    {
        PirateHunt,
        ItemTransport,
        ItemProcurement,
        CrewTransport
    }

    /// <summary>
    /// 개별 퀘스트 목표 정보를 나타냅니다.
    /// </summary>
    [Serializable]
    public class QuestObjective
    {
        /// <summary>목표 타입</summary>
        public QuestObjectiveType objectiveType;

        /// <summary>대상 ID (아이템 ID 등)</summary>
        public string targetId;

        /// <summary>목표 도달 행성 ID</summary>
        public string destinationPlanetId;

        /// <summary>목표 설명</summary>
        public string description;

        /// <summary>필요 수량</summary>
        public int requiredAmount;

        /// <summary>처치 수 (PirateHunt, CrewTransport 등)</summary>
        public int killCount;

        /// <summary>현재 진행 수</summary>
        [HideInInspector] public int currentAmount;

        /// <summary>달성 여부</summary>
        [HideInInspector] public bool isCompleted;
    }

    /// <summary>
    /// 퀘스트 보상을 정의합니다.
    /// </summary>
    [Serializable]
    public class QuestReward
    {
        /// <summary>COMA 보상 수치</summary>
        public int amount; // 항상 COMA 고정
    }

    /// <summary>
    /// 퀘스트를 수락 처리합니다.
    /// </summary>
    public void Accept()
    {
        if (status != QuestStatus.NotStarted) return;
        status = QuestStatus.Active;
    }

    /// <summary>
    /// 퀘스트를 거절 처리합니다.
    /// </summary>
    public void Decline()
    {
        if (status != QuestStatus.NotStarted) return;
        status = QuestStatus.Failed;
    }

    /// <summary>
    /// 퀘스트의 모든 목표 달성 여부를 체크하여 완료 상태로 전환합니다.
    /// </summary>
    public void CheckCompletion()
    {
        if (status != QuestStatus.Active) return;

        foreach (var o in objectives)
            if (!o.isCompleted) return;

        status = QuestStatus.Completed;
    }

    /// <summary>
    /// 이 랜덤 퀘스트를 QuestManager용 퀘스트 인스턴스로 변환합니다.
    /// </summary>
    /// <returns>QuestManager.Quest 객체</returns>
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
