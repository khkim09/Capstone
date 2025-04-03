using System;
using UnityEngine;

/// <summary>
/// 문의 위치와 방향 정보를 담는 구조체
/// </summary>
[Serializable]
public struct DoorPosition
{
    /// <summary>문의 상대적 위치 (방 내부 그리드 기준)</summary>
    public Vector2Int position;

    /// <summary>문의 방향</summary>
    public DoorDirection direction;

    /// <summary>
    /// 문 기본 생성자
    /// </summary>
    /// <param name="pos">문의 상대적 위치 (방 내부 그리드 기준)</param>
    /// <param name="dir">문의 방향</param>
    public DoorPosition(Vector2Int pos, DoorDirection dir)
    {
        position = pos;
        direction = dir;
    }
}
