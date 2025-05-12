using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 선원의 데이터를 확장하여 실제 게임 내 선원(Crew)의 기능을 담당하는 클래스입니다.
/// </summary>
[Serializable]
public class CrewMember : CrewBase
{
    /// <summary>
    /// 현재 이동 방향 (normalized)
    /// </summary>
    private Vector2 movementDirection;

    /// <summary>이동 경로</summary>
    private List<Vector2Int> path;

    /// <summary>
    /// 이동 수행 코루틴
    /// </summary>
    private Coroutine moveCoroutine;

    /// <summary>
    /// 최종 목적지 방
    /// </summary>
    public Room reservedRoom;

    /// <summary>
    /// 예약했지만 취소한 목적지 방
    /// </summary>
    public Room oldReservedRoom;

    /// <summary>
    /// 새로운 최종 목적지 타일
    /// </summary>
    public Vector2Int reservedTile;

    /// <summary>
    /// 이동 중 새로운 이동 명령 내리기 전 예약했던 목적지 타일 (최종으로 예약했지만 취소한 목적지 타일)
    /// </summary>
    public Vector2Int oldReservedTile;

    /// <summary>
    /// 이동 전 위치했던 타일
    /// </summary>
    public Vector2Int originPosTile;

    public List<CrewEffect> effectfs;

    /// <summary>
    /// Unity 생명주기 메서드.
    /// 매 프레임마다 선원 상태를 갱신합니다. (현재는 구현 미완료)
    /// </summary>
    private void Update()
    {
    }

    /// <summary>
    /// 현재 선원의 모든 스킬 값을 계산하여 반환합니다.
    /// 기본 숙련도, 장비 보너스, 사기 보너스를 포함합니다.
    /// </summary>
    /// <returns>스킬 타입별 총 숙련도 값 딕셔너리.</returns>
    public virtual Dictionary<SkillType, float> GetCrewSkillValue()
    {
        Dictionary<SkillType, float> totalSkills = new();

        foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
        {
            if (skill == SkillType.None)
                continue;

            float baseSkill = skills.ContainsKey(skill) ? skills[skill] : 0f;
            float equipmentBonus = equipAdditionalSkills.ContainsKey(skill) ? equipAdditionalSkills[skill] : 0f;
            float moraleBonus = MoraleManager.Instance.GetTotalMoraleBonus(this); // 향후 보완 예정 -> morale manager 생성

            float total = baseSkill + equipmentBonus + moraleBonus;
            totalSkills[skill] = total;
        }

        return totalSkills;
    }

    /// <summary>
    /// 현재 위치 반환 (가장 가까운 타일)
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetCurrentTile()
    {
        return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    /// <summary>
    /// 이동 경로를 명시하고 이동 시작
    /// 이동 시작 시 기존 점유 타일 해제, 새로운 목적지 타일 점유 등록
    /// </summary>
    /// <param name="path"></param>
    public void AssignPathAndMove(List<Vector2Int> newPath)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        // 점유 타일 해제
        ExitCurrentTile();

        // 목적지 예약
        ReserveDestination();

        // 3. 경로 설정 후 이동 시작
        path = newPath;
        moveCoroutine = StartCoroutine(FollowPathCoroutine());
    }

    /// <summary>
    /// 이동 도중 새로운 목적지로 변경할 때 체력바도 함께 따라가도록
    /// </summary>
    public void CancelAndRedirect(List<Vector2Int> newPath)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        // 이전 목적지 타일 점유 해제
        ExitReserveTile();

        // 새로운 목적지 예약
        ReserveDestination();

        // 체력바가 부모를 따라 이동하도록 보장
        EnsureHealthBarFollows();

