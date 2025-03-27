using System.Collections.Generic;
using UnityEngine;

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

    public void PurchaseAndEquipGlobal(EquipmentItem eqItem)
    {
        if (!eqItem.isGlobalEquip)
            return;

        List<CrewBase> list = CrewManager.Instance.crewList;
        foreach (CrewMember crew in list) crew.ApplyPersonalEquipment(eqItem);
    }

    // 선원 장비 적용
    public void PurchaseAndEquipPersonal(CrewBase crew, EquipmentItem eqItem)
    {
        if (eqItem.isGlobalEquip)
            return;

        crew.ApplyPersonalEquipment(eqItem);
    }
}
