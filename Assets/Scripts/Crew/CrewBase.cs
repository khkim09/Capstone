using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 모든 선원의 베이스 클래스.
/// 종족, 체력, 장비, 스킬, 이동, 전투, 스탯 기여 등 다양한 기능을 포함합니다.
/// </summary>
public abstract class CrewBase : MonoBehaviour, IShipStatContributor
{
    /// <summary>선원 이름.</summary>
    [Header("Basic Info")] public string crewName; // 이름

    /// <summary>플레이어 소속 여부 (true = 아군, false = 적군).</summary>
    public bool isPlayerControlled;

    /// <summary>선원의 종족.</summary>
    [Header("Details")] public CrewRace race;

    /// <summary>최대 체력.</summary>
    public float maxHealth;

    /// <summary>공격력.</summary>
    public float attack;

    /// <summary>방어력.</summary>
    public float defense;

    /// <summary>스킬 학습 속도.</summary>
    public float learningSpeed;

    /// <summary>산소가 필요한 종족 여부.</summary>
    public bool needsOxygen;

    /// <summary>
    /// 스킬 타입별 최대 숙련도 값 딕셔너리.
    /// 종족별 초기값에 따라 설정됩니다.
    /// </summary>
    [Header("Skill Values")] public Dictionary<SkillType, float> maxSkillValueArray = new();

    /// <summary>조종(Pilot) 스킬 최대 숙련도.</summary>
    public float maxPilotSkillValue;

    /// <summary>엔진(Engine) 스킬 최대 숙련도.</summary>
    public float maxEngineSkillValue;

    /// <summary>전력(Power) 스킬 최대 숙련도.</summary>
    public float maxPowerSkillValue;

    /// <summary>방어막(Shield) 스킬 최대 숙련도.</summary>
    public float maxShieldSkillValue;

    /// <summary>무기(Weapon) 스킬 최대 숙련도.</summary>
    public float maxWeaponSkillValue;

    /// <summary>탄약(Ammunition) 스킬 최대 숙련도.</summary>
    public float maxAmmunitionSkillValue;

    /// <summary>의무실(MedBay) 스킬 최대 숙련도.</summary>
    public float maxMedBaySkillValue;

    /// <summary>수리(Repair) 스킬 최대 숙련도.</summary>
    public float maxRepairSkillValue;

    /// <summary>
    /// 현재 스킬 레벨을 나타내는 딕셔너리.
    /// 레벨은 실제 사용 중인 스킬 수치를 의미하며, 학습을 통해 증가합니다.
    /// </summary>
    public Dictionary<SkillType, float> skills = new();

    /// <summary>
    /// 장비 장착에 의해 추가된 보너스 스킬 값.
    /// 기본 스킬과는 별도로 합산되어 사용됩니다.
    /// </summary>
    public Dictionary<SkillType, float> equipAdditionalSkills = new();

    /// <summary>착용 중인 무기 장비.</summary>
    [Header("Equipped Items")] public EquipmentItem equippedWeapon;

    /// <summary>착용 중인 무기 장비.</summary>
    public EquipmentItem equippedShield;

    /// <summary>착용 중인 어시스턴트 장비 (스킬 보조).</summary>
    [CanBeNull] public EquipmentItem equippedAssistant;

    /// <summary>현재 위치한 방.</summary>
    [Header("Location")] public Room currentRoom;

    /// <summary>현재 좌표 (월드 또는 로컬 좌표계 기반).</summary>
    public Vector2Int position;

    /// <summary>이동 예정 좌표 (월드 또는 로컬 좌표계 기반).</summary>
    public Vector2Int targetPosition;

    /// <summary>이동 속도 (초당 거리).</summary>
    public float moveSpeed = 3.0f;

    /// <summary>현재 체력.</summary>
    [Header("Status")] public float health;

    /// <summary>생존 여부 (false = 사망).</summary>
    public bool isAlive;

    /// <summary>이동 중 여부.</summary>
    public bool isMoving;

    /// <summary>마지막 스킬 상승 이후 경과 시간.</summary>
    private float timeSinceLastSkillIncrease = 0f;

