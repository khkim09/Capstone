using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장비 데이터베이스 - 모든 장비 아이템을 관리하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "EquipmentDatabase", menuName = "Equipment/EquipmentDatabase")]
public class EquipmentDatabase : ScriptableObject
{
    public List<EquipmentItem> allEquipments = new();

    // 장비 타입별로 필터링하는 유틸리티 메소드
    public List<EquipmentItem> GetEquipmentsByType(EquipmentType type)
    {
        return allEquipments.FindAll(eq => eq.eqType == type);
    }

    // 이름으로 장비 찾기
    public EquipmentItem GetEquipmentByName(string name)
    {
        return allEquipments.Find(eq => eq.eqName == name);
    }

    public EquipmentItem GetEquipmentByTypeAndId(EquipmentType type, int id)
    {
        return allEquipments.Find(eq => eq.eqType == type && eq.eqId == id);
    }

    public List<EquipmentItem> GetEquipmentByPlanet(ItemPlanet planet)
    {
        return allEquipments.FindAll(eq => eq.planet == planet);
    }
}