        // 새로운 경로 이동
        path = newPath;
        moveCoroutine = StartCoroutine(FollowPathCoroutine());
    }

    /// <summary>
    /// 체력바가 선원을 제대로 따라가도록 확인
    /// </summary>
    private void EnsureHealthBarFollows()
    {
        CrewHealthBar healthBar = GetComponentInChildren<CrewHealthBar>();
        if (healthBar != null)
            // 체력바의 부모가 다른 방으로 설정된 경우 선원 오브젝트의 자식으로 다시 설정
            if (healthBar.transform.parent.parent != transform)
                healthBar.transform.parent.SetParent(transform);
    }

    /// <summary>
    /// 선원이 방을 바꿀 때 체력바가 함께 이동하도록
    /// </summary>
    private void ReserveDestination()
    {
        if (reservedRoom != null)
        {
            Debug.LogError($"최종 예약된 타일 pos : {reservedTile}");
            reservedRoom.OccupyTile(reservedTile);
            reservedRoom.OnCrewEnter(this);
            currentRoom = reservedRoom;
            RTSSelectionManager.Instance.playerShip.MarkCrewTileOccupied(reservedRoom, reservedTile);

            // 선원 자체의 부모는 방으로 설정하지만, 체력바는 선원의 자식으로 유지
            transform.SetParent(reservedRoom.transform);
            EnsureHealthBarFollows();
        }
    }

    /// <summary>
    /// 정지 상태에서 이동 명령 - 기존 점유 타일 해제, 방에서 나옴
    /// </summary>
    private void ExitCurrentTile()
    {
        if (currentRoom != null)
        {
            Debug.LogError($"기존 위치하던 pos : {GetCurrentTile()}");
            Vector2Int currentTile = GetCurrentTile();
            currentRoom.VacateTile(currentTile);
            currentRoom.OnCrewExit(this);
            RTSSelectionManager.Instance.playerShip.UnmarkCrewTile(currentRoom, currentTile);
            transform.SetParent(null);
        }
    }

    /// <summary>
    /// 움직이던 도중 새로운 이동 명령 - 예약 목적지 타일 점유 해제, 방에서 나옴
    /// </summary>
    private void ExitReserveTile()
    {
        if (oldReservedRoom != null)
        {
            Debug.LogError($"최종 예약했지만 취소하는 타일 pos : {oldReservedTile}");
            oldReservedRoom.VacateTile(oldReservedTile);
            oldReservedRoom.OnCrewExit(this);
            RTSSelectionManager.Instance.playerShip.UnmarkCrewTile(oldReservedRoom, oldReservedTile);
            transform.SetParent(null);
        }
    }


    /// <summary>
    /// 이동 animation
    /// </summary>
    /// <param name="trigger"></param>
    private void PlayAnimation(string trigger)
    {
        if (trigger.Equals("walk"))
        {
            animator.SetFloat("X", movementDirection.x);
            animator.SetFloat("Y", movementDirection.y);
        }

        animator.SetBool(trigger, isMoving);
    }

    /// <summary>
    /// 현재 방에서 선원이 점유 중인 타일의 우선순위 인덱스 반환
    /// </summary>
    /// <returns></returns>
    public int GetCurrentTilePriorityIndex()
    {
        if (currentRoom == null)
            return -1;

        List<Vector2Int> priorityList = currentRoom.GetRotatedCrewEntryGridPriority();
        return priorityList.IndexOf(GetCurrentTile());
    }

    /// <summary>
    /// 경로를 따라 이동하는 코루틴
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerator FollowPathCoroutine()
    {
        isMoving = true;

        foreach (Vector2Int tile in path)
        {
            Vector3 targetWorldPos = GridToWorldPosition(tile);

            // 이동 중인 방향
            movementDirection = (targetWorldPos - transform.position).normalized;
            PlayAnimation("walk");
            while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
            {
                float speedMultiplier = 1f;

                // 현재 방 체크
                Room room = RTSSelectionManager.Instance.playerShip.GetRoomAtPosition(GetCurrentTile());

                // 복도는 이동 속도 30% 증가
                if (room != null && room.GetRoomType() == RoomType.Corridor)
                    speedMultiplier = 1.33f;

                // MoveTowards() : 다른 선원과의 충돌 무시하고 통과
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos,
                    moveSpeed * speedMultiplier * Time.deltaTime);
                yield return null;
            }

            // 선원 위치 갱신
            transform.position = targetWorldPos;
            position = new Vector2Int(tile.x, tile.y);
        }

        isMoving = false;

        // 도착 후 방 갱신
        currentRoom = reservedRoom;
        reservedRoom = null;

        PlayAnimation("walk");

        // 이동 완료한 위치에서 함내 전투 검사
        // oninvoke()로 해결
    }

    // <summary>
    /// 월드 좌표를 그리드 좌표로 변환합니다.
    /// </summary>
    public Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        Vector3 local = new Vector3(worldPos.x, worldPos.y, 0) - Vector3.zero;
        return new Vector2Int(Mathf.FloorToInt(local.x / GridConstants.CELL_SIZE),
            Mathf.FloorToInt(local.y / GridConstants.CELL_SIZE));
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표로 변환
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return Vector3.zero + new Vector3((gridPos.x + 0.5f) * GridConstants.CELL_SIZE,
            (gridPos.y + 0.5f) * GridConstants.CELL_SIZE, 0f);
    }

    /// <summary>
    /// 현재 선원의 스텟 반환
    /// </summary>
    /// <returns></returns>
    public EquipmentStats GetCombinedStats()
    {
        EquipmentStats total = new();

        total.health = health;
        total.attack = attack;
        total.defense = defense;
        total.pilotSkill = skills[SkillType.PilotSkill];
        total.engineSkill = skills[SkillType.EngineSkill];
        total.powerSkill = skills[SkillType.PowerSkill];
        total.shieldSkill = skills[SkillType.ShieldSkill];
        total.weaponSkill = skills[SkillType.WeaponSkill];
        total.ammunitionSkill = skills[SkillType.AmmunitionSkill];
        total.medbaySkill = skills[SkillType.MedBaySkill];
        total.repairSkill = skills[SkillType.RepairSkill];

        return total;
    }

    /// <summary>
    /// 새로운 장비 착용 시뮬레이션
    /// </summary>
    /// <param name="newItem"></param>
    /// <returns></returns>
    public EquipmentStats GetStatsIfEquipped(EquipmentItem newItem)
    {
        // 현재 스텟 복사
        EquipmentStats result = GetCombinedStats();

        // 새 장비 타입에 따라 기존 장비 효과 제거 후 새 장비 효과 적용 시뮬레이션
        if (newItem.eqType == EquipmentType.WeaponEquipment)
        {
            if (equippedWeapon != null)
            {
                result.health -= equippedWeapon.eqHealthBonus;
                result.attack -= equippedWeapon.eqAttackBonus;
                result.defense -= equippedWeapon.eqDefenseBonus;
            }

            result.health += newItem.eqHealthBonus;
            result.attack += newItem.eqAttackBonus;
            result.defense += newItem.eqDefenseBonus;
        }
        else if (newItem.eqType == EquipmentType.ShieldEquipment)
        {
            if (equippedShield != null)
            {
                result.health -= equippedShield.eqHealthBonus;
                result.attack -= equippedShield.eqAttackBonus;
                result.defense -= equippedShield.eqDefenseBonus;
            }

            result.health += newItem.eqHealthBonus;
            result.attack += newItem.eqAttackBonus;
            result.defense += newItem.eqDefenseBonus;
        }
        else if (newItem.eqType == EquipmentType.AssistantEquipment)
        {
            if (equippedAssistant != null)
            {
                result.pilotSkill -= equippedAssistant.eqAdditionalPilotSkill;
                result.engineSkill -= equippedAssistant.eqAdditionalEngineSkill;
                result.powerSkill -= equippedAssistant.eqAdditionalPowerSkill;
                result.shieldSkill -= equippedAssistant.eqAdditionalShieldSkill;
                result.weaponSkill -= equippedAssistant.eqAdditionalWeaponSkill;
                result.ammunitionSkill -= equippedAssistant.eqAdditionalAmmunitionSkill;
                result.medbaySkill -= equippedAssistant.eqAdditionalMedBaySkill;
                result.repairSkill -= equippedAssistant.eqAdditionalRepairSkill;
            }

            result.pilotSkill += newItem.eqAdditionalPilotSkill;
            result.engineSkill += newItem.eqAdditionalEngineSkill;
            result.powerSkill += newItem.eqAdditionalPowerSkill;
            result.shieldSkill += newItem.eqAdditionalShieldSkill;
            result.weaponSkill += newItem.eqAdditionalWeaponSkill;
            result.ammunitionSkill += newItem.eqAdditionalAmmunitionSkill;
            result.medbaySkill += newItem.eqAdditionalMedBaySkill;
            result.repairSkill += newItem.eqAdditionalRepairSkill;
        }

        return result;
    }
}
