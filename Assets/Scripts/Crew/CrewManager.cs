using System.Collections.Generic;
using UnityEngine;

public class CrewManager : MonoBehaviour
{
    // instance
    public static CrewManager Instance { get; set; }

    // CrewInfoWrapper
    private CrewInfoWrapper crewInfoWrapper;

    // 승무원 리스트
    [Header("Crew List")]
    [SerializeField] public List<CrewMember> crewList = new List<CrewMember>();

    [Header("Crew UI")]
    [SerializeField] private GameObject alertAddCrewUI;
    [SerializeField] private GameObject mainUI;

    [Header("Crew Prefabs")]
    [SerializeField] private GameObject[] crewPrefabs;

    [Header("Crew Race Settings")]
    [SerializeField] private CrewRaceSettings[] raceSettings;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (crewList.Count == 0)
                AlertNeedCrew();
        }
        else
        {
            Destroy(gameObject);
        }

        // CrewInfoLoader의 LoadedCrewInfo를 가져옵니다.
        crewInfoWrapper = CrewInfoLoader.crewInfo;
        if (crewInfoWrapper != null)
        {
            Debug.Log("CrewInfo data load 성공");
        }
        else
        {
            Debug.LogError("CrewInfo data load 실패");
        }
    }

    private void Update()
    {
        if (alertAddCrewUI.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                alertAddCrewUI.SetActive(false);
                mainUI.SetActive(true);
            }
        }
    }

    private void AlertNeedCrew()
    {
        alertAddCrewUI.SetActive(true);
        mainUI.SetActive(false);
    }

    private void InitializeCrewSetting(CrewMember newCrew, CrewRace newCrewRace)
    {
        CrewRaceSettings settings = System.Array.Find(raceSettings, s => s.race == newCrewRace);
        if (settings == null)
        {
            Debug.LogWarning($"설정 에셋이 {newCrewRace} 종족에 대해 할당 안 됨");
            return;
        }

        newCrew.maxHealth = settings.maxHealth;
        newCrew.attack = settings.attack;
        newCrew.defense = settings.defense;
        newCrew.learningSpeed = settings.learningSpeed;
        newCrew.needsOxygen = settings.needsOxygen;

        newCrew.health = newCrew.maxHealth;
        newCrew.status = CrewStatus.Normal;
        newCrew.isAlive = true;
        newCrew.isMoving = false;

        newCrew.maxSkillValueArray = (float[])settings.maxSkillValueArray.Clone();
        newCrew.maxPilotSkillValue = newCrew.maxSkillValueArray[0];
        newCrew.maxEngineSkillValue = newCrew.maxSkillValueArray[1];
        newCrew.maxPowerSkillValue = newCrew.maxSkillValueArray[2];
        newCrew.maxShieldSkillValue = newCrew.maxSkillValueArray[3];
        newCrew.maxWeaponSkillValue = newCrew.maxSkillValueArray[4];
        newCrew.maxAmmunitionSkillValue = newCrew.maxSkillValueArray[5];
        newCrew.maxMedBaySkillValue = newCrew.maxSkillValueArray[6];
        newCrew.maxRepairSkillValue = newCrew.maxSkillValueArray[7];

        newCrew.skills[SkillType.PilotSkill] = settings.initialPilotSkill;
        newCrew.skills[SkillType.EngineSkill] = settings.initialEngineSkill;
        newCrew.skills[SkillType.PowerSkill] = settings.initialPowerSkill;
        newCrew.skills[SkillType.ShieldSkill] = settings.initialShieldSkill;
        newCrew.skills[SkillType.WeaponSkill] = settings.initialWeaponSkill;
        newCrew.skills[SkillType.AmmunitionSkill] = settings.initialAmmunitionSkill;
        newCrew.skills[SkillType.MedBaySkill] = settings.initialMedBaySkill;
        newCrew.skills[SkillType.RepairSkill] = settings.initialRepairSkill;
    }

    public void AddGlobalEquipment(EquipmentItem eqItem)
    {
        // global 장비는 WeaponEquipment와 ShieldEquipment 타입으로 구분합니다.
        if (eqItem.eqType != EquipmentType.WeaponEquipment && eqItem.eqType != EquipmentType.ShieldEquipment)
        {
            Debug.LogWarning("이 장비는 모든 선원에게 적용되는 타입이 아닙니다.");
            return;
        }

        foreach (CrewMember crew in crewList)
        {
            // 예시: 공격력과 방어력 보너스를 더해줍니다.
            crew.allCrewEquipment += eqItem.eqAttackBonus;  // 무기라면 공격력 보너스
            crew.allCrewEquipment += eqItem.eqDefenseBonus;   // 방어구라면 방어력 보너스 (혹은 별도의 변수를 사용)
            // 체력 보너스 등도 필요하면 추가
        }
        Debug.Log($"모든 선원에게 {eqItem.eqName} 장비 효과 적용 완료.");
    }

    public void AddCrewMember(string inputName, CrewRace selectedRace)
    {
        Vector3 startPos = new Vector3(-8.0f, 0.0f, 0.0f);

        // 겹쳐서 생성되는거 수정필요

        // 새 crew 생성
        CrewMember newCrew = Instantiate(crewPrefabs[(int)selectedRace - 1], startPos, Quaternion.identity, null).GetComponent<CrewMember>();
        newCrew.name = $"Name: {inputName}, Race: {selectedRace}";
        newCrew.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);

        // 기본 정보 초기화
        newCrew.crewName = inputName;
        newCrew.race = selectedRace;

        // 선원 정보 세팅
        InitializeCrewSetting(newCrew, selectedRace);

        // 크루 추가
        crewList.Add(newCrew);

        // 확인용 로그
        Debug.Log($"새로운 선원 : {newCrew.crewName} {newCrew.race}");
    }
}
