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
