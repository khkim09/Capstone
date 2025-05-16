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

        // animator 초기화
        InitializeCrewAnimator(crew);

        return crew;
    }

    public CrewBase CreateCrewObject(CrewBase crew)
    {
        GameObject crewObject = Instantiate(crewPrefab);

        CrewBase crewComponent = crewObject.GetComponent<CrewBase>();
        if (crewComponent == null)
            // CrewMember 컴포넌트가 없으면 추가
            crewComponent = crewObject.AddComponent<CrewBase>();

        crewComponent.CopyFrom(crew);

        crewObject.name = $"crew_{crew.race}_{crew.crewName}";

        return crewComponent;
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
            crew.isAlive = true;
            crew.isMoving = false;

            // 기본 장비 착용
            crew.equippedWeapon = EquipmentManager.Instance.defaultWeapon;
            crew.equippedShield = EquipmentManager.Instance.defaultShield;
            crew.equippedAssistant = EquipmentManager.Instance.defaultAssistant;

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
            crew.isAlive = true;
            crew.isMoving = false;
        }

        // 스프라이트 렌더러 설정
        SpriteRenderer spriteRenderer = crew.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.sortingOrder = Constants.SortingOrders.Character;
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
                humanCollider.offset = new Vector2(0, 0);
                humanCollider.size = new Vector2(0.8f, 0.8f);
                break;
            case CrewRace.Amorphous:
                BoxCollider2D amorphousCollider = crew.AddComponent<BoxCollider2D>();
                amorphousCollider.offset = new Vector2(0, 0);
                amorphousCollider.size = new Vector2(0.8f, 0.8f);
                break;
            case CrewRace.MechanicTank:
                BoxCollider2D mechanicTankCollider = crew.AddComponent<BoxCollider2D>();
                mechanicTankCollider.offset = new Vector2(0, 0);
                mechanicTankCollider.size = new Vector2(1, 1);
                break;
            case CrewRace.MechanicSup:
                BoxCollider2D mechanicSupCollider = crew.AddComponent<BoxCollider2D>();
                mechanicSupCollider.offset = new Vector2(0, 0);
                mechanicSupCollider.size = new Vector2(1, 1);
                break;
            case CrewRace.Beast:
                BoxCollider2D beastCollider = crew.AddComponent<BoxCollider2D>();
                beastCollider.offset = new Vector2(0, 0);
                beastCollider.size = new Vector2(0.9f, 1);
                break;
            case CrewRace.Insect:
                BoxCollider2D insectCollider = crew.AddComponent<BoxCollider2D>();
                insectCollider.offset = new Vector2(0, 0);
                insectCollider.size = new Vector2(0.8f, 1);
                break;
        }
    }

    /// <summary>
    /// 종족 별 animator 연결
    /// </summary>
    /// <param name="crew"></param>
    private void InitializeCrewAnimator(CrewBase crew)
    {
        // animator 연결
        Animator animator = crew.GetComponent<Animator>();
        if (animator == null)
            animator = crew.gameObject.AddComponent<Animator>();

        // 종족에 맞는 animator controller 로드
        RuntimeAnimatorController controller =
            Resources.Load<RuntimeAnimatorController>($"Animation/{crew.race}/{crew.race}");

        if (controller == null)
        {
            Debug.LogError($"Animator Controller를 찾을 수 없습니다: Animation/{crew.race}/{crew.race}");
            return;
        }

        animator.runtimeAnimatorController = controller;

        // CrewBase에 animator 연결
        crew.animator = animator;
    }

    /// <summary>
    /// 저장 데이터에서 선원 복원
    /// </summary>
    /// <param name="data">선원 직렬화 데이터</param>
    /// <returns>복원된 선원 객체</returns>
    /// <summary>
    /// 저장 데이터에서 선원 복원 (Easy Save 3 사용)
    /// </summary>
    /// <param name="savedData">ES3 저장 키 또는 파일 경로</param>
    /// <returns>복원된 선원 객체</returns>
    public CrewBase RestoreCrewFromES3(string savedKey, string filename)
    {
        if (!ES3.FileExists(filename) || !ES3.KeyExists(savedKey, filename))
        {
            Debug.LogError($"선원 데이터를 찾을 수 없습니다: {filename}, 키: {savedKey}");
            return null;
        }

        try
        {
            // 직접 선원 객체 복원
            CrewBase crew = ES3.Load<CrewBase>(savedKey, filename);

            // 객체가 제대로 로드되지 않은 경우 수동으로 생성
            if (crew == null)
            {
                // 기본 정보 로드
                string crewName = ES3.Load<string>($"{savedKey}.crewName", filename);
                CrewRace race = ES3.Load<CrewRace>($"{savedKey}.race", filename);
                bool isPlayerControlled = ES3.Load<bool>($"{savedKey}.isPlayerControlled", filename);

                // 기본 선원 객체 생성
                crew = CreateCrewInstance(race, crewName, isPlayerControlled);

                // 추가 속성 로드 및 적용
                if (ES3.KeyExists($"{savedKey}.health", filename))
                    crew.health = ES3.Load<float>($"{savedKey}.health", filename);

                if (ES3.KeyExists($"{savedKey}.skills", filename))
                    crew.skills = ES3.Load<Dictionary<SkillType, float>>($"{savedKey}.skills", filename);

                // 필요한 다른 속성 로드...
            }

            // 스프라이트 등 필요한 리소스 로드
            SpriteRenderer spriteRenderer = crew.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = Resources.Load<Sprite>($"Sprites/Crew/{crew.race.ToString().ToLower()}");
                spriteRenderer.sortingOrder = Constants.SortingOrders.Character;
            }

            return crew;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"선원 복원 중 오류 발생: {e.Message}");
            return null;
        }
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
