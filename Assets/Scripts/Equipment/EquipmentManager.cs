using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    // UI에서 선택할 수 있는 장비 목록 (예시)
    [SerializeField] private EquipmentItem[] availableGlobalEquipments;    // 무기, 방어구
    [SerializeField] private EquipmentItem[] availableAssistantEquipments; // 보조장치

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 선원 전체 적용 장비 (무기, 방어구)
    public void PurchaseAndEquipGlobal(EquipmentItem eqItem)
    {
        // 가격 체크, 재화 차감 등 로직

        // 장비 착용
        CrewManager.Instance.AddGlobalEquipment(eqItem);
    }

    // 선원 개인 적용 장비 (보조 장비)
    public void PurchaseAndEquipAssistant(CrewMember crew, EquipmentItem eqItem)
    {
        // 가격 체크, 재화 차감 등 로직은 생략하고 바로 장착
        crew.AddAssistantEquipment(eqItem);
    }
}
