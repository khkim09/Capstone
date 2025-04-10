using System.Collections.Generic;
using UnityEngine;

public class BlueprintLayoutSaver : MonoBehaviour
{
    public static List<BlueprintRoomSaveData> SavedLayout = new();

    public static void SaveLayout(BlueprintRoom[] bpRooms)
    {
        SavedLayout.Clear();
        foreach (BlueprintRoom bpRoom in bpRooms)
        {
            SavedLayout.Add(new BlueprintRoomSaveData
            {
                bpRoomData = bpRoom.bpRoomData,
                bpLevelIndex = bpRoom.bpLevelIndex,
                bpPosition = bpRoom.bpPosition,
                bpRotation = bpRoom.bpRotation
            });
        }
    }

    public static List<BlueprintRoomSaveData> LoadLayout()
    {
        return new List<BlueprintRoomSaveData>(SavedLayout);
    }
}
