using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
    /// 새로운 이동 명령 전 기존 목적지 방
    /// </summary>
    public Room oldReservedRoom;

    /// <summary>
    /// 새로운 최종 목적지 타일
    /// </summary>
    public Vector2Int reservedTile;

    /// <summary>
    /// 새로운 이동 명령 전 기존 목적지 타일 (예약 됐다 취소된..)
    /// </summary>
    public Vector2Int oldReservedTile;

    /// <summary>
    /// 이동 전 위치했던 타일
    /// </summary>
    public Vector2Int originPosTile;

    /// <summary>
    /// 움직이는 중이었다.
    /// </summary>
    private bool wasMoving = false;

    /// <summary>
    /// 텔레포트 방에 입장 여부 (텔포 준비 완료 여부)
    /// </summary>
    public bool hasEnteredTPRoom = false;

    /// <summary>
    /// Unity 생명주기 메서드.
    /// 매 프레임마다 선원 상태를 갱신합니다. (현재는 구현 미완료)
    /// </summary>
    private void Update()
    {
    }

    /// <summary>
    /// Scriptable Object로 수정 필요!!
    /// </summary>
    public override void Start()
    {
        base.Start();

        switch (race)
        {
            case CrewRace.Amorphous:
                attackBeforeDelay = 0.72f;
                attackAfterDelay = 0.27f;
                repairBeforeDelay = 0.572f;
                repairAfterDelay = 0.428f;
                break;
            case CrewRace.Beast:
                attackBeforeDelay = 0.57f;
                attackAfterDelay = 0.43f;
                repairBeforeDelay = 0.5f;
                repairAfterDelay = 0.5f;
                break;
            case CrewRace.Human:
                attackBeforeDelay = 0.72f;
                attackAfterDelay = 0.27f;
                repairBeforeDelay = 0.375f;
                repairAfterDelay = 0.625f;
                break;
            case CrewRace.Insect:
                attackBeforeDelay = 0.6f;
                attackAfterDelay = 0.4f;
                repairBeforeDelay = 0.6f;
                repairAfterDelay = 0.4f;
                break;
            case CrewRace.MechanicSup:
                repairBeforeDelay = 0.4f;
                repairAfterDelay = 0.6f;
                break;
            case CrewRace.MechanicTank:
                attackBeforeDelay = 0.66f;
                attackAfterDelay = 0.34f;
                break;
        }
    }

    /// <summary>
    /// 현재 선원의 모든 스킬 값을 계산하여 반환합니다.
    /// 기본 숙련도, 장비 보너스, 사기 보너스를 포함합니다.
    /// </summary>
    /// <returns>스킬 타입별 총 숙련도 값 딕셔너리.</returns>
    public override Dictionary<SkillType, float> GetCrewSkillValue()
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
            totalSkills[skill] = total / 100;
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

        // 1. 점유 타일 해제
        if (currentRoom != null)
            CrewReservationManager.ExitTile(currentShip, currentRoom, GetCurrentTile(), this);

        // 2. 목적지 예약
        if (reservedRoom != null)
        {
            CrewReservationManager.ReserveTile(currentShip, reservedRoom, reservedTile, this);
            currentRoom = reservedRoom;
        }

        // 3. 체력바
        EnsureHealthBarFollows();

        // 4. 경로 설정 후 이동 시작
        path = newPath;
        moveCoroutine = StartCoroutine(FollowPathCoroutine());
    }

    /// <summary>
    /// 이동 중 새로운 이동 명령 수신 시 이전 목적지 타일 점유 해제 (체력바 follow)
    /// </summary>
    /// <param name="newPath"></param>
    public void CancelAndRedirect(List<Vector2Int> newPath)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        // 1. 이전 목적지 타일 점유 해제
        if (oldReservedRoom != null)
            CrewReservationManager.ExitTile(currentShip, oldReservedRoom, oldReservedTile, this);

        // 2. 새로운 목적지 예약
        if (reservedRoom != null)
        {
            CrewReservationManager.ReserveTile(currentShip, reservedRoom, reservedTile, this);
            currentRoom = reservedRoom;
        }

        // 3. 체력바가 부모를 따라 이동하도록 보장
        EnsureHealthBarFollows();

        // 4. 새로운 경로 이동
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

        DontTouchMe();

        foreach (Vector2Int tile in path)
        {
            Vector3 targetWorldPos = currentShip.GetWorldPositionFromGrid(tile);

            // 이동 중인 방향
            movementDirection = (targetWorldPos - transform.position).normalized;
            PlayAnimation("walk");

            while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
            {
                float speedMultiplier = 1f;

                // 현재 방 체크
                Room room = currentShip.GetRoomAtPosition(GetCurrentTile());

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

        PlayAnimation("walk");

        // 도착 후 방 갱신
        currentRoom = reservedRoom;

        // 도착 후 방 진입 표시
        if (currentRoom != null)
        {
            currentRoom.OnCrewEnter(this);

            // 테스트 코드
            currentRoom.occupyingTiles.Add(new Room.ot { crewMember = this, tile = this.reservedTile });
        }

        Debug.Log($"현재 방: {currentRoom}");
        reservedRoom = null;

        // 본인 함선의 텔포 방일 경우, 즉시 텔포 수행
        if (hasEnteredTPRoom && currentRoom.roomType == RoomType.Teleporter)
        {
            hasEnteredTPRoom = false;

            if (SceneManager.GetActiveScene().name == "Combat")
            {
                // 텔포 준비
                Freeze();

                StartCoroutine(TeleportAfterDelay(this, 0.5f));
            }

            yield break;
        }


        // 도착한 방에서 적군 탐지
        if (isWithEnemy() && inCombat == false)
        {
            Debug.Log("적 있음");
            // 이동 중이던 선원이 우선적으로 마저 이동 (적군을 찾아감)
            RTSSelectionManager.Instance.MoveForCombat(this);

            // 적군 찾아가는 동작 완료 후
            // 전투 중이지 않거나 이동 중이지 않은 적들에 대해 본인 공격하도록 어그로
            LookAtMe();
        }
        else // 도착한 방에 적군이 없음
        {
            Debug.Log("적없음");
            // 내 배가 아닐 경우 부수는 행동 개시
            if (!IsOwnShip())
                combatCoroutine = StartCoroutine(CombatRoutine());
            else
            {
                // 적이 없고 유저 함선이라 부술 방도 없을 경우, Idle 상태를 거쳐 수리로 연결
                BackToThePeace();
            }
        }
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

    //----------애니메이션------------

    /// <summary>
    /// animation 실행
    /// </summary>
    /// <param name="trigger"></param>
    public void PlayAnimation(string trigger)
    {
        if (trigger.Equals("walk"))
        {
            SetAnimationDirection(movementDirection);
            animator.SetBool(trigger, isMoving);
        }
        else if (trigger.Equals("attack"))
        {
            SetAnimationDirection(movementDirection);
            animator.SetTrigger("attack");
        }
        else if (trigger.Equals("die"))
        {
            SetAnimationDirection(movementDirection);
            animator.SetTrigger("die");
        }
        else if (trigger.Equals("repair"))
        {
            SetAnimationDirection(Vector2.down);
            animator.SetTrigger("repair");
        }
        else if (trigger.Equals("work"))
        {
            animator.SetBool(trigger, isWorking);
        }
        else if (trigger.Equals("idle"))
        {
            animator.SetTrigger("idle");
        }
        else if (trigger.Equals("tp_out"))
            animator.SetTrigger("tp_out");
        else if (trigger.Equals("tp_in"))
            animator.SetTrigger("tp_in");
    }

    /// <summary>
    /// 애니메이션 방향 설정
    /// </summary>
    /// <param name="direction"></param>
    private void SetAnimationDirection(Vector2 direction)
    {
        animator.SetFloat("X", direction.x);
        animator.SetFloat("Y", direction.y);
    }

    //----------전투---------

    /// <summary>
    /// 전투 대상
    /// </summary>
    public CrewMember combatTarget { set; get; }

    /// <summary>
    /// 전투 상태 여부
    /// </summary>
    public bool inCombat = false;

    /// <summary>
    /// 애니메이션 발동 타이밍과 실제 데미지 적용 사이의 딜레이
    /// </summary>
    private float attackBeforeDelay = 1f;

    private float attackAfterDelay = 1f;

    /// <summary>
    /// 전투 코루틴
    /// </summary>
    public Coroutine combatCoroutine = null;

    /// <summary>
    /// 사망 코루틴
    /// </summary>
    public Coroutine DieCoroutine = null;

    /// <summary>
    /// 파괴할 방
    /// </summary>
    public Room madRoom;

    /// <summary>
    /// 전투 진입 코루틴 (타겟 설정 후 전투 시작, 타겟이 없고 적 함선일 경우 방 부수기 돌입)
    /// </summary>
    /// <returns></returns>
    public IEnumerator CombatRoutine()
    {
        // 때릴 적군 없음 -> 정지 (본인 함선) or 방 부수기 (적 함선)
        if (combatTarget == null || !combatTarget.isAlive)
        {
            // 본인 함선이면 break (그 자리에서 정지)
            if (IsOwnShip())
            {
                inCombat = false;
                combatCoroutine = null;
                yield break;
            }

            // 일단 부수기 방 설정 = 현재 방
            madRoom = currentRoom;
            if (madRoom == null || !madRoom.GetIsDamageable() || madRoom.currentHitPoints <= 0) // 부술 수 없는 방
            {
                yield break;
            }

            // 방 부수기 진입 시 true
            inCombat = true;
        }
        else // 때릴 적군 찾음
        {
            inCombat = true;
            combatTarget.bullier.Add(this); // 적군을 때리는 선원 리스트에 이 선원 추가

            // 때릴 적군이 전투 중이 아닌 상태 - 나랑 전투하도록
            if (!combatTarget.inCombat && currentRoom == combatTarget.currentRoom)
            {
                combatTarget.Freeze();
                combatTarget.combatTarget = this;
                combatTarget.combatCoroutine = StartCoroutine(combatTarget.CombatRoutine());
            }

            // 때릴 방향
            movementDirection = combatTarget.GetCurrentTile() - GetCurrentTile();
        }

        PlayAnimation("attack");
    }

    /// <summary>
    /// CombatRoutine()은 1회 공격 -> 반복해라
    /// </summary>
    public void RepeatCombatRoutine()
    {
        if (madRoom == null && combatTarget == null)
        {
            if (combatCoroutine != null)
            {
                StopCoroutine(combatCoroutine);
                combatCoroutine = null;
            }
        }// 여기 맞지?
        else
            combatCoroutine = StartCoroutine(CombatRoutine());
    }

    /// <summary>
    /// 현재 착용 중인 장비들의 공격력 수치 합계
    /// </summary>
    /// <returns></returns>
    private float GetEquipmentAttack()
    {
        float total = 0;
        if (equippedShield != null)
            total += equippedShield.eqAttackBonus;
        if (equippedWeapon != null)
            total += equippedWeapon.eqAttackBonus;
        return total;
    }

    /// <summary>
    /// 현재 착용 중인 장비들의 방어력 수치 합계
    /// </summary>
    /// <returns></returns>
    private float GetEquipmentDefense()
    {
        float total = 0;
        if (equippedShield != null)
            total += equippedShield.eqDefenseBonus;
        if (equippedWeapon != null)
            total += equippedWeapon.eqDefenseBonus;
        return total;
    }

    /// <summary>
    /// 전투 메서드: 1 hit 당 피해량 계산 후 체력 차감, 체력이 0 이하이면 죽음 처리
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    /// <returns>
    ///공격한 상대가 살아있는지 여부를 반환한다.<br/>
    /// True: 아직 살아있음 / False: 죽었음
    /// </returns>
    public void Attack()
    {
        // 피해량 계산식: (공격 주체 기본 공격 + 장비 공격력(tmp)) * (1 - (상대 방어력 / 100))
        float damage = attack + GetEquipmentAttack(); //(attacker.attack + attacker.equippedWeapon.eqAttackBonus) * (1 - target.defense / 100f);
        if (combatTarget == null)
        {
            //인자로 받는 target이 combatRoutine에서 현재 적군을 찾을 수 없는 경우에는 null로 전달되기 때문에 하위 분기에서 시설 파괴로 진행
            if (madRoom == null)
            {
                //목표를 포착했...는 중이다
                inCombat = false;
                if (isWithEnemy())
                {
                    RTSSelectionManager.Instance.MoveForCombat(this);
                }
                return;
            }

            currentShip.TakeDamage(damage);
            madRoom.TakeDamage(damage);
            if (madRoom.currentHitPoints <= 0)
            {
                madRoom = null;
                inCombat = false;
            }

            return;
        }

        Debug.Log($"{this.crewName}이(가) {combatTarget.crewName}에게 {damage}의 피해를 입혔습니다.");
        combatTarget.TakeDamage(damage);
    }

    /// <summary>
    /// 현재 선원의 방어력을 고려한 피해 발생
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(float damage)
    {
        float realDamage = damage * (1 - (defense + GetEquipmentDefense()) / 100f);
        health -= realDamage;

        // if (healthBarController != null)
        //     healthBarController.UpdateHealth(health);

        if (health <= 0)
            Die();
    }

    /// <summary>
    /// 사망 시, 자신을 combatTarget으로 지정한 선원들에게서 자신을 할당 해제시키고 다른 목표를 탐색하도록한다.
    /// 또한 애니메이션 재생을 마친 후에 Destroy
    /// </summary>
    public void Die()
    {
        wasMoving = isMoving;
        isMoving = false;
        inCombat = false;
        isAlive = false;
        isWorking = false;

        StopAllCrewCoroutine();
        DontTouchMe();

        if (currentRoom.workingCrew == this) WalkOut();

        if (DieCoroutine == null)
        {
            // 이거 해결해야 될 듯
            if (RTSSelectionManager.Instance.selectedCrew.Contains(this))
                RTSSelectionManager.Instance.selectedCrew.RemoveAll(cm => cm == null || cm == this);

            if (currentShip.allCrews.Contains(this))
                currentShip.allCrews.Remove(this);
            if (currentShip.allEnemies.Contains(this))
                currentShip.allEnemies.Remove(this);

            GameEvents.PirateKilled();
            DieCoroutine = StartCoroutine(ImDying());
        }
    }

    /// <summary>
    /// 전투 상황에서 자신이 이동했을 때, 자신을 공격하던 다른 선원들에게 자신을 공격하는 행위 이외의 다른 행동을 하도록 명령
    /// </summary>
    public void DontTouchMe()
    {
        if (inCombat)
        {
            inCombat = false;
            if(combatCoroutine!=null) StopCoroutine(combatCoroutine);
            combatCoroutine = null;
            madRoom = null;
            combatTarget = null;
        }

        if (isWorking)
        {
            Debug.Log("작업 중이었던");
            WalkOut();
            Debug.Log("작업 선원 할당 해제");
        }

        if (bullier.Count > 0)
        {
            List<CrewMember> bulling = new(bullier);
            foreach (CrewMember hittingMan in bulling)
            {
                hittingMan.inCombat = false;
                hittingMan.combatTarget = null;
                if (combatCoroutine != null)
                    StopCoroutine(hittingMan.combatCoroutine);
                hittingMan.combatCoroutine = null;
                hittingMan.bullier.Remove(this);
                if (hittingMan.isWithEnemy())
                {
                    RTSSelectionManager.Instance.MoveForCombat(hittingMan);
                }
                else
                {
                    hittingMan.BackToThePeace();
                }
            }
            bullier.Clear();
        }
    }

    /// <summary>
    /// 죽음 처리
    /// </summary>
    /// <returns></returns>
    public IEnumerator ImDying()
    {
        PlayAnimation("die");
        yield return new WaitForSeconds(1.5f);

        if (currentRoom != null)
        {
            if (!wasMoving)
                CrewReservationManager.ExitTile(currentShip, currentRoom, GetCurrentTile(), this);
            else
                CrewReservationManager.ExitTile(currentShip, reservedRoom, reservedTile, this);
        }

        Destroy(gameObject);
        // object pulling -> 죽은 선원 집합소 생성 (setparent())

        Debug.Log($"{crewName} 사망 처리 완료");
    }

    /// <summary>
    /// 선원이 진입한 방의 적이 멈춰 있고 전투 중이 아니라면, 모두 본인을 공격하도록 설정 (어그로)
    /// </summary>
    public void LookAtMe()
    {
        Debug.Log("나를 바라봐");
        List<CrewMember> others = new(currentRoom.GetTotalCrewsInRoom());
        foreach (CrewMember other in others)
        {
            Debug.Log($"검색 선원 : {other.race}");
            if (other == this)
            {
                Debug.Log($"{other.race} 나야");
                continue;
            }

            if (other.isPlayerControlled == isPlayerControlled)
            {
                Debug.Log($"{other.race} 아군이야");
                continue;
            }

            if ((other.combatTarget == null || other.madRoom == null) && other.isMoving == false)
            {
                // 방에 있는 상대를 나에게 이동
                Debug.Log($"{other.race}야, {race}한테 와");
                other.WalkOut();
                RTSSelectionManager.Instance.MoveForCombat(other);
            }
        }
    }

    /// <summary>
    /// Idle 상태 이후
    /// </summary>
    public void BackToThePeace()
    {
        if (isAlive && !inCombat && !isMoving)
        {
            StopAllCrewCoroutine();

            PlayAnimation("idle");

            // 본인 함선일 경우 수리, 작업 시도 및 진입
            if (IsOwnShip())
            {
                if (currentRoom.NeedsRepair())
                    TryRepair();
                else
                    TryWork();
            }
        }
    }

    /// <summary>
    /// 방 안의 적군 탐지
    /// </summary>
    /// <returns></returns>
    public bool isWithEnemy()
    {
        foreach (CrewMember someone in currentRoom.GetTotalCrewsInRoom())
        {
            if (someone.isPlayerControlled != isPlayerControlled && someone.isAlive)
            {
                Debug.Log($"방안 적 찾음 {someone.race}");
                return true;
            }
        }
        Debug.Log("방안에 적 못 찾음");
        return false;
    }


    #region 수리

    /// <summary>
    /// 수리 코루틴
    /// </summary>
    public Coroutine repairCoroutine;

    /// <summary>
    /// 수리 효율
    /// </summary>
    public float repairCoefficient = 10;

    /// <summary>
    /// 수리 간 딜레이
    /// </summary>
    public float repairBeforeDelay = 0.5f;

    public float repairAfterDelay = 0.5f;

    /// <summary>
    /// 본인 함선에서 수리 시도
    /// </summary>
    public void TryRepair()
    {
        if (!IsOwnShip())
            return;
        Dictionary<SkillType, float> crewSkill = GetCrewSkillValue();
        if (!crewSkill.ContainsKey(SkillType.RepairSkill))
        {
            TryWork();
            return;
        }

        if (repairCoroutine == null)
        {
            repairCoroutine = StartCoroutine(RepairRoutine());
        }
        else // 수리 중인 코루틴이 있는데 왜 할당 해제함?
        {
            StopCoroutine(repairCoroutine);
            repairCoroutine = null;
        }
    }

    /// <summary>
    /// 수리 루틴
    /// </summary>
    /// <returns></returns>
    public IEnumerator RepairRoutine()
    {
        // 전투 개시 or 수리 필요 없는 상황
        if (isWithEnemy() || !currentRoom.NeedsRepair())
        {
            repairCoroutine = null;
            yield break;
        }

        PlayAnimation("repair");
        yield return new WaitForSeconds(repairBeforeDelay);

        // 실제 수리 실행
        RepairFacility(currentRoom);
        //todo: 수리 숙련도 적용시켜야됨
        yield return new WaitForSeconds(repairAfterDelay);

        if (currentRoom.NeedsRepair())
            repairCoroutine = StartCoroutine(RepairRoutine());
        else
        {
            repairCoroutine = null;
            TryWork();
        }
    }

    /// <summary>
    /// 특정 방을 수리하며 수리 스킬을 향상시킵니다.
    /// </summary>
    /// <param name="room">수리 대상 방.</param>
    /// <param name="amount">수리량.</param>
    public void RepairFacility(Room room)
    {
        // 수리 스킬에 따른 수리량 계산
        // float repairSkillBonus = skills.ContainsKey(SkillType.RepairSkill) ? skills[SkillType.RepairSkill] / 100f : 0f;

        //TODO:장비 완성되면 장비와 사기, 숙련도에 대한 보너스 추가 필요
        float repairAmount = repairCoefficient * GetCrewSkillValue()[SkillType.RepairSkill];

        // 수리 실행
        room.Repair(repairAmount);

        // 수리 스킬 향상
        ImproveSkill(SkillType.RepairSkill, 0.5f);
    }

    #endregion


    #region 시설 작업

    /// <summary>
    /// 현재 작업 중인지 여부
    /// </summary>
    public bool isWorking = false;

    /// <summary>
    /// 주변 적들의 유무와 현재 자신의 위치한 타일, 작업 가능한 방인지 여부에 따라 작업 행동을 시도한다.
    /// 선원의 작업 숙련도 유무와 숙련도 적용은 Room.CanITouch에서 작동된다.
    /// </summary>
    public void TryWork()
    {
        if (currentRoom.isActive && currentRoom.workDirection != Vector2Int.zero)
        {
            if (!isWithEnemy())
            {
                if (GetCurrentTilePriorityIndex() == 0)
                {
                    if (currentRoom.CanITouch(this))
                    {
                        isWorking = true;
                        PlayAnimation("work");

                        // 작업 애니메이션 방향 (방 회전 방향 적용)
                        List<Vector2Int> directions = new()
                        {
                            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
                        };
                        Vector2Int workDir = directions[(directions.IndexOf(currentRoom.workDirection) + (int)currentRoom.currentRotation) % 4];

                        if (!isPlayerControlled)
                            workDir = directions[(directions.IndexOf(currentRoom.workDirection) + (int)currentRoom.currentRotation + 2) % 4];

                        SetAnimationDirection(workDir);
                    }
                }
            }
        }
    }

    #endregion

    #region 치유

    /// <summary>
    /// 선원의 체력을 회복합니다. 최대 체력을 초과하지 않습니다.
    /// </summary>
    /// <param name="amount">회복량.</param>
    public void Heal(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);

        // 체력바 업데이트
        if (healthBarController != null)
            healthBarController.UpdateHealth(health);
    }

    /// <summary>
    /// 체력이 최대치보다 낮은지 여부를 반환합니다.
    /// </summary>
    /// <returns>치료가 필요하면 true.</returns>
    public bool NeedsHealing()
    {
        return health < maxHealth;
    }

    #endregion


    #region 코루틴 초기화

    /// <summary>
    /// 현재 하던 행동을 모두 멈추고 애니메이션을 대기 상태로 전환시킨다.
    /// </summary>
    public void Freeze()
    {
        // 일단 모든 코루틴을 멈추고
        StopAllCrewCoroutine();

        // 전투 중단
        inCombat = false;
        //PlayAnimation("attack");
        combatCoroutine = null;
        combatTarget = null;
        madRoom = null;

        // 작업 중단
        WalkOut();

        // 이동 중단
        isMoving = false;
        moveCoroutine = null;

        // 수리 중단
        repairCoroutine = null;

        // 애니메이션 대기상태로 전환
        PlayAnimation("idle");
    }

    /// <summary>
    /// 현재 하던 작업 행동을 멈추고 자신을 Room.workingCrew로 할당한 Room을 찾아 할당 해제시키고 함선 능력치를 재계산한다.
    /// </summary>
    public void WalkOut()
    {
        if (isWorking)
        {
            isWorking = false;
            PlayAnimation("work");
            foreach (Room room in currentShip.GetAllRooms())
            {
                if (room.workingCrew == this)
                {
                    room.workingCrew = null;
                }
            }
            currentShip.RecalculateAllStats();
        }
    }

    #endregion

    /// <summary>
    /// 현재 선원이 탑승해 있는 함선이 선원의 모선인지 확인 (상대 배에 타있으면 false)
    /// </summary>
    /// <returns></returns>
    public bool IsOwnShip()
    {
        // 유저 함선에 텔포한 적일 경우 : isPlayerControlled = false, currentShip.isPlayerShip = true
        return isPlayerControlled == currentShip.isPlayerShip ? true : false;
    }

    #region 텔레포트

    public bool isTPing = false;

    /// <summary>
    /// 딜레이 후 텔레포트 진행
    /// </summary>
    /// <param name="crew"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public IEnumerator TeleportAfterDelay(CrewMember crew, float delay)
    {
        isTPing = true;
        PlayAnimation("tp_out");
        yield return new WaitForSeconds(delay);

        // 1. 현재 선원의 모선을 확인 후 상대 함선 추적
        Ship targetShip = null;
        Ship exitShip = null;

        // 일단 본인 함선이야 - 텔포로 상대 배로 가는 행동
        if (crew.IsOwnShip())
        {
            // 유저 함선 && 유저 선원
            if (crew.isPlayerControlled)
            {
                targetShip = GameManager.Instance.currentEnemyShip;
                exitShip = GameManager.Instance.playerShip;
            }
            else // 적 함선 && 적 선원
            {
                targetShip = GameManager.Instance.playerShip;
                exitShip = GameManager.Instance.currentEnemyShip;
            }
        }
        else // 본인 함선으로 복귀 행동
        {
            Debug.LogWarning("본인 함선 아님");
            // 적 함선 && 유저 선원
            if (crew.isPlayerControlled)
            {
                targetShip = GameManager.Instance.playerShip;
                exitShip = GameManager.Instance.currentEnemyShip;
            }
            else // 유저 함선 && 적 선원
            {
                targetShip = GameManager.Instance.currentEnemyShip;
                exitShip = GameManager.Instance.playerShip;
            }
        }

        // 2. 상대 함선의 랜덤 위치에 선원 생성
        List<Room> oppositeAllRooms = targetShip.GetAllRooms();
        List<Room> oppositeShuffled = oppositeAllRooms.Where(
            r => r.roomType != RoomType.Teleporter
        ).OrderBy(_ => UnityEngine.Random.value).ToList();

        Room assignedRoom = null;
        Vector2Int assignedTile = Vector2Int.zero;
        bool assigned = false;

        // 텔포 방은 피해서 스폰
        foreach (Room targetRoom in oppositeShuffled)
        {
            List<Vector2Int> candidates = targetRoom.GetRotatedCrewEntryGridPriority().Where
            (
                t => !CrewReservationManager.IsTileOccupied(targetShip, t)
            ).ToList();

            if (candidates.Count == 0)
                continue;

            assignedTile = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            assignedRoom = targetRoom;
            assigned = true;
            break;
        }

        // 3. 실제 이동 처리
        if (assigned)
        {
            // 1) 기존 함선 점유 타일 해제, 선원 리스트 갱신
            CrewReservationManager.ExitTile(exitShip, crew.currentRoom, crew.reservedTile, crew);
            if (crew.IsOwnShip())
                exitShip.allCrews.Remove(crew);
            else
                exitShip.allEnemies.Remove(crew);

            // 2) 타겟 함선 점유 타일 등록, 선원 리스트 갱신
            CrewReservationManager.ReserveTile(targetShip, assignedRoom, assignedTile, crew);

            // 3) 위치 설정
            crew.position = assignedTile;
            crew.currentRoom = assignedRoom;
            crew.transform.position = targetShip.GetWorldPositionFromGrid(assignedTile);
            crew.transform.SetParent(assignedRoom.transform);
            crew.currentShip = targetShip;

            if (crew.IsOwnShip())
                targetShip.allCrews.Add(crew);
            else
                targetShip.allEnemies.Add(crew);

            // 4) 컴포넌트 활성화
            crew.gameObject.SetActive(true);
            crew.enabled = true;

            BoxCollider2D colTP = crew.GetComponent<BoxCollider2D>();
            if (colTP != null)
                colTP.enabled = true;

            Animator animTP = crew.GetComponent<Animator>();
            if (animTP != null)
                animTP.enabled = true;

            // 5) 타겟 함선이 본인 모선이 아닐 경우 (상대 함선으로 넘어갈 때 RTSSelection 해제)
            if (targetShip != IsOwnShip())
                if (RTSSelectionManager.Instance.selectedCrew.Contains(this))
                    RTSSelectionManager.Instance.selectedCrew.RemoveAll(cm => cm == null || cm == this);

            // 6) 애니메이션 + 체력바 카메라 세팅
            PlayAnimation("tp_in");
            yield return new WaitForSeconds(delay);

            isTPing = false;

            CrewHealthBar healthBarTP = gameObject.transform.GetChild(0).GetComponent<CrewHealthBar>();
            if (targetShip == GameManager.Instance.playerShip)
                healthBarTP.targetCamera = Camera.main;
            else
                if (RTSSelectionManager.Instance.CallCombatManager(out CombatManager combatManager))
                healthBarTP.targetCamera = combatManager.cam.enemyCam;

            // 7) 텔포 후 도착한 방에 적 있을 경우 : 자동 전투, lookatme()로 광역 어그로
            if (crew.isWithEnemy() && crew.inCombat == false)
            {
                Debug.Log("텔포 후 스폰된 방에서 적 찾음");
                RTSSelectionManager.Instance.MoveForCombat(crew);
                crew.LookAtMe();
            }
        }
        else
        {
            Debug.LogError("상대 함선의 모든 타일이 차있어 텔포 불가");
            isTPing = false;
            yield return null;
        }
    }

    #endregion

    private void StopAllCrewCoroutine()
    {
        StopAllCoroutines();
        combatCoroutine = null;
        repairCoroutine = null;
        moveCoroutine = null;
        DieCoroutine = null;
    }
}
