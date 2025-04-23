using System.Collections.Generic;
using UnityEngine;

public class ShipPathDebugVisualizer : MonoBehaviour
{
    public List<Room> visitedRooms = new();
    public Room startRoom;
    public List<Room> allRooms = new();

    private void OnDrawGizmos()
    {
        if (allRooms == null || allRooms.Count == 0)
        {
            Debug.LogWarning("no rooms");
            return;
        }

        foreach (Room room in allRooms)
        {
            if (room == null) continue;

            Vector3 center = room.transform.position;
            center.z = 0;

            if (room == startRoom)
                Gizmos.color = Color.green;
            else if (visitedRooms.Contains(room))
                Gizmos.color = Color.yellow;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawCube(center, Vector3.one * 0.5f);
        }
    }
}
