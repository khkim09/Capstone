using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// 선원의 데이터를 확장하여 실제 게임 내 선원(Crew)의 기능을 담당하는 클래스입니다.
/// </summary>
[Serializable]
public class CrewMember : CrewBase
{

    /// <summary>
    /// 현재 이동중인 타일
    /// </summary>
    private Vector2Int currentTargetTile;

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

    public Coroutine comBatCoroutine;

    /// <summary>
    /// 이동 전 위치했던 타일
    /// </summary>
    public Vector2Int originPosTile;

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
    /// 이동 중 새로운 이동 명령 수신 시
    /// 이동 방향으로 가장 가까운 타일까지 이동 후 경로 재탐색
    /// </summary>
    /// <param name="newPath"></param>
    public void CancelAndRedirect(List<Vector2Int> newPath)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        // 1. 현재 이동 중인 타일까지 도달 후 정지
        if (currentTargetTile != null)
        {
            Vector3 targetWorldPos = GridToWorldPosition(currentTargetTile);

            while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
            {
                float speedMultiplier = 1f;

                // 현재 방 체크
                Room room = RTSSelectionManager.Instance.playerShip.GetRoomAtPosition(GetCurrentTile());

                // 복도는 이동 속도 30% 증가
                if (room != null && room.GetRoomType() == RoomType.Corridor)
                    speedMultiplier = 1.33f;

                // MoveTowards() : 다른 선원과의 충돌 무시하고 통과
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * speedMultiplier * Time.deltaTime);
            }

