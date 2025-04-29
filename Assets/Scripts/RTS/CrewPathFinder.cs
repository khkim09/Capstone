using System.Collections.Generic;
using UnityEngine;

public class CrewPathfinder : MonoBehaviour
{
    private CrewMovementValidator movementValidator;

    public void Initialize(CrewMovementValidator validator)
    {
        movementValidator = validator;
    }

    public List<Vector2Int> FindPathToRoom(CrewMember crew, Room targetRoom)
    {
        List<Vector2Int> bestPath = null;
        int minCost = int.MaxValue;

        foreach (Vector2Int entry in targetRoom.GetRotatedCrewEntryGridPriority())
        {
            Debug.LogError($"목적지 : {entry}");
            var path = AStar.FindPath(crew.GetCurrentTile(), entry, movementValidator);
            if (path != null && path.Count < minCost)
            {
                bestPath = path;
                minCost = path.Count;
            }
        }

        return bestPath;
    }
}
