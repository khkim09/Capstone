using System;

/// <summary>
/// 퀘스트 보상을 정의하는 내부 클래스입니다.
/// </summary>
[Serializable]
public class QuestReward
{
    public QuestRewardType questRewardType;

    /// <summary>보상 수치 (COMA 기준)</summary>
    public int amount;
}
