using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 함선의 크루(승무원) 관리 시스템.
/// 크루의 추가, 제거 및 크루 수 관련 기능을 담당.
/// </summary>
public class CrewSystem : ShipSystem
{
    /// <summary>
    /// 현재 배치된 크루들의 목록입니다.
    /// </summary>
    // public List<CrewMember> crews = new();

    /// <summary>
    /// 시스템을 초기화합니다. 크루가 없는 경우 경고를 표시할 수 있습니다.
    /// </summary>
    /// <param name="ship">초기화할 대상 함선 객체.</param>
    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);

        Refresh();
    }

    public override void Refresh()
    {
        if (GetCrewCount() == 0)
        {
            // AlertNeedCrew();
        }
    }

    /// <summary>
    /// 매 프레임마다 호출되어 시스템 상태를 갱신합니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
    public override void Update(float deltaTime)
    {
    }

    /// <summary>
    /// 새로운 크루 멤버를 추가합니다.
    /// </summary>
    /// <param name="newCrew">추가할 크루 멤버.</param>
    /// <returns>추가에 성공하면 true, 크루 정원이 초과되었으면 false.</returns>
    public bool AddCrew(CrewBase newCrew)
    {
        // TODO : 검사 밖으로 빼기
        if (newCrew.isPlayerControlled)
            if (parentShip.allCrews.Count >= GetShipStat(ShipStat.CrewCapacity))
                return false;

        parentShip.RecalculateAllStats();

        // 1. 유효한 CrewMember인지 검사
        CrewMember crew = newCrew as CrewMember;
        if (crew == null)
        {
            Debug.LogWarning("CrewSystem - AddCrew : CrewBase가 CrewMember가 아님");
            return false;
        }

        // 2. 배치 가능한 방 리스트 획득
        List<Room> allRooms = parentShip.GetAllRooms();
        List<Room> shuffled = allRooms.OrderBy(_ => Random.value).ToList();

        // 3. 랜덤방 점유되지 않은 랜덤 타일에 선원 배치 (단, 텔포 방은 제외)
        foreach (Room room in shuffled)
        {
            if (room.roomType == RoomType.Teleporter)
                continue;

            List<Vector2Int> candidates = room.GetRotatedCrewEntryGridPriority().Where
            (
                t => !CrewReservationManager.IsTileOccupied(parentShip, t)
            ).ToList();

            if (candidates.Count == 0)
                continue;

            Vector2Int spawnTile = candidates[Random.Range(0, candidates.Count)];
            crew.position = spawnTile;
            crew.currentRoom = room;
            crew.transform.position = parentShip.GetWorldPositionFromGrid(spawnTile);
            crew.transform.SetParent(room.transform);
            crew.currentShip = parentShip;

            Debug.Log($"선원 랜덤 스폰 위치 : {spawnTile}");

            // 점유 타일 등록
            CrewReservationManager.ReserveTile(parentShip, room, spawnTile, crew);
            room.OnCrewEnter(crew);

            parentShip.allCrews.Add(crew);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 기존의 크루 멤버를 제거합니다. (아군, 적군 모두 제거)
    /// </summary>
    /// <param name="crewToRemove">제거할 크루 멤버.</param>
    /// <returns>제거에 성공하면 true, 해당 크루가 존재하지 않으면 false.</returns>
    public bool RemoveCrew(CrewMember crewToRemove)
    {
        if (!parentShip.allCrews.Contains(crewToRemove) || !parentShip.allEnemies.Contains(crewToRemove))
            return false;

        if (parentShip.allCrews.Contains(crewToRemove))
            parentShip.allCrews.Remove(crewToRemove);
        else if (parentShip.allEnemies.Contains(crewToRemove))
            parentShip.allEnemies.Remove(crewToRemove);

        Object.Destroy(crewToRemove.gameObject);

        return true;
    }

    /// <summary>
    /// 소유하고 있는 모든 선원을 제거 (아군, 적군)
    /// </summary>
    public void RemoveAllCrews()
    {
        for (int i = parentShip.allCrews.Count - 1; i >= 0; i--)
            RemoveCrew(parentShip.allCrews[i]);

        for (int i = parentShip.allEnemies.Count - 1; i >= 0; i--)
            RemoveCrew(parentShip.allEnemies[i]);
    }

    /// <summary>
    /// 현재 탑승 중인 크루의 수를 반환합니다.
    /// </summary>
    /// <returns>현재 크루 수.</returns>
    public int GetCrewCount()
    {
        return parentShip.allCrews.Count;
    }

    /// <summary>
    /// 현재 탑승 중인 아군, 적군 포함 모든 크루 객체의 목록을 반환합니다.
    /// </summary>
    /// <returns>크루 객체 리스트.</returns>
    public List<CrewMember> GetCrews()
    {
        List<CrewMember> total = new();

        foreach (CrewMember crew in parentShip.allCrews)
            total.Add(crew);

        foreach (CrewMember crew in parentShip.allEnemies)
            total.Add(crew);

        return total;
    }

    /// <summary>
    /// 특정 위치에 있는 모든 선원들을 반환합니다. (아군, 적군 모두)
    /// </summary>
    /// <param name="position">확인할 격자 위치</param>
    /// <returns>해당 위치에 있는 선원들의 리스트</returns>
    public List<CrewMember> GetCrewsAtPosition(Vector2Int position)
    {
        List<CrewMember> crewsAtPosition = new();
        List<CrewMember> crews = GetCrews();

        foreach (CrewMember crew in crews)
            // crew.position이 선원의 현재 위치를 가지고 있다고 가정
            if (crew.position == position)
                crewsAtPosition.Add(crew);

        return crewsAtPosition;
    }

    /// <summary>
    /// 유효성 검사 실패 시, 기존 선원들을 백업된 위치 및 방으로 복구
    /// </summary>
    public void RevertOriginalCrews(List<BackupCrewData> backupDatas)
    {
        // ReplaceShipFromBlueprint()에서 Room을 날렸기 때문에 함선 복구 후 매핑 작업 필요 (currentRoom == null)
        Dictionary<Vector2Int, Room> roomMap = parentShip.GetAllRooms().ToDictionary(r => r.position, r => r);

        // 선원 원위치
        foreach (BackupCrewData data in backupDatas)
        {
            if (!roomMap.TryGetValue(data.roomPos, out Room room))
            {
                Debug.LogError($"{data.roomPos}를 찾지 못해서 {data.crewName} 설치 불가");
                continue;
            }

            // 선원 Destroy() -> 새로 생성 필요
            CrewMember originCrew =
                GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(data.race, data.crewName) as CrewMember;

            originCrew.needsOxygen = data.needsOxygen;
            originCrew.position = data.position;
            originCrew.currentRoom = room;
            originCrew.transform.position = parentShip.GetWorldPositionFromGrid(data.position);
            originCrew.transform.SetParent(room.transform);
            originCrew.currentShip = parentShip;
            originCrew.health = data.currentHP;

            // 점유 타일 등록
            CrewReservationManager.ReserveTile(parentShip, room, data.position, originCrew);
            room.OnCrewEnter(originCrew);

            // 오브젝트 및 컴포넌트 활성화
            originCrew.gameObject.SetActive(true);
            originCrew.enabled = true;

            BoxCollider2D col = originCrew.GetComponent<BoxCollider2D>();
            if (col != null)
                col.enabled = true;

            Animator anim = originCrew.GetComponent<Animator>();
            if (anim != null)
                anim.enabled = true;

            parentShip.allCrews.Add(originCrew);
        }
    }

    /// <summary>
    /// 새로운 함선의 랜덤한 방의 랜덤 타일에 선원 배치 (겹치지 않게 타일 점유 등록)
    /// </summary>
    /// <param name="backupCrewDatas"></param>
    public void RestoreCrewAfterBuild(List<BackupCrewData> backupCrewDatas)
    {
        foreach (BackupCrewData data in backupCrewDatas)
        {
            CrewMember crew = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(data.race, data.crewName) as CrewMember;
            if (crew == null)
                continue;

            List<Room> allRooms = parentShip.GetAllRooms();
            List<Room> shuffled = allRooms.OrderBy(_ => Random.value).ToList();

            bool assigned = false;

            // 텔포 방 제외 랜덤 방 랜덤 타일 선원 배치
            foreach (Room room in shuffled)
            {
                if (room.roomType == RoomType.Teleporter)
                    continue;

                List<Vector2Int> candidates = room.GetRotatedCrewEntryGridPriority().Where
                (
                    t => !CrewReservationManager.IsTileOccupied(parentShip, t)
                ).ToList();

                if (candidates.Count == 0)
                    continue;

                // 랜덤 타일 선택
                Vector2Int spawnTile = candidates[Random.Range(0, candidates.Count)];

                crew.needsOxygen = data.needsOxygen;
                // 위치 설정
                crew.position = spawnTile;
                crew.currentRoom = room;
                crew.transform.position = parentShip.GetWorldPositionFromGrid(spawnTile);
                crew.transform.SetParent(room.transform);
                crew.currentShip = parentShip;
                crew.health = data.currentHP;

                // 점유 등록
                CrewReservationManager.ReserveTile(parentShip, room, spawnTile, crew);
                room.OnCrewEnter(crew);

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
                crew.BackToThePeace();

                // crewList에 추가
                parentShip.allCrews.Add(crew);

                assigned = true;
                break;
            }

            // 만약 모든 방이 다 찼다면 로그 출력
            if (!assigned) Debug.LogError("모든 방이 다 차서 선원 겹쳐지게 배치됨.");
        }
    }
}
