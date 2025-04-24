using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 선원 관리 및 생성을 담당하는 싱글톤 매니저 클래스
/// </summary>
public class CrewFactory : MonoBehaviour
{
    [SerializeField] private GameObject crewPrefab; // 선원 프리팹

    // 선원 이름 생성을 위한 데이터
    [SerializeField] private List<string> humanNamePool;
    [SerializeField] private List<string> amorphousNamePool;
    [SerializeField] private List<string> beastNamePool;
    [SerializeField] private List<string> insectNamePool;
    [SerializeField] private List<string> mechSupNamePool;
    [SerializeField] private List<string> mechTankNamePool;

    public CrewDatabase crewDatabase;

    // 고유 ID 카운터
    private int nextCrewId = 1;

    private void Awake()
    {
        InitializeNamePools();
    }

    /// <summary>
    /// 특정 종족의 스탯 정보 반환
    /// </summary>
    /// <param name="race">종족 타입</param>
    /// <returns>종족 스탯 정보</returns>
    public CrewDatabase.RaceStat GetRaceStat(CrewRace race)
    {
        return crewDatabase.GetRaceStat(race);
    }

    /// <summary>
    /// 새로운 선원 객체 생성
    /// </summary>
    /// <param name="race">선원 종족</param>
    /// <param name="name">선원 이름 (지정하지 않을 경우 자동 생성)</param>
    /// <param name="isPlayerControlled">플레이어 소속 여부</param>
    /// <returns>생성된 선원 객체</returns>
    public CrewBase CreateCrewInstance(CrewRace race, string name = null, bool isPlayerControlled = true)
    {
        // 프리팹 확인
        if (crewPrefab == null)
        {
            Debug.LogError("선원 프리팹이 할당되지 않았습니다!");
            return null;
        }

        // 이름이 지정되지 않았을 경우 종족에 맞는 랜덤 이름 생성
        if (string.IsNullOrEmpty(name)) name = GenerateRandomName(race);

        // 프리팹에서 선원 오브젝트 생성
        GameObject crewObject = Instantiate(crewPrefab);
        crewObject.name = $"Crew_{name}_{race}";

        // 선원 컴포넌트 얻기
        CrewBase crew = crewObject.GetComponent<CrewBase>();
        if (crew == null)
            // CrewMember 컴포넌트가 없으면 추가
            crew = crewObject.AddComponent<CrewBase>();

        // 기본 정보 설정
        crew.crewName = name;
        crew.race = race;
        crew.isPlayerControlled = isPlayerControlled;

        // TODO : 이거 equipment manager 완성되고 주석 해제해야함
        //crew.equippedAssistant = EquipmentManager.Instance.GetEquipmentByTypeAndId(EquipmentType.AssistantEquipment, 0);


        // Factory에서 직접 선원 초기화 (스탯, 스킬 설정)
        InitializeCrewStats(crew);

        // collider 초기화
        InitializeCrewCollider(crew);

        return crew;
    }

