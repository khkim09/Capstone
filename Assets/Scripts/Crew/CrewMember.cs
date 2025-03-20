using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

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
    None,
    PilotSkill, // 조종
    EngineSkill, // 엔진
    PowerSkill, // 전력
    ShieldSkill, // 배리어
    WeaponSkill, // 조준
    AmmunitionSkill, // 탄약
    MedBaySkill, // 의무
    RepairSkill, // 수리
}


[Serializable]
public class CrewMember : MonoBehaviour
{
    // 기본 정보
    [Header("Basic Info")]
    public string crewName; // 이름

    // 디테일 정보
    [Header("Details")]
    public CrewRace race; // 종족
    public float maxHealth; // 최대 체력
    public float attack; // 공격력
    public float defense; // 방어력
    public float learningSpeed; // 학습 속도
    public bool needsOxygen; // 산소 호흡 여부

    // 숙련도
    [Header("Skill Values")]
    public float[] maxSkillValueArray = new float[8]; // 최대 숙련도 배열
    public float maxPilotSkillValue, maxEngineSkillValue, maxPowerSkillValue, maxShieldSkillValue, maxWeaponSkillValue, maxAmmunitionSkillValue, maxMedBaySkillValue, maxRepairSkillValue;
    public Dictionary<SkillType, float> skills = new Dictionary<SkillType, float>();

    // 장비
    [Header("Equipments")]
    public float allCrewEquipment = 0.0f; // 선원 전체 적용 장비
    public float ownEquipment = 0.0f; // 개인 적용 장비

    // 이하 - 다른 파트와의 연결 필요
    public float morale = 50f; // 사기 (0-100)

    [Header("Location")]
    public Room currentRoom;
    public Vector2 position;
    public Vector2 targetPosition;
    public float moveSpeed = 2.0f;

    [Header("Status")]
    public float health; // 현재 체력
    public CrewStatus status; // 현재 상태 (부상 등)
    public bool isAlive; // 생존 여부
    public bool isMoving;

    private float timeSinceLastSkillIncrease = 0f;
    private float skillIncreaseInterval = 10f; // 10초마다 스킬 증가 체크

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
            case RoomType.Weapon:
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

    // 스킬 향상
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

    // 공격
    public void Attack(CrewMember target)
    {
        // 공격 가하는 crew의 공격력 인자로 넘김
        float measuredAttack = attack + allCrewEquipment + ownEquipment; // 기본 공격력 + 장비
        measuredAttack = (float)Math.Round(measuredAttack, 2);
        target.GetDamage(measuredAttack);
    }

    // 데미지 처리 - ocAttack : opponent Crew Attack (공격 가하는 crew의 공격력 + 장비)
    public void GetDamage(float ocAttack)
    {
        // 방어력 적용 - 최종 피해량
        float receivedDamage = ocAttack * (100.0f - (defense + allCrewEquipment + ownEquipment)) / 100.0f;
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
        // 사망 처리 - 0.5초 후 사라짐
        Destroy(gameObject, 0.5f);

        // 현재 방에서 제거
        if (currentRoom != null)
            currentRoom.OnCrewExit(this);

        // 사망 이벤트 발생 등 추가 처리
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

    // 개인 장비 적용 - 보조 장비비
    public void AddAssistantEquipment(EquipmentItem eqItem)
    {
        if (eqItem.eqType != EquipmentType.AssistantEquipment)
        {
            Debug.LogWarning("보조 장비비 아님");
            return;
        }

        // 보조 장치 효과
        ownEquipment = 0.0f; // 기존 착용 보조 장비 해제
        // ownEquipment += 1.0f; (수정 필요)
    }

    // 사기 보너스 적용
    public void AddMoraleBonus(float bonus)
    {
        morale += bonus;
        morale = Mathf.Clamp(morale, 0f, 100f);
    }

    // 스킬 레벨 가져오기
    public float GetSkillLevel(SkillType skill)
    {
        if (skills.ContainsKey(skill))
            return skills[skill];
        return 0f;
    }
}