    /// <summary>스킬 상승 주기 (초 단위).</summary>
    private float skillIncreaseInterval = 10f; // 10초마다 스킬 증가 체크

    /// <summary>현재 선원이 속한 함선 참조.</summary>
    public Ship currentShip;

    /// <summary>스프라이트 렌더러.</summary>
    public SpriteRenderer spriteRenderer;

    /// <summary>
    /// 정면샷 (crewInfoPanel, crewDetailPanel 에서 선원 정면 Sprite 필요)
    /// </summary>
    public Sprite portraitSprite;

    /// <summary>
    /// 애니메이션 작동 Animator
    /// </summary>
    public Animator animator;

    /// <summary>
    /// 선원 체력 바
    /// </summary>
    [Header("Health Bar")]
    [SerializeField]
    public CrewHealthBar healthBarController;

    /// <summary>
    /// 해당 선원을 때리고 있는 선원 리스트
    /// </summary>
    public HashSet<CrewMember> bullier = new();

    [SerializeField] private List<Sprite> allRaceSprites;

    [SerializeField] private List<AnimatorController> allRaceAnimations;


    public virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = Constants.SortingOrders.Character;
        //spriteRenderer.sprite = Resources.Load<Sprite>($"Sprites/Crew/{race.ToString().ToLower()}");
        switch (race)
        {
            case CrewRace.Amorphous:
                spriteRenderer.sprite = allRaceSprites[0];
                break;
            case CrewRace.Beast:
                spriteRenderer.sprite = allRaceSprites[1];
                break;
            case CrewRace.Human:
                spriteRenderer.sprite = allRaceSprites[2];
                break;
            case CrewRace.Insect:
                spriteRenderer.sprite = allRaceSprites[3];
                break;
            case CrewRace.MechanicSup:
                spriteRenderer.sprite = allRaceSprites[4];
                break;
            case CrewRace.MechanicTank:
                spriteRenderer.sprite = allRaceSprites[5];
                break;
        }

        spriteRenderer.material = Resources.Load<Material>("Material/UnitDefaultMaterial");

        InitializePortrait();

        // animator.runtimeAnimatorController =
        //     Resources.Load<RuntimeAnimatorController>($"Animation/{race.ToString()}/{race.ToString()}");

        // 종족별 애니메이터 연결
        if (animator == null)
        {
            animator = gameObject.GetComponent<Animator>();
            switch (race)
            {
                case CrewRace.Amorphous:
                    animator.runtimeAnimatorController = allRaceAnimations[0];
                    break;
                case CrewRace.Beast:
                    animator.runtimeAnimatorController = allRaceAnimations[1];
                    break;
                case CrewRace.Human:
                    animator.runtimeAnimatorController = allRaceAnimations[2];
                    break;
                case CrewRace.Insect:
                    animator.runtimeAnimatorController = allRaceAnimations[3];
                    break;
                case CrewRace.MechanicSup:
                    animator.runtimeAnimatorController = allRaceAnimations[4];
                    break;
                case CrewRace.MechanicTank:
                    animator.runtimeAnimatorController = allRaceAnimations[5];
                    break;
            }


            Debug.Log("여기 호출됨");
        }