    /// <summary>
    /// 선원의 스탯과 스킬을 종족 정보에 따라 초기화합니다.
    /// </summary>
    /// <param name="crew">초기화할 선원 객체</param>
    private void InitializeCrewStats(CrewBase crew)
    {
        // CrewDataDatabase에서 종족 정보 가져오기
        CrewDatabase.RaceStat raceStat = crewDatabase.GetRaceStat(crew.race);

        if (raceStat != null)
        {
            crew.maxHealth = raceStat.maxHealth;
            crew.attack = raceStat.attack;
            crew.defense = raceStat.defense;
            crew.learningSpeed = raceStat.learningSpeed;
            crew.needsOxygen = raceStat.needsOxygen;

            crew.health = crew.maxHealth;
            crew.status = CrewStatus.Normal;
            crew.isAlive = true;
            crew.isMoving = false;

            // 스킬 딕셔너리 초기화
            if (crew.skills == null)
                crew.skills = new Dictionary<SkillType, float>();

            if (crew.maxSkillValueArray == null)
                crew.maxSkillValueArray = new Dictionary<SkillType, float>();

            if (crew.equipAdditionalSkills == null)
                crew.equipAdditionalSkills = new Dictionary<SkillType, float>();

            // 최대 스킬값 설정
            crew.maxSkillValueArray = raceStat.GetMaxSkillValueDictionary();
            crew.maxPilotSkillValue = crew.maxSkillValueArray[SkillType.PilotSkill];
            crew.maxEngineSkillValue = crew.maxSkillValueArray[SkillType.EngineSkill];
            crew.maxPowerSkillValue = crew.maxSkillValueArray[SkillType.PowerSkill];
            crew.maxShieldSkillValue = crew.maxSkillValueArray[SkillType.ShieldSkill];
            crew.maxWeaponSkillValue = crew.maxSkillValueArray[SkillType.WeaponSkill];
            crew.maxAmmunitionSkillValue = crew.maxSkillValueArray[SkillType.AmmunitionSkill];
            crew.maxMedBaySkillValue = crew.maxSkillValueArray[SkillType.MedBaySkill];
            crew.maxRepairSkillValue = crew.maxSkillValueArray[SkillType.RepairSkill];

            // 초기 스킬값 설정
            crew.skills[SkillType.PilotSkill] = raceStat.initialPilotSkill;
            crew.skills[SkillType.EngineSkill] = raceStat.initialEngineSkill;
            crew.skills[SkillType.PowerSkill] = raceStat.initialPowerSkill;
            crew.skills[SkillType.ShieldSkill] = raceStat.initialShieldSkill;
            crew.skills[SkillType.WeaponSkill] = raceStat.initialWeaponSkill;
            crew.skills[SkillType.AmmunitionSkill] = raceStat.initialAmmunitionSkill;
            crew.skills[SkillType.MedBaySkill] = raceStat.initialMedBaySkill;
            crew.skills[SkillType.RepairSkill] = raceStat.initialRepairSkill;

            // 장비 추가 스킬 초기화
            crew.equipAdditionalSkills[SkillType.PilotSkill] = 0.0f;
            crew.equipAdditionalSkills[SkillType.EngineSkill] = 0.0f;
            crew.equipAdditionalSkills[SkillType.PowerSkill] = 0.0f;
            crew.equipAdditionalSkills[SkillType.ShieldSkill] = 0.0f;
            crew.equipAdditionalSkills[SkillType.WeaponSkill] = 0.0f;
            crew.equipAdditionalSkills[SkillType.AmmunitionSkill] = 0.0f;
            crew.equipAdditionalSkills[SkillType.MedBaySkill] = 0.0f;
            crew.equipAdditionalSkills[SkillType.RepairSkill] = 0.0f;
        }
        else
        {
            Debug.LogError($"종족 {crew.race}에 대한 스탯 정보를 찾을 수 없습니다!");
            // 기본값 설정
            crew.maxHealth = 100f;
            crew.health = crew.maxHealth;
            crew.status = CrewStatus.Normal;
            crew.isAlive = true;
            crew.isMoving = false;
        }

        // 스프라이트 렌더러 설정
        SpriteRenderer spriteRenderer = crew.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.sortingOrder = SortingOrderConstants.Character;
    }

    /// <summary>
    /// RTS 조작을 위해 선원의 collider 설정
    /// </summary>
    /// <param name="crew"></param>
    private void InitializeCrewCollider(CrewBase crew)
    {
        // collider 설정
        switch (crew.race)
        {
            case CrewRace.Human:
                BoxCollider2D humanCollider = crew.AddComponent<BoxCollider2D>();
                humanCollider.offset = new Vector2(0.0278020874f, -0.0139010847f);
                humanCollider.size = new Vector2(0.540258348f, 0.691506088f);
                break;
            case CrewRace.Amorphous:
                CircleCollider2D amorphousCollider = crew.AddComponent<CircleCollider2D>();
                amorphousCollider.offset = new Vector2(-0.0199999996f, -0.0250000004f);
                amorphousCollider.radius = 0.3471974f;
                break;
            case CrewRace.MechanicTank:
                BoxCollider2D mechanicTankCollider = crew.AddComponent<BoxCollider2D>();
                mechanicTankCollider.offset = new Vector2(0, -0.198685423f);
                mechanicTankCollider.size = new Vector2(1, 0.602629185f);
                break;
            case CrewRace.MechanicSup:
                BoxCollider2D mechanicSupCollider = crew.AddComponent<BoxCollider2D>();
                mechanicSupCollider.offset = new Vector2(0, 0);
                mechanicSupCollider.size = new Vector2(1, 1);
                break;
            case CrewRace.Beast:
                BoxCollider2D beastCollider = crew.AddComponent<BoxCollider2D>();
                beastCollider.offset = new Vector2(0, 0);
                beastCollider.size = new Vector2(1, 1);
                break;
            case CrewRace.Insect:
                BoxCollider2D insectCollider = crew.AddComponent<BoxCollider2D>();
                insectCollider.offset = new Vector2(0.0749756098f, 0);
                insectCollider.size = new Vector2(0.85004878f, 1);
                break;
        }
    }

