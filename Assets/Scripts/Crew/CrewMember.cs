using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public enum CrewRace
{
    None = 0,
    Human = 1,
    Amorphous = 2,
    MechanicTank = 3,
    MechanicSup = 4,
    Beast = 5,
    Insect = 6
}

public enum SkillType
{
    Piloting, // 조종
    Engineering, // 엔진
    Weapons, // 무기
    Shields, // 방어막
    Repairs, // 수리
    Medical, // 의료
    Combat // 전투
}


[Serializable]
public class CrewMember : MonoBehaviour
{
    // 기본 정보
    public string crewName; // 이름
    public float health; // 현재 체력
    public CrewStatus status; // 현재 상태 (부상 등)
    public bool isAlive = true; // 생존 여부

    // 디테일 정보
    public CrewRace race; // 종족
    public float maxHealth; // 최대 체력
    public float attack; // 공격력
    public float defense; // 방어력
    public float morale = 50f; // 사기 (0-100)

    [Header("Skills")] public Dictionary<SkillType, float> skills = new();
    public float learningSpeed = 1.0f; // 학습 속도

    [FormerlySerializedAs("currentRoomBase")] [Header("Location")]
    public Room currentRoom;

    public Vector2 position;
    public Vector2 targetPosition;
    public float moveSpeed = 2.0f;

    [Header("Combat")] public float attackPower = 10f;
    public float defensePower = 10f;

    [Header("Status")] public bool needsOxygen = true; // 산소 필요 여부
    public bool isMoving = false;

    private float timeSinceLastSkillIncrease = 0f;
    private float skillIncreaseInterval = 10f; // 10초마다 스킬 증가 체크

    /*
    // 장비
    public string workAssistant; // 작업 보조 장비
    public string weapon; // 무기 장비
    public string armor; // 방어구 장비
*/


    private void Awake()
    {
        // 기본 스킬 초기화
        InitializeSkills();
    }

    private void InitializeSkills()
    {
        // 종족별 기본 스킬 설정
        switch (race)
        {
            case CrewRace.Human:
                skills[SkillType.Piloting] = 80f;
                skills[SkillType.Engineering] = 100f;
                skills[SkillType.Weapons] = 80f;
                skills[SkillType.Shields] = 80f;
                skills[SkillType.Repairs] = 100f;
                skills[SkillType.Medical] = 80f;
                skills[SkillType.Combat] = 80f;
                learningSpeed = 1.5f;
                needsOxygen = true;
                break;

            case CrewRace.Amorphous:
                skills[SkillType.Piloting] = 100f;
                skills[SkillType.Engineering] = 80f;
                skills[SkillType.Weapons] = 100f;
                skills[SkillType.Shields] = 80f;
                skills[SkillType.Repairs] = 80f;
                skills[SkillType.Medical] = 100f;
                skills[SkillType.Combat] = 60f;
                learningSpeed = 0.8f;
                needsOxygen = false;
                break;
            case CrewRace.MechanicTank:
                skills[SkillType.Piloting] = 0f;
                skills[SkillType.Engineering] = 0f;
                skills[SkillType.Weapons] = 120f;
                skills[SkillType.Shields] = 0f;
                skills[SkillType.Repairs] = 0f;
                skills[SkillType.Medical] = 0f;
                skills[SkillType.Combat] = 120f;
                learningSpeed = 0f; // 학습 불가
                needsOxygen = false;
                break;
            case CrewRace.MechanicSup:
                skills[SkillType.Piloting] = 100f;
                skills[SkillType.Engineering] = 100f;
                skills[SkillType.Weapons] = 0f;
                skills[SkillType.Shields] = 0f;
                skills[SkillType.Repairs] = 120f;
                skills[SkillType.Medical] = 120f;
                skills[SkillType.Combat] = 0f;
                learningSpeed = 0f; // 학습 불가
                needsOxygen = false;
                break;
            case CrewRace.Beast:
                skills[SkillType.Piloting] = 50f;
                skills[SkillType.Engineering] = 40f;
                skills[SkillType.Weapons] = 60f;
                skills[SkillType.Shields] = 50f;
                skills[SkillType.Repairs] = 40f;
                skills[SkillType.Medical] = 40f;
                skills[SkillType.Combat] = 100f;
                learningSpeed = 0.8f;
                needsOxygen = true;
                break;
            case CrewRace.Insect:
                skills[SkillType.Piloting] = 100f;
                skills[SkillType.Engineering] = 50f;
                skills[SkillType.Weapons] = 100f;
                skills[SkillType.Shields] = 50f;
                skills[SkillType.Repairs] = 50f;
                skills[SkillType.Medical] = 80f;
                skills[SkillType.Combat] = 80f;
                learningSpeed = 1.0f;
                needsOxygen = true;
                break;
            default:
                // 기본 스킬 설정
                foreach (SkillType skill in Enum.GetValues(typeof(SkillType))) skills[skill] = 50f;
                break;
        }

        // 종족별 기본 능력치 설정
        SetRaceAttributes();
    }

