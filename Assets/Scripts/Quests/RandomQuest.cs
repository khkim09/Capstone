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
    public QuestStatus status;

    /// <summary>퀘스트 목표 목록</summary>
    public List<QuestObjective> objectives = new();

    /// <summary>퀘스트 보상 목록</summary>
    public List<QuestReward> rewards = new();

    /// <summary>퀘스트를 수락 처리합니다.</summary>
    public void Accept()
    {
        status = QuestStatus.Active;
    }

    /// <summary>퀘스트를 거절 처리합니다.</summary>
    public void Decline()
    {
        status = QuestStatus.NotStarted;
    }
}
