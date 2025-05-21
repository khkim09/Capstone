using System.Collections.Generic;

/// <summary>
/// 설계도 1개 저장 데이터 (방 위치/회전/레벨 등 포함)
/// </summary>
[System.Serializable]
public class BlueprintSaveData
{
    public List<BlueprintRoomSaveData> rooms;
    public List<BlueprintWeaponSaveData> weapons;
    public int hullLevel = -1;

    /// <summary>
    /// 도안 정보 저장
    /// </summary>
    /// <param name="rooms"></param>
    /// <param name="weapons"></param>
    public BlueprintSaveData(List<BlueprintRoomSaveData> rooms, List<BlueprintWeaponSaveData> weapons, int hullLevel = -1)
    {
        this.rooms = rooms;
        this.weapons = weapons;
        this.hullLevel = hullLevel;
    }
}
