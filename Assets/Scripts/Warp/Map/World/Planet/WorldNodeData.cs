using UnityEngine;

public class WorldNodeData
{
    /// <summary>
    /// 행성맵에 표시될 상대적인 좌표.
    /// </summary>
    public Vector2 normalizedPosition;

    public bool isVisited = false;

    public bool isCurrentNode = false;
}
