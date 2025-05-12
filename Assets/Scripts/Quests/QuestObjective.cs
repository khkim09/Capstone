using System;
using UnityEngine.Serialization;

/// <summary>
/// 퀘스트 목표를 정의하는 내부 클래스입니다.
/// </summary>
[Serializable]
public class QuestObjective
{
    /// <summary>목표 타입</summary>
    public QuestObjectiveType objectiveType;

    /// <summary>목표 설명</summary>
    public string description;

    /// <summary>목표 대상 ID (예: 아이템 ID)</summary>
    public string targetId;

    /// <summary>필요 수량</summary>
    public int amount;

    public int currentAmount;

    /// <summary>목표 행성 ID</summary>
    public string destinationPlanetId;

    public bool isCompleted;
}