    private void SetRaceAttributes()
    {
        switch (race)
        {
            case CrewRace.Human:
                maxHealth = 100f;
                attackPower = 8f;
                defensePower = 8f;
                break;

            case CrewRace.Amorphous:
                maxHealth = 100f;
                attackPower = 9f;
                defensePower = 6f;
                break;

            case CrewRace.MechanicTank:
                maxHealth = 120f;
                attackPower = 14f;
                defensePower = 10f;
                break;
            case CrewRace.MechanicSup:
                maxHealth = 70f;
                attackPower = 5f;
                defensePower = 5f;
                break;
            case CrewRace.Beast:
                maxHealth = 120f;
                attackPower = 12f;
                defensePower = 12f;
                break;

            case CrewRace.Insect:
                maxHealth = 120f;
                attackPower = 10f;
                defensePower = 15f;
                break;
        }

        health = maxHealth;
    }

    private void Update()
    {
        // 이동 처리
        if (isMoving) UpdateMovement();

        // 스킬 증가 체크
        timeSinceLastSkillIncrease += Time.deltaTime;
        if (timeSinceLastSkillIncrease >= skillIncreaseInterval)
        {
            timeSinceLastSkillIncrease = 0f;
            CheckSkillImprovement();
        }

        // 산소 체크
        CheckOxygenStatus();
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
            if (currentRoom != null) currentRoom.OnCrewEnter(this);
        }
    }

    private void CheckSkillImprovement()
    {
        // 현재 수행 중인 작업에 따라 관련 스킬 증가
        if (currentRoom == null) return;

        SkillType skillToImprove = SkillType.Repairs; // 기본값

        // 방 타입에 따라 향상될 스킬 결정
        switch (currentRoom.roomType)
        {
            case RoomType.Cockpit:
                skillToImprove = SkillType.Piloting;
                break;
            case RoomType.Engine:
                skillToImprove = SkillType.Engineering;
                break;
            case RoomType.Weapon:
                skillToImprove = SkillType.Weapons;
                break;
            case RoomType.Shield:
                skillToImprove = SkillType.Shields;
                break;
            case RoomType.MedBay:
                skillToImprove = SkillType.Medical;
                break;
            // 다른 방 타입들...
        }

        // 학습 속도에 따라 스킬 증가
        if (learningSpeed > 0) ImproveSkill(skillToImprove, 0.1f * learningSpeed);
    }

    private void CheckOxygenStatus()
    {
        if (!needsOxygen) return; // 산소가 필요 없는 종족

        // 현재 방의 산소 레벨 확인
        if (currentRoom != null)
        {
            OxygenLevel roomOxygen = currentRoom.GetOxygenLevel();

            // 산소 부족 시 데미지
            if (roomOxygen == OxygenLevel.None)
                TakeDamage(10f * Time.deltaTime); // 산소 없음: 초당 10 데미지
            else if (roomOxygen == OxygenLevel.Critical) TakeDamage(5f * Time.deltaTime); // 위험 수준: 초당 5 데미지
        }
    }

    // 스킬 향상
    public void ImproveSkill(SkillType skill, float amount)
    {
        if (!skills.ContainsKey(skill)) skills[skill] = 0f;

        // 스킬 최대치 설정 (종족별로 다름)
        float maxSkill = 120f;
        if (race == CrewRace.Human)
            maxSkill = 120f;
        else if (race == CrewRace.Amorphous) maxSkill = 150f;

        // 스킬 향상
        skills[skill] = Mathf.Min(skills[skill] + amount, maxSkill);
    }

    // 방 이동
    public void MoveToRoom(Room targetRoom, Vector2 roomPosition)
    {
        // 현재 방에서 나감
        if (currentRoom != null) currentRoom.OnCrewExit(this);

        // 새 방으로 설정
        currentRoom = targetRoom;

        // 이동 시작
        targetPosition = roomPosition;
        isMoving = true;
    }

    // 데미지 처리
    public void TakeDamage(float damage)
    {
        // 방어력 적용
        float reducedDamage = damage * (100f - defensePower) / 100f;
        health -= reducedDamage;

        // 사망 체크
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    // 체력 회복
    public void Heal(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
    }

    // 선원 사망
    private void Die()
    {
        // 사망 처리
        gameObject.SetActive(false);

        // 현재 방에서 제거
        if (currentRoom != null) currentRoom.OnCrewExit(this);

        // 사망 이벤트 발생 등 추가 처리
    }

    // 사기 보너스 적용
    public void AddMoraleBonus(float bonus)
    {
        morale += bonus;
        morale = Mathf.Clamp(morale, 0f, 100f);
    }

    // 공격
    public void Attack(CrewMember target)
    {
        // 공격력 + 전투 스킬 보너스로 데미지 계산
        float combatSkillBonus = skills.ContainsKey(SkillType.Combat) ? skills[SkillType.Combat] / 100f : 0f;
        float damage = attackPower * (1f + combatSkillBonus);

        // 상대방에게 데미지
        target.TakeDamage(damage);

        // 전투 스킬 향상
        ImproveSkill(SkillType.Combat, 0.2f);
    }

    // 수리 작업
    public void RepairFacility(Room room, float amount)
    {
        // 수리 스킬에 따른 수리량 계산
        float repairSkillBonus = skills.ContainsKey(SkillType.Repairs) ? skills[SkillType.Repairs] / 100f : 0f;
        float repairAmount = amount * (1f + repairSkillBonus);

        // 수리 실행
        room.Repair(repairAmount);

        // 수리 스킬 향상
        ImproveSkill(SkillType.Repairs, 0.2f);
    }

    // 치료 필요 여부
    public bool NeedsHealing()
    {
        return health < maxHealth;
    }

    // 스킬 레벨 가져오기
    public float GetSkillLevel(SkillType skill)
    {
        if (skills.ContainsKey(skill)) return skills[skill];
        return 0f;
    }
}
