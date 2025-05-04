using System.Collections.Generic;
using UnityEngine;

public class CrewPathfinder : MonoBehaviour
{
    private CrewMovementValidator movementValidator;

    public void Initialize(CrewMovementValidator validator)
    {
        movementValidator = validator;
    }

    /// <summary>
    /// 인자로 들어온 위치의 타일까지 최단 경로 검색
    /// </summary>
    /// <param name="crew"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    public List<Vector2Int> FindPathToTile(CrewMember crew, Vector2Int tile)
    {
        Debug.LogWarning($"목적지 타일 : {tile}");
        return AStar.FindPath(crew.GetCurrentTile(), tile, movementValidator);
    }
}
