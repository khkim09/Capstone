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
    public int currentCrewCount = 0; // 현재 선원 수
    public int maxCrewCount = 1; // 총 고용 가능 선원 수
    public List<CrewMember> crewList = new List<CrewMember>();

    [Header("Crew UI")]
    [SerializeField] private GameObject alertAddCrewUI;
    [SerializeField] private GameObject mainUI;

    [Header("Crew Prefabs")]
    [SerializeField] private GameObject[] crewPrefabs;

    [Header("Crew Race Settings")]
    [SerializeField] private CrewRaceSettings[] raceSettings;

    [Header("Default Equipments")]
    public EquipmentItem defaultWeapon;
    public EquipmentItem defaultShield;
    public EquipmentItem defaultAssistant;

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

        newCrew.equipAdditionalSkills[SkillType.PilotSkill] = 0.0f;
        newCrew.equipAdditionalSkills[SkillType.EngineSkill] = 0.0f;
        newCrew.equipAdditionalSkills[SkillType.PowerSkill] = 0.0f;
        newCrew.equipAdditionalSkills[SkillType.ShieldSkill] = 0.0f;
        newCrew.equipAdditionalSkills[SkillType.WeaponSkill] = 0.0f;
        newCrew.equipAdditionalSkills[SkillType.AmmunitionSkill] = 0.0f;
        newCrew.equipAdditionalSkills[SkillType.MedBaySkill] = 0.0f;
        newCrew.equipAdditionalSkills[SkillType.RepairSkill] = 0.0f;
    }

    public void EquipDefaultItemsToCrew(CrewMember crew)
    {
        if (defaultWeapon != null)
        {
            crew.equippedWeapon = defaultWeapon;
            crew.RecalculateEquipmentBonus(crew.equippedWeapon, true);
        }

        if (defaultShield != null)
        {
            crew.equippedShield = defaultShield;
            crew.RecalculateEquipmentBonus(crew.equippedShield, true);
        }

        if (defaultAssistant != null)
        {
            crew.equippedAssistant = defaultAssistant;
            crew.RecalculateEquipmentBonus(crew.equippedAssistant, true);
        }
    }

    public void AddCrewMember(string inputName, CrewRace selectedRace)
    {
        Vector3 startPos = new Vector3(-8.0f, 0.0f, 0.0f);

        // 새 crew 생성
        CrewMember newCrew = Instantiate(crewPrefabs[(int)selectedRace - 1], startPos, Quaternion.identity, null).GetComponent<CrewMember>();
        newCrew.name = $"Name: {inputName}, Race: {selectedRace}";
        newCrew.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);

        // 기본 정보 초기화
        newCrew.crewName = inputName;
        newCrew.isPlayerControlled = true;
        newCrew.race = selectedRace;

        // 선원 정보 세팅
        InitializeCrewSetting(newCrew, selectedRace);

        // 기본 장비 장착
        EquipDefaultItemsToCrew(newCrew);

        // 크루 추가
        crewList.Add(newCrew);
        RefreshCrewList(crewList.Count, maxCrewCount);

        // 확인용 로그
        Debug.Log($"새로운 선원 : {newCrew.crewName} {newCrew.race}");
    }

    // 현재 선원 수 가져오기
    public int GetCurrentCrewCount()
    {
        return currentCrewCount;
    }

    // 현재 수용 가능 선원 수 가져오기 (총 고용 가능 수)
    public int GetMaxCrewCount()
    {
        return maxCrewCount;
    }

    // 선원 수 갱신
    public void RefreshCrewList(int currentCnt, int maxCnt)
    {
        currentCrewCount = currentCnt;
        maxCrewCount = maxCnt;
    }

    public CrewMember GetSelectedCrew()
    {
        // crewList UI (discord 참조)에서 유저가 선택하도록 수정 필요
        return crewList.Count > 0 ? crewList[0] : null; // 예시: 첫 번째 선원
    }
}
