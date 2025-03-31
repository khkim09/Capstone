using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장비 구매 및 착용 담당 Manager
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    // UI에서 선택할 수 있는 장비 목록 (예시)
    [SerializeField] private EquipmentItem[] availableGlobalEquipments; // 무기, 방어구
    [SerializeField] private EquipmentItem[] availableAssistantEquipments; // 보조장치

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 선원 전체에게 적용되는 장비 구매 시 호출
    /// </summary>
    /// <param name="eqItem"></param>
    public void PurchaseAndEquipGlobal(EquipmentItem eqItem)
    {
        if (!eqItem.isGlobalEquip)
            return;

        List<CrewBase> list = GameManager.Instance.GetPlayerShip().GetAllCrew();
        foreach (CrewMember crew in list) crew.ApplyPersonalEquipment(eqItem);
    }

    /// <summary>
    /// 선원 개인에게 장비 적용
    /// </summary>
    /// <param name="crew"></param>
    /// <param name="eqItem"></param>
    public void PurchaseAndEquipPersonal(CrewBase crew, EquipmentItem eqItem)
    {
        if (eqItem.isGlobalEquip)
            return;

        crew.ApplyPersonalEquipment(eqItem);
    }
}
