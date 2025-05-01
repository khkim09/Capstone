using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 백업할 선원 데이터 구조체
/// </summary>
[System.Serializable]
public class BackupCrewData
{
    public CrewMember crew;
    public Vector2Int position;
    public Room currentRoom;
}

/// <summary>
/// 함선의 크루(승무원) 관리 시스템.
/// 크루의 추가, 제거 및 크루 수 관련 기능을 담당.
/// </summary>
public class CrewSystem : ShipSystem
{
    /// <summary>
    /// 현재 배치된 크루들의 목록입니다.
    /// </summary>
    private List<CrewBase> crews = new();

    /// <summary>
    /// 시스템을 초기화합니다. 크루가 없는 경우 경고를 표시할 수 있습니다.
    /// </summary>
    /// <param name="ship">초기화할 대상 함선 객체.</param>
    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);

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
        // TODO : 조건 검사를 밖에서 해서 capacity 부족하다는 걸 전달해야됨
        if (crews.Count >= GetShipStat(ShipStat.CrewCapacity))
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

        // 3. 랜덤방 점유되지 않은 랜덤 타일에 선원 배치
        foreach (Room room in shuffled)
        {
            List<Vector2Int> candidates = room.GetRotatedCrewEntryGridPriority().Where
            (
                t => !room.IsTileOccupiedByCrew(t) && !parentShip.IsCrewTileOccupied(room, t)
            ).ToList();

            if (candidates.Count == 0)
                continue;

            Vector2Int spawnTile = candidates[Random.Range(0, candidates.Count)];
            crew.position = spawnTile;
            crew.currentRoom = room;
            crew.transform.position = parentShip.GridToWorldPosition(spawnTile);
            crew.transform.SetParent(room.transform);

            room.OccupyTile(spawnTile);
            room.OnCrewEnter(crew);
            parentShip.MarkCrewTileOccupied(room, spawnTile);

            crews.Add(crew);
            return true;
        }

        return false;

        // if (newCrew.currentRoom == null)
        // {
        //     Room randomRoom = parentShip.GetRandomRoom();
        //     randomRoom.OnCrewEnter(newCrew);

        //     newCrew.transform.position = parentShip.GridToWorldPosition(randomRoom.position);
        //     newCrew.transform.SetParent(randomRoom.transform);
        //     newCrew.currentRoom = randomRoom;
        //     newCrew.position = randomRoom.position;
        // }
        // else
        // {
        //     newCrew.currentRoom.OnCrewEnter(newCrew);
        //     newCrew.transform.position = parentShip.GridToWorldPosition(newCrew.position);
        //     newCrew.transform.SetParent(newCrew.currentRoom.transform);
        // }

        // crews.Add(newCrew);
        // return true;
    }

    /// <summary>
    /// 기존의 크루 멤버를 제거합니다.
    /// </summary>
    /// <param name="crewToRemove">제거할 크루 멤버.</param>
    /// <returns>제거에 성공하면 true, 해당 크루가 존재하지 않으면 false.</returns>
    public bool RemoveCrew(CrewBase crewToRemove)
    {
        if (!crews.Contains(crewToRemove))
            return false;

        crews.Remove(crewToRemove);
        Object.Destroy(crewToRemove.gameObject);

        // TODO: 방에서 나가는 처리도 해야할 수도 있다. 추후 구현 필요하면 구현할 것

        return true;
    }

    /// <summary>
    /// 소유하고 있는 모든 선원을 제거
    /// </summary>
    public void RemoveAllCrews()
    {
        for (int i = crews.Count - 1; i >= 0; i--) RemoveCrew(crews[i]);
    }

    /// <summary>
    /// 현재 탑승 중인 크루의 수를 반환합니다.
    /// </summary>
    /// <returns>현재 크루 수.</returns>
    public int GetCrewCount()
    {
        return crews.Count;
    }

    /// <summary>
    /// 현재 탑승 중인 모든 크루 객체의 목록을 반환합니다.
    /// </summary>
    /// <returns>크루 객체 리스트.</returns>
    public List<CrewBase> GetCrews()
    {
        return crews;
    }

    /// <summary>
    /// 특정 위치에 있는 모든 선원들을 반환합니다.
    /// </summary>
    /// <param name="position">확인할 격자 위치</param>
    /// <returns>해당 위치에 있는 선원들의 리스트</returns>
    public List<CrewBase> GetCrewsAtPosition(Vector2Int position)
    {
        List<CrewBase> crewsAtPosition = new();

        foreach (CrewBase crew in crews)
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
        foreach (BackupCrewData data in backupDatas)
        {
            CrewMember crew = data.crew;
            Vector2Int pos = data.position;
            Room room = data.currentRoom;

            CrewBase originCrew = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(crew.race, crew.crewName);


            originCrew.position = pos;
            originCrew.currentRoom = room;
            originCrew.transform.position = parentShip.GridToWorldPosition(pos);
            originCrew.transform.SetParent(room.transform);

            room.OccupyTile(pos);
            room.OnCrewEnter(originCrew);
            parentShip.MarkCrewTileOccupied(room, pos);

            // 오브젝트 및 컴포넌트 활성화
            originCrew.gameObject.SetActive(true);
            originCrew.enabled = true;
            BoxCollider2D col = originCrew.GetComponent<BoxCollider2D>();
            if (col != null)
                col.enabled = true;

            // 크루 리스트 중복 추가 방지
            if (!GetCrews().Contains(originCrew))
                GetCrews().Add(originCrew);
        }
    }

    /// <summary>
    /// 새로운 함선의 랜덤한 방의 랜덤 타일에 선원 배치 (겹치지 않게 타일 점유 등록)
    /// </summary>
    /// <param name="backupCrewDatas"></param>
    public void RestoreCrewAfterBuild(List<BackupCrewData> backupCrewDatas)
    {
        HashSet<Vector2Int> alreadyOccupiedTiles = new HashSet<Vector2Int>();

        foreach (BackupCrewData data in backupCrewDatas)
        {
            CrewMember crew = data.crew;

            List<Room> allRooms = parentShip.GetAllRooms();
            List<Room> shuffled = allRooms.OrderBy(_ => Random.value).ToList();

            bool assigned = false;

            foreach (Room room in shuffled)
            {
                List<Vector2Int> candidates = room.GetRotatedCrewEntryGridPriority().Where
                (
                    t => !room.IsTileOccupiedByCrew(t)
                    && !parentShip.IsCrewTileOccupied(room, t)
                    && !alreadyOccupiedTiles.Contains(t)
                ).ToList();

                if (candidates.Count == 0)
                    continue;

                // 랜덤 타일 선택
                Vector2Int spawnTile = candidates[Random.Range(0, candidates.Count)];
                alreadyOccupiedTiles.Add(spawnTile);

                // 위치 설정
                crew.position = spawnTile;
                crew.currentRoom = room;
                crew.transform.position = parentShip.GridToWorldPosition(spawnTile);
                crew.transform.SetParent(room.transform);

                // 점유 등록
                room.OccupyTile(spawnTile);
                room.OnCrewEnter(crew);
                parentShip.MarkCrewTileOccupied(room, spawnTile);

                // 컴포넌트 활성화
                crew.gameObject.SetActive(true);
                crew.enabled = true;
                BoxCollider2D col = crew.GetComponent<BoxCollider2D>();
                if (col != null)
                    col.enabled = true;

                if (!GetCrews().Contains(crew))
                    GetCrews().Add(crew);

                assigned = true;
                break;
            }

            // 만약 모든 방이 다 찼다면 로그 출력
            if (!assigned)
            {
                Debug.LogError("모든 방이 다 차서 선원 겹쳐지게 배치됨.");
            }
        }
    }
}