        SetupHealthBar();
    }

    /// <summary>
    /// Unity 생명주기 메서드.
    /// 매 프레임 호출되어 이동 처리, 스킬 성장 체크, 산소 부족 데미지를 처리합니다.
    /// </summary>
    private void Update()
    {
        // 이동 처리
        // if (isMoving)
        //     UpdateMovement();

        // 스킬 증가 체크
        timeSinceLastSkillIncrease += Time.deltaTime;
        if (timeSinceLastSkillIncrease >= skillIncreaseInterval)
        {
            timeSinceLastSkillIncrease = 0f;
            CheckSkillImprovement(); // 숙련도 증가
        }

        // 산소 농도 체크 - 체력 감소
        ApplyOxygenDamage();
    }

    /// <summary>
    /// 초기 정면샷 저장
    /// </summary>
    private void InitializePortrait()
    {
        portraitSprite = spriteRenderer.sprite;
    }

    /// <summary>
    /// 선원이 현재 위치한 방에 따라 관련 스킬을 자동 향상시킵니다.
    /// </summary>
    // 시설 숙련도 증가 (기획서 27p 참고) - 수정 필요
    private void CheckSkillImprovement()
    {
        // 현재 수행 중인 작업에 따라 관련 스킬 증가
        if (currentRoom == null)
            return;

        // 기본값 세팅
        SkillType skillToImprove = SkillType.None; // 숙련도 타입
        float skillIncreaseAmount = 0.0f; // 증가량

        // 방 타입에 따라 향상될 스킬 결정
        switch (currentRoom.GetRoomType())
        {
            case RoomType.Cockpit:
                skillToImprove = SkillType.PilotSkill;
                skillIncreaseAmount = 0.1f;
                break;
            case RoomType.Engine:
                skillToImprove = SkillType.EngineSkill;
                skillIncreaseAmount = 0.1f;
                break;
            case RoomType.Power:
                skillToImprove = SkillType.PowerSkill;
                skillIncreaseAmount = 0.1f;
                break;
            case RoomType.Shield:
                skillToImprove = SkillType.ShieldSkill;
                skillIncreaseAmount = 0.1f;
                break;
            case RoomType.WeaponControl:
                skillToImprove = SkillType.WeaponSkill;
                skillIncreaseAmount = 0.1f;
                break;
            case RoomType.Ammunition:
                skillToImprove = SkillType.AmmunitionSkill;
                skillIncreaseAmount = 0.1f;
                break;
            case RoomType.MedBay:
                skillToImprove = SkillType.MedBaySkill;
                skillIncreaseAmount = 0.1f;
                break;
        }

        // 학습 속도에 따라 스킬 증가
        if (learningSpeed > 0)
            ImproveSkill(skillToImprove, skillIncreaseAmount * learningSpeed);
    }

    /// <summary>
    /// 산소가 필요한 종족일 경우, 산소 부족 시 체력 데미지를 적용합니다.
    /// </summary>
    private void ApplyOxygenDamage()
    {
        if (!needsOxygen)
            return;

        if (currentShip != null)
            if (currentShip.GetOxygenLevel() == OxygenLevel.None)
                gameObject.GetComponent<CrewMember>().TakeDamage(maxHealth * 0.01f); // 최대 체력의 1%만큼 데미지
    }

    /// <summary>
    /// 특정 스킬을 주어진 수치만큼 향상시키며 최대치를 넘지 않도록 제한합니다.
    /// </summary>
    /// <param name="skill">향상시킬 스킬 종류.</param>
    /// <param name="amount">향상할 수치.</param>
    public void ImproveSkill(SkillType skill, float amount)
    {
        if (!skills.ContainsKey(skill))
            skills[skill] = 0f;

        float maxSkillValue = 0.0f;
        switch (skill)
        {
            case SkillType.PilotSkill:
                maxSkillValue = maxPilotSkillValue;
                break;
            case SkillType.EngineSkill:
                maxSkillValue = maxEngineSkillValue;
                break;
            case SkillType.PowerSkill:
                maxSkillValue = maxPowerSkillValue;
                break;
            case SkillType.ShieldSkill:
                maxSkillValue = maxShieldSkillValue;
                break;
            case SkillType.WeaponSkill:
                maxSkillValue = maxWeaponSkillValue;
                break;
            case SkillType.AmmunitionSkill:
                maxSkillValue = maxAmmunitionSkillValue;
                break;
            case SkillType.MedBaySkill:
                maxSkillValue = maxMedBaySkillValue;
                break;
            case SkillType.RepairSkill:
                maxSkillValue = maxRepairSkillValue;
                break;
        }

        // 스킬 향상
        skills[skill] = Mathf.Min(skills[skill] + amount, maxSkillValue);
    }

    /// <summary>
    /// 선원의 숙련도를 반환합니다. 추후 사기 수치 등도 반영 예정입니다.
    /// </summary>
    /// <returns>스킬 타입별 현재 숙련도 딕셔너리.</returns>
    public virtual Dictionary<SkillType, float> GetCrewSkillValue()
    {
        Dictionary<SkillType, float> totalSkills = new();

        return totalSkills;
    }

    #region 장비

    /// <summary>
    /// 장비 효과를 스탯에 반영하거나 제거합니다.
    /// </summary>
    /// <param name="item">적용할 장비.</param>
    /// <param name="isAdding">true면 적용, false면 해제.</param>
    public void RecalculateEquipmentBonus(EquipmentItem item, bool isAdding)
    {
        float sign = isAdding ? 1f : -1f;

        attack += sign * item.eqAttackBonus;
        defense += sign * item.eqDefenseBonus;
        float oldMaxHealth = maxHealth;

        maxHealth += sign * item.eqHealthBonus;
        health += sign * item.eqHealthBonus;

        if (healthBarController != null && oldMaxHealth != maxHealth)
        {
            healthBarController.UpdateMaxHealth(maxHealth);
            healthBarController.UpdateHealth(health);
        }

        // Assistant일 경우 숙련도 보너스
        if (item.eqType == EquipmentType.AssistantEquipment)
        {
            equipAdditionalSkills[SkillType.PilotSkill] += sign * item.eqAdditionalPilotSkill;
            equipAdditionalSkills[SkillType.EngineSkill] += sign * item.eqAdditionalEngineSkill;
            equipAdditionalSkills[SkillType.PowerSkill] += sign * item.eqAdditionalPowerSkill;
            equipAdditionalSkills[SkillType.ShieldSkill] += sign * item.eqAdditionalShieldSkill;
            equipAdditionalSkills[SkillType.WeaponSkill] += sign * item.eqAdditionalWeaponSkill;
            equipAdditionalSkills[SkillType.AmmunitionSkill] += sign * item.eqAdditionalAmmunitionSkill;
            equipAdditionalSkills[SkillType.MedBaySkill] += sign * item.eqAdditionalMedBaySkill;
            equipAdditionalSkills[SkillType.RepairSkill] += sign * item.eqAdditionalRepairSkill;
        }
    }

    /// <summary>
    /// 현재 장착 중인 장비를 반환합니다.
    /// </summary>
    /// <param name="type">장비 타입.</param>
    /// <returns>장착된 장비 또는 null.</returns>
    private EquipmentItem GetEquippedItem(EquipmentType type)
    {
        return type switch
        {
            EquipmentType.WeaponEquipment => equippedWeapon,
            EquipmentType.ShieldEquipment => equippedShield,
            EquipmentType.AssistantEquipment => equippedAssistant,
            _ => null
        };
    }

    /// <summary>
    /// 장비를 지정된 슬롯에 장착합니다.
    /// </summary>
    /// <param name="type">장비 타입.</param>
    /// <param name="newItem">장착할 장비.</param>
    private void SetEquippedItem(EquipmentType type, EquipmentItem newItem)
    {
        switch (type)
        {
            case EquipmentType.WeaponEquipment:
                equippedWeapon = newItem;
                break;
            case EquipmentType.ShieldEquipment:
                equippedShield = newItem;
                break;
            case EquipmentType.AssistantEquipment:
                equippedAssistant = newItem;
                break;
        }
    }

    /// <summary>
    /// 기존 장비를 해제하고 새 장비를 장착합니다.
    /// </summary>
    /// <param name="newItem">장착할 새 장비.</param>
    public void SwapEquipment(EquipmentItem newItem)
    {
        if (newItem == null)
            return;

        // 기본 장비 호출
        EquipmentItem oldItem = GetEquippedItem(newItem.eqType);

        // 기존 장비 해제
        if (oldItem != null)
            RecalculateEquipmentBonus(oldItem, false);

        // 새 장비 장착
        SetEquippedItem(newItem.eqType, newItem);

        // 새 장비 효과 적용
        RecalculateEquipmentBonus(newItem, true);
    }

    /// <summary>
    /// 개인 장비를 착용합니다. 기존 장비는 해제되며 효과도 반영됩니다.
    /// </summary>
    /// <param name="eqItem">장착할 장비.</param>
    public void ApplyPersonalEquipment(EquipmentItem eqItem)
    {
        if (eqItem == null)
            return;

        SwapEquipment(eqItem); // 기존 장비 해제, 효과 해제 -> 새 장비 착용, 효과 적용

        Debug.Log($"{crewName}에 개인 장비 {eqItem.eqName} 적용 완료.");
    }

    #endregion

    /// <summary>
    /// 특정 스킬의 현재 레벨을 반환합니다.
    /// </summary>
    /// <param name="skill">조회할 스킬.</param>
    /// <returns>현재 스킬 레벨.</returns>
    public float GetSkillLevel(SkillType skill)
    {
        if (skills.ContainsKey(skill))
            return skills[skill];
        return 0f;
    }

    /// <summary>
    /// 다른 선원이 적대적인 대상인지 확인합니다.
    /// </summary>
    /// <param name="other">다른 선원.</param>
    /// <returns>적이면 true.</returns>
    public bool IsEnemyOf(CrewMember other)
    {
        return isPlayerControlled != other.isPlayerControlled;
    }

    /// <summary>
    /// 선원이 함선 스탯에 기여하는 수치를 반환합니다.
    /// 예: 산소 소모량 등.
    /// </summary>
    public Dictionary<ShipStat, float> GetStatContributions()
    {
        Dictionary<ShipStat, float> contributions = new();

        // 죽어있으면 해당 안됨
        if (!isAlive) return contributions;

        // 산소 호흡을 하는 종이면 산소 1초에 1% 소모
        if (needsOxygen) contributions[ShipStat.OxygenUsingPerSecond] = 1.0f;

        return contributions;
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    /// <summary>
    /// 다른 CrewBase 객체의 데이터를 복사합니다.
    /// </summary>
    /// <param name="other">복사할 대상.</param>
    public void CopyFrom(CrewBase other)
    {
        if (other == null) return;

        crewName = other.crewName;
        isPlayerControlled = other.isPlayerControlled;
        race = other.race;
        maxHealth = other.maxHealth;
        attack = other.attack;
        defense = other.defense;
        learningSpeed = other.learningSpeed;
        needsOxygen = other.needsOxygen;

        // 딥 카피 - Dictionary는 참조형이라 새로 할당
        maxSkillValueArray = new Dictionary<SkillType, float>(other.maxSkillValueArray);
        maxPilotSkillValue = other.maxPilotSkillValue;
        maxEngineSkillValue = other.maxEngineSkillValue;
        maxPowerSkillValue = other.maxPowerSkillValue;
        maxShieldSkillValue = other.maxShieldSkillValue;
        maxWeaponSkillValue = other.maxWeaponSkillValue;
        maxAmmunitionSkillValue = other.maxAmmunitionSkillValue;
        maxMedBaySkillValue = other.maxMedBaySkillValue;
        maxRepairSkillValue = other.maxRepairSkillValue;

        skills = new Dictionary<SkillType, float>(other.skills);
        equipAdditionalSkills = new Dictionary<SkillType, float>(other.equipAdditionalSkills);

        // 장비는 참조 복사 (필요 시 DeepCopy로 변경 가능)
        equippedWeapon = other.equippedWeapon;
        equippedShield = other.equippedShield;
        equippedAssistant = other.equippedAssistant;

        currentRoom = other.currentRoom;
        position = other.position;
        targetPosition = other.targetPosition;
        moveSpeed = other.moveSpeed;
        health = other.health;
        isAlive = other.isAlive;
        isMoving = other.isMoving;
        currentShip = other.currentShip;
    }

    // 체력바 설정 메서드 (새로 추가)
    private void SetupHealthBar()
    {
        // 자식 오브젝트에서 HealthBarController 찾기
        healthBarController = GetComponentInChildren<CrewHealthBar>();

        if (healthBarController != null)
            // 초기 체력바 설정
            healthBarController.UpdateHealth(health);
        else
            Debug.LogWarning($"{crewName}: HealthBarController를 찾을 수 없습니다. 체력바를 설정하세요.");
    }
}
