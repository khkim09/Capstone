using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 함선의 텔레포트실(RoomType.Teleporter)을 나타내는 클래스.
/// 전력을 소비하며, 손상 상태에 따라 작동 여부가 제한됩니다.
/// </summary>
public class TeleportRoom : Room<TeleportRoomData, TeleportRoomData.TeleportRoomLevel>
{
    /// <summary>
    /// 초기화 시 방 타입을 Teleporter로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.Teleporter;
        workDirection = Vector2Int.zero;
    }

    /// <summary>
    /// 이 방이 함선 스탯에 기여하는 값을 계산합니다.
    /// 전력 사용량이 포함되며, 손상 시 단계별 기여도 감소 또는 작동 불능 처리됩니다.
    /// </summary>
    /// <returns>스탯 기여도 딕셔너리.</returns>
    public override Dictionary<ShipStat, float> GetStatContributions()
    {
        // 기본 기여도 가져오기 (작동 상태 체크 등)
        Dictionary<ShipStat, float> contributions = base.GetStatContributions();

        // 작동 상태가 아니면 기여도 없음
        if (!IsOperational() || currentRoomLevelData == null)
            return contributions;

        if (isActive) contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;

        if (currentLevel > 1 && damageCondition == DamageLevel.scratch)
        {
            TeleportRoomData.TeleportRoomLevel
                weakedRoomLevelData = roomData.GetTypedRoomData(currentLevel - 1);
            contributions[ShipStat.PowerUsing] = weakedRoomLevelData.powerRequirement;
        }

        return contributions;
    }

    /// <summary>
    /// 텔레포트실이 피해를 받을 때 호출됩니다.
    /// 이펙트를 갱신합니다.
    /// </summary>
    /// <param name="damage">받은 피해량.</param>
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        UpdateEffects();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="crew"></param>
    public override void OnCrewEnter(CrewMember crew)
    {
        base.OnCrewEnter(crew);

        // 0. 일단 텔포 방이 본인 모선인지 확인
        if (!crew.IsOwnShip())
            return;

        // 텔포 준비
        crew.Freeze();

        // 1. 현재 선원의 모선을 확인 후 상대 함선 추적
        Ship targetShip = null;

        // 일단 본인 함선이야
        if (crew.IsOwnShip())
        {
            // 유저 함선 && 유저 선원
            if (crew.isPlayerControlled)
                targetShip = GameManager.Instance.currentEnemyShip;
            else // 적 함선 && 적 선원
                targetShip = GameManager.Instance.playerShip;
        }

        // 딜레이 작업 필요
        // yield return new WaitForSeconds(2f);

        // 2. 상대 함선의 랜덤 위치에 선원 생성
        List<Room> oppositeAllRooms = targetShip.GetAllRooms();
        List<Room> oppositeShuffled = oppositeAllRooms.OrderBy(_ => Random.value).ToList();

        bool assigned = false;

        // 텔포 방은 피해서 스폰
        foreach (Room targetRoom in oppositeShuffled)
        {
            if (targetRoom.roomType == RoomType.Teleporter)
                continue;

            List<Vector2Int> candidates = targetRoom.GetRotatedCrewEntryGridPriority().Where
            (
                t => !CrewReservationManager.IsTileOccupied(targetShip, t)
            ).ToList();

            if (candidates.Count == 0)
                continue;

            // 랜덤 타일 선택
            Vector2Int oppositeSpawnTile = candidates[Random.Range(0, candidates.Count)];

            // 위치 설정
            crew.position = oppositeSpawnTile;
            crew.currentRoom = targetRoom;
            crew.transform.position = targetShip.GetWorldPositionFromGrid(oppositeSpawnTile);
            crew.transform.SetParent(targetRoom.transform);
            crew.currentShip = targetShip;

            // 기존 함선 텔포 방 점유 해제
            CrewReservationManager.ExitTile(crew.currentShip, crew.reservedRoom, crew.reservedTile, crew);

            // 타겟 함선 랜덤 위치 점유 등록
            CrewReservationManager.ReserveTile(targetShip, targetRoom, oppositeSpawnTile, crew);

            // 컴포넌트 활성화
            crew.gameObject.SetActive(true);
            crew.enabled = true;

            BoxCollider2D col = crew.GetComponent<BoxCollider2D>();
            if (col != null)
                col.enabled = true;

            Animator anim = crew.GetComponent<Animator>();
            if (anim != null)
                anim.enabled = true;

            crew.Freeze();

            // 적 함선으로 텔레포트 했으므로 enemy 리스트에 추가
            targetShip.allEnemies.Add(crew);

            assigned = true;
            break;
        }

        // 상대 함선 모든 타일이 꽉 차서 텔포 불가
        if (!assigned)
        {
            Debug.LogError("상대 함선의 모든 타일이 차있어 텔포 불가");
            return;
        }

        // 3. 텔포 후 도착한 방에 적 있을 경우 : 자동 전투, lookatme()로 광역 어그로 (유저 선원, 적 선원)
        // 4. 적 없을 경우 방 부수기 - (적 선원 AI)
        if (crew.isWithEnemy() && crew.inCombat == false)
        {
            Debug.LogError("텔포 후 스폰된 방에서 적 찾음");

            RTSSelectionManager.Instance.MoveForCombat(crew);

            crew.LookAtMe();
        }
        else
        {
            Debug.LogError("텔포 후 스폰된 방에 적 없음");

            // 적 선원 AI 가동 - 수정 필요
            if (!crew.isPlayerControlled)
                crew.combatCoroutine = StartCoroutine(crew.CombatRoutine());
            // 적 선원이 유저 함선 다 부수고, 유저 선원도 모두 죽였을 때 휴식 구현 필요
        }
    }
}
