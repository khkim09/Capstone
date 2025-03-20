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
    [SerializeField] private List<CrewMember> crewList = new List<CrewMember>();

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

        InitializeCrewSetting(newCrew, selectedRace);

        /*
                // 기본 장비 설정 (수정 필요)
                newCrew.equipment.workAssistant = "Basic workAssistant";
                newCrew.equipment.weapon = "Basic weapon";
                newCrew.equipment.armor = "Basic armor";
        */

        // 크루 추가
        crewList.Add(newCrew);

        // 확인용 로그
        Debug.Log($"새로운 선원 : {newCrew.crewName} {newCrew.race}");
    }
}
