using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 랜덤 퀘스트의 데이터 정의를 위한 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "New Random Quest", menuName = "Quest/Random Quest")]
public class RandomQuest : ScriptableObject
{
    /// <summary>퀘스트 고유 ID</summary>
    public string questId;

    /// <summary>퀘스트 제목</summary>
    public string title;

    /// <summary>퀘스트 설명</summary>
    public string description;

    /// <summary>퀘스트 상태</summary>
    public QuestManager.QuestStatus status;

    /// <summary>퀘스트 목표 목록</summary>
    public List<QuestObjective> objectives = new List<QuestObjective>();

    /// <summary>퀘스트 보상 목록</summary>
    public List<QuestReward> rewards = new List<QuestReward>();

    /// <summary>퀘스트를 수락 처리합니다.</summary>
    public void Accept()
    {
        status = QuestManager.QuestStatus.Active;
    }

    /// <summary>퀘스트를 거절 처리합니다.</summary>
    public void Decline()
    {
        status = QuestManager.QuestStatus.NotStarted;
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
            int required;

            switch (o.objectiveType)
            {
                case QuestObjectiveType.CrewTransport:
                case QuestObjectiveType.PirateHunt:
                    required = o.killCount;
                    break;

                case QuestObjectiveType.ItemTransport:
                case QuestObjectiveType.ItemProcurement:
                    required = o.requiredAmount;
                    break;

                default:
                    required = 0;
                    break;
            }

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

    /// <summary>
    /// 퀘스트 목표를 정의하는 내부 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class QuestObjective
    {
        /// <summary>목표 타입</summary>
        public QuestObjectiveType objectiveType;

        /// <summary>목표 설명</summary>
        public string description;

        /// <summary>목표 대상 ID (예: 아이템 ID)</summary>
        public string targetId;

        /// <summary>필요 수량</summary>
        public int requiredAmount;

        /// <summary>필요 처치 수 (해적, 선원)</summary>
        public int killCount;

        /// <summary>목표 행성 ID</summary>
        public string destinationPlanetId;
    }

    /// <summary>
    /// 퀘스트 보상을 정의하는 내부 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class QuestReward
    {
        /// <summary>보상 수치 (COMA 기준)</summary>
        public int amount;
    }

    /// <summary>
    /// 퀘스트 목표 타입 열거형입니다.
    /// </summary>
    public enum QuestObjectiveType
    {
        PirateHunt,
        ItemTransport,
        ItemProcurement,
        CrewTransport
    }
}
