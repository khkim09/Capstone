using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장비 구매 및 장착을 관리하는 매니저 클래스.
/// 전역 장비(Global)와 개인 장비(Personal)의 적용을 담당합니다.
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static EquipmentManager Instance { get; private set; }

    public EquipmentDatabase equipmentDatabase;


    /// <summary>
    /// UI에서 선택 가능한 전역 장비 목록입니다. (무기, 방어구 등)
    /// </summary>
    [SerializeField] private EquipmentItem[] availableGlobalEquipments;

    /// <summary>
    /// UI에서 선택 가능한 보조 장비 목록입니다.
    /// </summary>
    [SerializeField] private EquipmentItem[] availableAssistantEquipments;

    /// <summary>
    /// 인스턴스를 초기화합니다. 중복 객체는 제거됩니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 전 크루에게 적용되는 전역 장비 구매 및 장착 처리.
    /// </summary>
    /// <param name="eqItem">구매한 장비 아이템.</param>
    public void PurchaseAndEquipGlobal(EquipmentItem eqItem)
    {
        if (!eqItem.isGlobalEquip)
            return;

        List<CrewMember> list = GameManager.Instance.GetPlayerShip().GetAllCrew();
        foreach (CrewMember crew in list) crew.ApplyPersonalEquipment(eqItem);
    }

    /// <summary>
    /// 개별 크루에게 장비를 구매하고 적용합니다.
    /// 전역 장비는 무시됩니다.
    /// </summary>
    /// <param name="crew">장비를 적용할 크루.</param>
    /// <param name="eqItem">구매한 장비 아이템.</param>
    public void PurchaseAndEquipPersonal(CrewBase crew, EquipmentItem eqItem)
    {
        if (eqItem.isGlobalEquip)
            return;

        crew.ApplyPersonalEquipment(eqItem);
    }

    // 장비 타입별로 필터링하는 유틸리티 메소드
    public List<EquipmentItem> GetEquipmentsByType(EquipmentType type)
    {
        return equipmentDatabase.GetEquipmentsByType(type);
    }

    // 이름으로 장비 찾기
    public EquipmentItem GetEquipmentByName(string name)
    {
        return equipmentDatabase.GetEquipmentByName(name);
    }

    public EquipmentItem GetEquipmentByTypeAndId(EquipmentType type, int id)
    {
        return equipmentDatabase.GetEquipmentByTypeAndId(type, id);
    }
}
