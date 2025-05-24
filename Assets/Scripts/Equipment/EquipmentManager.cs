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
    /// 기본 장착 무기 장비
    /// </summary>
    public EquipmentItem defaultWeapon;

    /// <summary>
    /// 기본 장착 방어구 장비
    /// </summary>
    public EquipmentItem defaultShield;

    /// <summary>
    /// 기본 장착 보조 장비
    /// </summary>
    public EquipmentItem defaultAssistant;

    /// <summary>
    /// UI에서 선택 가능한 전역 장비 목록입니다. (무기, 방어구 등)
    /// </summary>
    [SerializeField] private EquipmentItem[] availableGlobalEquipments;

    /// <summary>
    /// UI에서 선택 가능한 보조 장비 목록입니다.
    /// </summary>
    [SerializeField] private EquipmentItem[] availableAssistantEquipments;

    public EquipmentItem globalWeaponLevel1;
    public EquipmentItem globalWeaponLevel2;
    public EquipmentItem globalWeaponLevel3;

    public EquipmentItem globalShieldLevel1;
    public EquipmentItem globalShieldLevel2;
    public EquipmentItem globalShieldLevel3;

    /// <summary>
    /// 인스턴스를 초기화합니다. 중복 객체는 제거됩니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
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
        crew.ApplyPersonalEquipment(eqItem);
    }

    /// <summary>
    /// 장비 타입별로 필터링하는 유틸리티 메소드
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public List<EquipmentItem> GetEquipmentsByType(EquipmentType type)
    {
        return equipmentDatabase.GetEquipmentsByType(type);
    }

    /// <summary>
    /// 이름으로 장비 찾기
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public EquipmentItem GetEquipmentByName(string name)
    {
        return equipmentDatabase.GetEquipmentByName(name);
    }

    public EquipmentItem GetEquipmentByTypeAndId(EquipmentType type, int id)
    {
        return equipmentDatabase.GetEquipmentByTypeAndId(type, id);
    }

    /// <summary>
    /// 기본 장비 반환
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public EquipmentItem GetDefaultEquipment(EquipmentType type)
    {
        return type switch
        {
            EquipmentType.WeaponEquipment => defaultWeapon,
            EquipmentType.ShieldEquipment => defaultShield,
            EquipmentType.AssistantEquipment => defaultAssistant,
            _ => null
        };
    }

    public EquipmentItem GetNextLevelEquipment(EquipmentType type)
    {
        if (type == EquipmentType.AssistantEquipment)
        {
            Debug.LogError("보조 장비는 다음 레벨이 없음");
            return null;
        }

        if (type == EquipmentType.WeaponEquipment)
        {
            if (GameManager.Instance.CurrentGlobalWeaponLevel == 3)
                return null;
            else if (GameManager.Instance.CurrentGlobalWeaponLevel == 2)
                return globalWeaponLevel3;
            else
                return globalWeaponLevel2;
        }
        else
        {
            if (GameManager.Instance.CurrentGlobalShieldLevel == 3)
                return null;
            else if (GameManager.Instance.CurrentGlobalShieldLevel == 2)
                return globalShieldLevel3;
            else
                return globalShieldLevel2;
        }
    }
}