    /// <summary>
    /// 저장 데이터에서 선원 복원
    /// </summary>
    /// <param name="data">선원 직렬화 데이터</param>
    /// <returns>복원된 선원 객체</returns>
    public CrewBase RestoreCrewFromData(CrewSerialization.CrewSerializationData data)
    {
        if (data == null)
        {
            Debug.LogError("선원 데이터가 null입니다!");
            return null;
        }

        // 기본 선원 객체 생성
        CrewBase crew = CreateCrewInstance(data.race, data.crewName, data.isPlayerControlled);

        // 데이터가 있는 경우 CrewSerialization에서 처리
        return CrewSerialization.DeserializeCrew(data);
    }

    /// <summary>
    /// 종족에 맞는 랜덤 이름 생성
    /// </summary>
    /// <param name="race">선원 종족</param>
    /// <returns>생성된 이름</returns>
    private string GenerateRandomName(CrewRace race)
    {
        List<string> namePool;

        // 종족별 이름 풀 선택
        switch (race)
        {
            case CrewRace.Human:
                namePool = humanNamePool;
                break;
            case CrewRace.Amorphous:
                namePool = amorphousNamePool;
                break;
            case CrewRace.Beast:
                namePool = beastNamePool;
                break;
            case CrewRace.Insect:
                namePool = insectNamePool;
                break;
            case CrewRace.MechanicSup:
                namePool = mechSupNamePool;
                break;
            case CrewRace.MechanicTank:
                namePool = mechTankNamePool;
                break;
            default:
                namePool = humanNamePool;
                break;
        }

        // 이름 풀이 비어있는 경우 기본 이름 생성
        if (namePool == null || namePool.Count == 0) return $"{race}-{nextCrewId++}";

        // 랜덤 이름 선택
        int randomIndex = Random.Range(0, namePool.Count);
        string baseName = namePool[randomIndex];

        // 이름 중복 방지를 위해 번호 추가
        return $"{baseName}-{nextCrewId++}";
    }

    /// <summary>
    /// 모든 이름 풀 초기화
    /// </summary>
    public void InitializeNamePools()
    {
        // 종족별 기본 이름 목록 설정
        if (humanNamePool == null || humanNamePool.Count == 0)
            humanNamePool = new List<string>
            {
                "Alex",
                "Morgan",
                "Jordan",
                "Taylor",
                "Casey",
                "Riley",
                "Quinn",
                "Avery",
                "Skyler",
                "Reese",
                "Blake",
                "Parker"
            };

        if (amorphousNamePool == null || amorphousNamePool.Count == 0)
            amorphousNamePool = new List<string>
            {
                "Flux",
                "Morphos",
                "Aeon",
                "Echo",
                "Void",
                "Nebula",
                "Zephyr",
                "Wisp",
                "Phantom",
                "Ether",
                "Mist",
                "Plasma"
            };

        if (beastNamePool == null || beastNamePool.Count == 0)
            beastNamePool = new List<string>
            {
                "Fang",
                "Claw",
                "Howl",
                "Talon",
                "Growl",
                "Prowl",
                "Fury",
                "Savage",
                "Primal",
                "Stalker",
                "Hunter",
                "Feral"
            };

        if (insectNamePool == null || insectNamePool.Count == 0)
            insectNamePool = new List<string>
            {
                "Buzz",
                "Sting",
                "Hive",
                "Mandible",
                "Pincers",
                "Carapace",
                "Thorax",
                "Antenna",
                "Swarm",
                "Chitin",
                "Segment",
                "Skitter"
            };

        if (mechSupNamePool == null || mechSupNamePool.Count == 0)
            mechSupNamePool = new List<string>
            {
                "Medic-",
                "Support-",
                "Repair-",
                "Aid-",
                "Tech-",
                "Assist-",
                "Patch-",
                "Mend-",
                "Care-",
                "Fix-",
                "Help-",
                "Boost-"
            };

        if (mechTankNamePool == null || mechTankNamePool.Count == 0)
            mechTankNamePool = new List<string>
            {
                "Tank-",
                "Shield-",
                "Guard-",
                "Armor-",
                "Defend-",
                "Protect-",
                "Bulwark-",
                "Bastion-",
                "Sentinel-",
                "Fortress-",
                "Juggernaut-",
                "Aegis-"
            };
    }
}
