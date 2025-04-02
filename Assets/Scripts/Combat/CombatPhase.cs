/// <summary>
/// 전투의 진행 단계를 나타내는 열거형.
/// 각 단계는 전투의 흐름 제어에 사용됩니다.
/// </summary>
public enum CombatPhase
{
    /// <summary>
    /// 전투 초기화 단계입니다.
    /// 함선 배치 및 전투 준비를 수행합니다.
    /// </summary>
    Initialize,

    /// <summary>
    /// 실제 전투가 진행되는 단계입니다.
    /// 유닛 간의 공격, 스킬 사용 등이 포함됩니다.
    /// </summary>
    Combat,

    /// <summary>
    /// 적 함선이 파괴된 후 연출이 재생되는 단계입니다.
    /// 파괴 애니메이션 또는 효과 처리를 담당합니다.
    /// </summary>
    DestroyAnimation,

    /// <summary>
    /// 전투 보상이 지급되는 단계입니다.
    /// 드롭 아이템 처리나 보상 창을 띄우는 데 사용됩니다.
    /// </summary>
    Reward,

    /// <summary>
    /// 전투가 완전히 종료된 단계입니다.
    /// 다음 화면 전환이나 결과 처리를 수행합니다.
    /// </summary>
    End
}
