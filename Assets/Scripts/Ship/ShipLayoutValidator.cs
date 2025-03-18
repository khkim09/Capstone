using System.Collections.Generic;
using UnityEngine;

public class ShipLayoutValidator : MonoBehaviour
{
    [Header("Ship Customization Manager Reference")]
    public ShipCustomizationManager customizationManager;

    /// <summary>
    /// 모든 배치된 모듈을 검사합니다.
    /// 조건: 각 Room은 인접한 셀에 있는 Door를 통해 다른 Room 또는 Corridor와 연결되어 있어야 합니다.
    /// </summary>
    /// <returns>유효하면 true, 하나라도 조건에 맞지 않으면 false</returns>
    public bool ValidateLayout()
    {
        // ShipCustomizationManager의 PlacedModules 딕셔너리를 가져옵니다.
        Dictionary<Vector2Int, GameObject> modules = customizationManager.PlacedModules;

        // 각 그리드 셀에 배치된 모듈을 순회합니다.
        foreach (KeyValuePair<Vector2Int, GameObject> kvp in modules)
        {
            Vector2Int pos = kvp.Key;
            GameObject module = kvp.Value;

            // 모듈 이름에 "Room"이 포함되어 있다면 Room으로 판단
            if (module.name.Contains("Room"))
            {
                bool isConnected = false;

                // 상하좌우 네 방향
                Vector2Int[] directions = new Vector2Int[]
                {
                    Vector2Int.up,
                    Vector2Int.down,
                    Vector2Int.left,
                    Vector2Int.right
                };

                // Room의 인접 셀을 검사하여 Door가 있는지 확인
                foreach (Vector2Int dir in directions)
                {
                    Vector2Int adjacentPos = pos + dir;
                    if (modules.ContainsKey(adjacentPos))
                    {
                        GameObject adjacentModule = modules[adjacentPos];
                        // 인접 모듈이 Door인 경우
                        if (adjacentModule.name.Contains("Door"))
                        {
                            // 해당 Door의 주변 셀(현재 Room 셀 제외)을 검사합니다.
                            foreach (Vector2Int doorDir in directions)
                            {
                                Vector2Int doorAdjacentPos = adjacentPos + doorDir;
                                if (doorAdjacentPos == pos)
                                    continue; // 현재 Room 셀은 제외

                                if (modules.ContainsKey(doorAdjacentPos))
                                {
                                    GameObject doorAdjacentModule = modules[doorAdjacentPos];
                                    // Door의 다른 쪽에 Room(현재 Room과 다른 경우) 또는 Corridor가 있으면 연결된 것으로 판단
                                    if ((doorAdjacentModule.name.Contains("Room") && doorAdjacentModule != module) ||
                                        doorAdjacentModule.name.Contains("Corridor"))
                                    {
                                        isConnected = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (isConnected)
                        break;
                }

                if (!isConnected)
                {
                    Debug.LogWarning($"Room at grid position {pos} is not properly connected to another Room or Corridor via a Door.");
                    return false;
                }
            }
        }

        Debug.Log("All rooms are properly connected.");
        return true;
    }
}
