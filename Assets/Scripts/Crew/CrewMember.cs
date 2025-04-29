using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 선원의 데이터를 확장하여 실제 게임 내 선원(Crew)의 기능을 담당하는 클래스입니다.
/// </summary>
[Serializable]
public class CrewMember : CrewBase
{
    /// <summary>
    /// 현재 목표 이동 타일
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
    /// 목적지 방
    /// </summary>
    public Room reservedRoom;

    /// <summary>
    /// 목적지 타일
    /// </summary>
    public Vector2Int reservedTile;

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

        if (currentTargetTile != null)
        {
            Vector3 targetWorldPos = GridToWorldPosition(currentTargetTile);

            while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);

            transform.position = targetWorldPos;
            position = new Vector2(currentTargetTile.x, currentTargetTile.y);
        }
        else
        {
            // 비상 처리 : 현재 좌표 스냅
            Vector2 snappedPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
            transform.position = new Vector3(snappedPos.x, snappedPos.y, transform.position.z);
            position = new Vector2(Mathf.RoundToInt(snappedPos.x), Mathf.RoundToInt(snappedPos.y));
        }

        // 1. 이동 중 새로운 이동 명령 수신에도 기존 점유 타일 해제
        ExitCurrentTile();

        // 2. 새로운 목적지 예약 (점유)
        ReserveDestination();

        // 새로운 경로 이동
        path = newPath;
        moveCoroutine = StartCoroutine(FollowPathCoroutine());
    }

    /// <summary>
    /// 기존 점유 타일 해제, 방에서 나옴
    /// </summary>
    private void ExitCurrentTile()
    {
        if (currentRoom != null)
        {
            Vector2Int currentTile = GetCurrentTile();
            currentRoom.VacateTile(currentTile);
            currentRoom.OnCrewExit(this);
        }
    }

    /// <summary>
    /// 새로운 목적지 예약, 점유 타일 등록
    /// </summary>
    private void ReserveDestination()
    {
        if (reservedRoom != null)
        {
            List<Vector2Int> candidates = reservedRoom.GetRotatedCrewEntryGridPriority().Where
            (
                t => !reservedRoom.IsTileOccupiedByCrew(t)
            ).ToList();

            if (candidates.Count > 0)
            {
                // 점유 되지 않은 타일 (candidates) 중 우선순위 가장 높은 타일 선택 (candidates[0])
                reservedTile = candidates[0];
                reservedRoom.OccupyTile(reservedTile);
            }
        }
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
            currentTargetTile = tile;
            Vector3 targetWorldPos = GridToWorldPosition(tile);

            // 이동 중인 방향
            movementDirection = (targetWorldPos - transform.position).normalized;

            while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
            {
                float speedMultiplier = 1f;

                // 현재 방 체크
                Room room = RTSSelectionManager.Instance.playerShip.GetRoomAtPosition(GetCurrentTile());

                // 복도는 이동 속도 10% 증가
                if (room != null && room.GetRoomType() == RoomType.Corridor)
                    speedMultiplier = 1.1f;

                // MoveTowards() : 다른 선원과의 충돌 무시하고 통과
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * speedMultiplier * Time.deltaTime);
                yield return null;
            }

            // 선원 위치 갱신
            transform.position = targetWorldPos;
            position = new Vector2(tile.x, tile.y);
        }

        isMoving = false;

        // 도착 후 방 갱신
        currentRoom = reservedRoom;
        reservedRoom = null;

        // 이동 완료한 위치에서 함내 전투 검사
        RTSSelectionManager.Instance.CheckForCombat(this);
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
}
