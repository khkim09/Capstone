/// <summary>
/// 현재 맵 시스템의 상태를 나타냅니다.
/// 노드 배치 모드 또는 이벤트 트리 모드를 구분합니다.
/// </summary>
public enum MapType
{
    /// <summary>
    /// 노드를 직접 배치하는 편집 모드입니다.
    /// </summary>
    NodePlacement, // 노드 배치 맵 상태

    /// <summary>
    /// 노드를 직접 배치하는 편집 모드입니다.
    /// </summary>
    EventTree // 이벤트 트리 맵 상태
}