            transform.position = targetWorldPos;
            position = new Vector2Int(currentTargetTile.x, currentTargetTile.y);
        }

        // 2. 이전 목적지 타일 점유 해제
        ExitReserveTile();

        // 3. 새로운 목적지 예약
        ReserveDestination();

        // 새로운 경로 이동
        path = newPath;
        moveCoroutine = StartCoroutine(FollowPathCoroutine());
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
    /// 새로운 목적지 예약, 점유 타일 등록
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
            transform.SetParent(reservedRoom.transform);
        }
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


        if (inCombat)
        {
            inCombat = false;
            StopCoroutine(comBatCoroutine);
            comBatCoroutine = null;
            DontTouchMe();
        }

        foreach (Vector2Int tile in path)
        {
            currentTargetTile = tile;
            Vector3 targetWorldPos = GridToWorldPosition(tile);

            // 이동 중인 방향
            movementDirection = (targetWorldPos - transform.position).normalized;
            PlayAnimation("walk",isMoving);
            while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
            {
                float speedMultiplier = 1f;

                // 현재 방 체크
                Room room = RTSSelectionManager.Instance.playerShip.GetRoomAtPosition(GetCurrentTile());

                // 복도는 이동 속도 30% 증가
                if (room != null && room.GetRoomType() == RoomType.Corridor)
                    speedMultiplier = 1.33f;

                // MoveTowards() : 다른 선원과의 충돌 무시하고 통과
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * speedMultiplier * Time.deltaTime);
                yield return null;
            }

            // 선원 위치 갱신
            transform.position = targetWorldPos;
            position = new Vector2Int(tile.x, tile.y);
        }

        isMoving = false;

        PlayAnimation("walk",isMoving);

        // 도착 후 방 갱신
        currentRoom = reservedRoom;
        reservedRoom = null;

        RTSSelectionManager.Instance.MoveForCombat(this,currentRoom.occupiedCrewTiles);
        LookAtMe();
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


    //----------애니메이션------------
    /// <summary>
    /// animation 실행
    /// </summary>
    /// <param name="trigger"></param>
    private void PlayAnimation(string trigger, bool onoff=true)
    {
        if (trigger.Equals("walk"))
        {
            animator.SetFloat("X", movementDirection.x);
            animator.SetFloat("Y", movementDirection.y);
            animator.SetBool(trigger, onoff);
        }
        else if (trigger.Equals("attack"))
        {
            animator.SetFloat("X",movementDirection.x);
            animator.SetFloat("Y", movementDirection.y);
            animator.SetTrigger("attack");
        }
        else if (trigger.Equals("die"))
        {
            animator.SetTrigger("die");
        }
    }

    //----------전투---------

    public CrewMember combatTarget { set; get; }
    public bool inCombat = false;
    private float attackDelay = 1f;
    public Coroutine DieCoroutine=null;


    public IEnumerator CombatRoutine()
    {
        //때릴 사람이 없어
        if (combatTarget == null || !combatTarget.isAlive)
        {
            inCombat = false;
            comBatCoroutine = null;
            yield break;
        }

        inCombat = true;
        combatTarget.bullier.Add(this);

        //이미 누군가랑 싸우고 있어
        if (!combatTarget.inCombat)
        {
            combatTarget.combatTarget = this;
            combatTarget.comBatCoroutine = StartCoroutine(combatTarget.CombatRoutine());
        }

        movementDirection = combatTarget.GetCurrentTile() - GetCurrentTile();
        PlayAnimation("attack");
        //실제로 데미지가 들어가는 부분
        yield return new WaitForSeconds(attackDelay);
        Attack(this,combatTarget);
        yield return new WaitForSeconds(attackDelay);

        //이래도 살아있어? 한대 더 맞자
        if (combatTarget.isAlive)
        {
            comBatCoroutine = StartCoroutine(CombatRoutine());
        }
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
    public void Attack(CrewMember attacker, CrewMember target)
    {
        if (target == null)
        {
            //목표를 포착했...는 중이다
            inCombat = false;
            RTSSelectionManager.Instance.MoveForCombat(this,this.reservedRoom.occupiedCrewTiles);
            return;
        }
        // 피해량 계산식: (공격 주체 기본 공격 + 장비 공격력(tmp)) * (1 - (상대 방어력 / 100))
        float damage = 1; //(attacker.attack + attacker.equippedWeapon.eqAttackBonus) * (1 - target.defense / 100f);
        target.health -= damage;
        Debug.Log($"{attacker.crewName}이(가) {target.crewName}에게 {damage}의 피해를 입혔습니다.");

        if (target.health <= 0)
        {
            target.isAlive = false;
            Debug.Log($"{target.crewName}이(가) 사망하였습니다.");

            // 타일 점유 해제 및 방 퇴장 처리
            if (target.currentRoom != null)
            {
                Vector2Int currentTile = target.GetCurrentTile();
                target.currentRoom.VacateTile(currentTile);
                target.currentRoom.OnCrewExit(target);
            }

            // 선원 제거 (죽음)
            // 사망신고하는데 이것저것 내야할 서류가 많아요
            target.Die();
        }
    }

    /// <summary>
    /// 사망 시, 자신을 combatTarget으로 지정한 선원들에게서 자신을 할당해제시키고 다른 목표를 탐색하도록한다.
    /// 또한 애니메이션 재생을 마친 후에 Destroy
    /// TODO: 아직 미완성이야
    ///
    /// </summary>
    public void Die()
    {
        isAlive = false;
        isMoving = false;
        inCombat = false;
        DontTouchMe();
        if (DieCoroutine == null)
        {
            DieCoroutine = StartCoroutine(ImDying());
        }
    }

    public void DontTouchMe()
    {
        if(bullier != null)
        {
            foreach (CrewMember hittingMan in bullier)
            {
                hittingMan.inCombat = false;
                hittingMan.combatTarget = null;
                StopCoroutine(hittingMan.comBatCoroutine);
                hittingMan.comBatCoroutine = null;
                hittingMan.bullier.Remove(this);

                RTSSelectionManager.Instance.MoveForCombat(hittingMan, hittingMan.currentRoom.occupiedCrewTiles);
            }
        }
    }

    public IEnumerator ImDying()
    {
        PlayAnimation("die");
        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
    }
}
