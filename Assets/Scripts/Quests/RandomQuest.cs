using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 랜덤 퀘스트의 데이터 정의를 위한 클래스입니다.
/// </summary>
public class RandomQuest
{
    /// <summary>퀘스트 고유 ID</summary>
    public string questId;

    /// <summary>퀘스트 제목</summary>
    public string title;

    /// <summary>퀘스트 설명</summary>
    public string description;

    /// <summary>퀘스트 수락 연도</summary>
    public int questAcceptedYear = -1;

    /// <summary>
    /// 퀘스트 만료 연도
    /// </summary>
    public int QuestExpiredYear => questAcceptedYear + Constants.Quest.QuestDuration;

    /// <summary>
    /// 퀘스트 만료까지 남은 연도
    /// </summary>
    public int QuestDurationLeft => QuestExpiredYear - GameManager.Instance.CurrentYear;

    /// <summary>퀘스트 수락한 Planet ID</summary>
    public string acceptedPlanetId;

    /// <summary>퀘스트 상태</summary>
    public QuestStatus status;

    /// <summary>퀘스트 목표 목록</summary>
    public List<QuestObjective> objectives = new();

    /// <summary>퀘스트 보상 목록</summary>
    public List<QuestReward> rewards = new();

    /// <summary>
    /// 퀘스트 조건 충족 여부
    /// </summary>
    private bool canCompleteQuest = false;

    /// <summary>Planet ID를 저장하며 퀘스트를 수락 처리</summary>
    public void Accept(string planetId)
    {
        status = QuestStatus.Active;
        acceptedPlanetId = planetId;
        questAcceptedYear = GameManager.Instance != null ? GameManager.Instance.CurrentYear : 0;
    }

    /// <summary>퀘스트를 거절 처리합니다.</summary>
    public void Decline()
    {
        status = QuestStatus.NotStarted;
    }

    /// <summary>
    /// 완료 준비 상태인지 판별합니다.
    /// 모든 목표가 완료된 활성 퀘스트만 true를 반환합니다.
    /// </summary>
    public bool GetCanComplete()
    {
        return canCompleteQuest;
    }

    public void SetCanComplete(bool canComplete)
    {
        canCompleteQuest = canComplete;
    }
}
