using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class CrewBase : MonoBehaviour
{
    [Header("Basic Info")] public string crewName; // 이름
    public bool isPlayerControlled; // 아군(true) ? 적군(false) ?

    [Header("Crew Stats")] public CrewRaceStat crewRaceStat;

    [Header("Details")] public CrewRace race; // 종족
    public float maxHealth; // 최대 체력
    public float attack; // 공격력
    public float defense; // 방어력
    public float learningSpeed; // 학습 속도
    public bool needsOxygen; // 산소 호흡 여부

    // 숙련도
    [Header("Skill Values")] public Dictionary<SkillType, float> maxSkillValueArray = new(); // 최대 숙련도 배열

    public float maxPilotSkillValue,
        maxEngineSkillValue,
        maxPowerSkillValue,
        maxShieldSkillValue,
        maxWeaponSkillValue,
        maxAmmunitionSkillValue,
        maxMedBaySkillValue,
        maxRepairSkillValue;

    public Dictionary<SkillType, float> skills = new(); // 선원 기본 숙련도
    public Dictionary<SkillType, float> equipAdditionalSkills = new(); // 장비로 인한 추가 숙련도

    // 착용 장비
    [Header("Equipped Items")] public EquipmentItem equippedWeapon;
    public EquipmentItem equippedShield;
    public EquipmentItem equippedAssistant;

    [Header("Location")] public Room currentRoom;
    public Vector2 position;
    public Vector2 targetPosition;
    public float moveSpeed = 2.0f;

    [Header("Status")] public float health; // 현재 체력
    public CrewStatus status; // 현재 상태 (부상 등)
    public bool isAlive; // 생존 여부
    public bool isMoving;

    private float timeSinceLastSkillIncrease = 0f;
    private float skillIncreaseInterval = 10f; // 10초마다 스킬 증가 체크

    public Ship currentShip;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        // 이동 처리
        if (isMoving)
            UpdateMovement();

        // 스킬 증가 체크
        timeSinceLastSkillIncrease += Time.deltaTime;
        if (timeSinceLastSkillIncrease >= skillIncreaseInterval)
        {
            timeSinceLastSkillIncrease = 0f;
            CheckSkillImprovement(); // 숙련도 증가
        }

        // 산소 농도 체크 - 체력 감소
        CheckOxygenStatus();
    }

    public void Initialize()
    {
        maxHealth = crewRaceStat.ByRace[race].maxHealth;
        attack = crewRaceStat.ByRace[race].attack;
        defense = crewRaceStat.ByRace[race].defense;
        learningSpeed = crewRaceStat.ByRace[race].learningSpeed;
        needsOxygen = crewRaceStat.ByRace[race].needsOxygen;

        health = maxHealth;
        status = CrewStatus.Normal;
        isAlive = true;
        isMoving = false;

        maxSkillValueArray = crewRaceStat.ByRace[race].GetMaxSkillValueDictionary();
        maxPilotSkillValue = maxSkillValueArray[SkillType.PilotSkill];
        maxEngineSkillValue = maxSkillValueArray[SkillType.EngineSkill];
        maxPowerSkillValue = maxSkillValueArray[SkillType.PowerSkill];
        maxShieldSkillValue = maxSkillValueArray[SkillType.ShieldSkill];
        maxWeaponSkillValue = maxSkillValueArray[SkillType.WeaponSkill];
        maxAmmunitionSkillValue = maxSkillValueArray[SkillType.AmmunitionSkill];
        maxMedBaySkillValue = maxSkillValueArray[SkillType.MedBaySkill];
        maxRepairSkillValue = maxSkillValueArray[SkillType.RepairSkill];

        skills[SkillType.PilotSkill] = crewRaceStat.ByRace[race].initialPilotSkill;
        skills[SkillType.EngineSkill] = crewRaceStat.ByRace[race].initialEngineSkill;
        skills[SkillType.PowerSkill] = crewRaceStat.ByRace[race].initialPowerSkill;
        skills[SkillType.ShieldSkill] = crewRaceStat.ByRace[race].initialShieldSkill;
        skills[SkillType.WeaponSkill] = crewRaceStat.ByRace[race].initialWeaponSkill;
        skills[SkillType.AmmunitionSkill] = crewRaceStat.ByRace[race].initialAmmunitionSkill;
        skills[SkillType.MedBaySkill] = crewRaceStat.ByRace[race].initialMedBaySkill;
        skills[SkillType.RepairSkill] = crewRaceStat.ByRace[race].initialRepairSkill;

        equipAdditionalSkills[SkillType.PilotSkill] = 0.0f;
        equipAdditionalSkills[SkillType.EngineSkill] = 0.0f;
        equipAdditionalSkills[SkillType.PowerSkill] = 0.0f;
        equipAdditionalSkills[SkillType.ShieldSkill] = 0.0f;
        equipAdditionalSkills[SkillType.WeaponSkill] = 0.0f;
        equipAdditionalSkills[SkillType.AmmunitionSkill] = 0.0f;
        equipAdditionalSkills[SkillType.MedBaySkill] = 0.0f;
        equipAdditionalSkills[SkillType.RepairSkill] = 0.0f;
    }


    private void UpdateMovement()
    {
        // 목표 위치로 이동
        Vector2 direction = (targetPosition - position).normalized;
        position += direction * moveSpeed * Time.deltaTime;

        // 도착 확인
        if (Vector2.Distance(position, targetPosition) < 0.1f)
        {
            position = targetPosition;
            isMoving = false;

            // 새 방에 도착한 경우 처리
            if (currentRoom != null)
                currentRoom.OnCrewEnter(this);
        }
    }

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
        switch (currentRoom.roomType)
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

    private void CheckOxygenStatus()
    {
        if (!needsOxygen)
            return;

        // 현재 방의 산소 레벨 확인
        if (currentRoom != null)
        {
            OxygenLevel roomOxygen = currentRoom.GetOxygenLevel();

            // 산소 부족 시 데미지
            if (roomOxygen == OxygenLevel.None)
                GetDamage(10f * Time.deltaTime); // 산소 없음: 초당 10 데미지
            else if (roomOxygen == OxygenLevel.Critical)
                GetDamage(5f * Time.deltaTime); // 위험 수준: 초당 5 데미지
        }
    }

    // 숙련도 증가
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

    // 방 이동
    public void MoveToRoom(Room targetRoom, Vector2 roomPosition)
    {
        // 현재 방에서 나감
        if (currentRoom != null)
            currentRoom.OnCrewExit(this);

        // 새 방으로 설정
        currentRoom = targetRoom;

        // 이동 시작
        targetPosition = roomPosition;
        isMoving = true;
    }

    // 전투 관련
    // 공격
    public void Attack(CrewMember target)
    {
        // 공격 가하는 crew의 공격력 인자로 넘김
        float measuredAttack = (float)Math.Round(attack, 2);
        target.GetDamage(measuredAttack);
    }

    // 데미지 처리 - ocAttack : opponent Crew Attack (공격 가하는 crew의 공격력 + 장비)
    public void GetDamage(float ocAttack)
    {
        // 방어력 적용 - 최종 피해량
        float measuredDefense = (float)Math.Round(defense, 2);
        float receivedDamage = ocAttack * (100.0f - measuredDefense) / 100.0f;
        receivedDamage = (float)Math.Round(receivedDamage, 2); // 소수점 셋째자리에서 반올림
        health -= receivedDamage;

        // 사망 체크
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    // 선원 사망
    private void Die()
    {
        isAlive = false;

        // 현재 방에서 제거
        if (currentRoom != null)
            currentRoom.OnCrewExit(this);

        // TODO : 임시로 작동되게 해놓음.

        if (currentShip.GetAllCrew().Contains(this)) currentShip.GetAllCrew().Remove(this);

        // 아래는 원래 코드
        /*
         *  // 사망 이벤트 발생 등 추가 처리
        if (CrewManager.Instance.crewList.Contains(this))
        {
            CrewManager.Instance.crewList.Remove(this); // 해당 선원 찾아 제외
            CrewManager.Instance.RefreshCrewList(CrewManager.Instance.crewList.Count,
                CrewManager.Instance.maxCrewCount); // 총 선원 수 갱신
        }

         *
         */


        // 사망 처리 - 0.5초 후 사라짐
        Destroy(gameObject, 0.5f);

        Debug.Log($"{crewName} 사망 처리 완료");
    }

    // 수리 작업
    public void RepairFacility(Room room, float amount)
    {
        // 수리 스킬에 따른 수리량 계산
        // float repairSkillBonus = skills.ContainsKey(SkillType.RepairSkill) ? skills[SkillType.RepairSkill] / 100f : 0f;
        float repairAmount = amount;

        // 수리 실행
        room.Repair(repairAmount);

        // 수리 스킬 향상
        ImproveSkill(SkillType.RepairSkill, 0.5f);
    }

    // 체력 회복
    public void Heal(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
    }

    // 치료 필요 여부
    public bool NeedsHealing()
    {
        return health < maxHealth;
    }

    // 숙련도 넘기기 - 사기 (morale) 추가 필요
    public virtual Dictionary<SkillType, float> GetCrewSkillValue()
    {
        Dictionary<SkillType, float> totalSkills = new();


        return totalSkills;
    }

    // 장비 효과 계산
    public void RecalculateEquipmentBonus(EquipmentItem item, bool isAdding)
    {
        float sign = isAdding ? 1f : -1f;

        attack += sign * item.eqAttackBonus;
        defense += sign * item.eqDefenseBonus;
        maxHealth += sign * item.eqHealthBonus;
        health += sign * item.eqHealthBonus;

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

    // 현재 착용중인 장비 호출
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

    // 장비 착용
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

    // 장비 교체
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

    // 개인 별 장비 착용
    public void ApplyPersonalEquipment(EquipmentItem eqItem)
    {
        if (eqItem == null)
            return;

        SwapEquipment(eqItem); // 기존 장비 해제, 효과 해제 -> 새 장비 착용, 효과 적용

        Debug.Log($"{crewName}에 개인 장비 {eqItem.eqName} 적용 완료.");
    }

    // 스킬 레벨 가져오기
    public float GetSkillLevel(SkillType skill)
    {
        if (skills.ContainsKey(skill))
            return skills[skill];
        return 0f;
    }


    // 전투 관련
    public bool IsEnemyOf(CrewMember other)
    {
        return isPlayerControlled != other.isPlayerControlled;
    }
}
