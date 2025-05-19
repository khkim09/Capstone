using System.Collections.Generic;

/// <summary>
/// 설계도 1개 저장 데이터 (방 위치/회전/레벨 등 포함)
/// </summary>
[System.Serializable]
public class BlueprintSaveData
{
    public List<BlueprintRoomSaveData> rooms;
    public List<BlueprintWeaponSaveData> weapons;

    public BlueprintSaveData(List<BlueprintRoomSaveData> rooms, List<BlueprintWeaponSaveData> weapons)
    {
        this.rooms = rooms;
        this.weapons = weapons;
    }
}
