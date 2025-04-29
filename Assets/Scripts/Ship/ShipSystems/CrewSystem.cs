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
        if (crews.Count >= GetShipStat(ShipStat.CrewCapacity))
            return false;

        parentShip.RecalculateAllStats();

        if (newCrew.currentRoom == null)
        {
            Room randomRoom = parentShip.GetRandomRoom();
            randomRoom.OnCrewEnter(newCrew);

            newCrew.transform.position = parentShip.GridToWorldPosition(randomRoom.position);
            newCrew.transform.SetParent(randomRoom.transform);
            newCrew.currentRoom = randomRoom;
            newCrew.position = randomRoom.position;
        }
        else
        {
            newCrew.currentRoom.OnCrewEnter(newCrew);
            newCrew.transform.position = parentShip.GridToWorldPosition(newCrew.currentRoom.position);
            newCrew.transform.SetParent(newCrew.currentRoom.transform);
        }

        crews.Add(newCrew);
        return true;
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
    /// 새로운 함선의 랜덤한 방의 랜덤 타일에 선원 배치 (겹치지 않게 타일 점유 등록)
    /// </summary>
    public void RestoreCrewAfterBuild(List<CrewMember> backupCrews)
    {
        HashSet<Vector2Int> alreadyOccupiedTiles = new HashSet<Vector2Int>();

        foreach (CrewMember crew in backupCrews)
        {
            Room assignRoom = FindRandomRoom();
            if (assignRoom != null)
            {
                // 랜덤 배치 가능한 좌표 리스트
                List<Vector2Int> candidates = assignRoom.GetRotatedCrewEntryGridPriority().Where
                (
                    t => !assignRoom.IsTileOccupiedByCrew(t) && !alreadyOccupiedTiles.Contains(t)
                ).ToList();

                // 모든 방이 다 찼으면 (그럴 일 없지만) 무시하고 랜덤 배치
                if (candidates.Count == 0)
                    candidates = assignRoom.GetRotatedCrewEntryGridPriority();

                // 랜덤 배치 좌표
                Vector2Int assignTile = candidates[Random.Range(0, candidates.Count)];
                alreadyOccupiedTiles.Add(assignTile);

                // z값 확인 필요 + 같은 타일 생성 안 되게 확인 필요
                crew.transform.position = new Vector3(assignTile.x + 0.5f, assignTile.y + 0.5f, 0f);
                crew.position = assignTile;
                crew.currentRoom = assignRoom;
                assignRoom.OccupyTile(assignTile);
                assignRoom.OnCrewEnter(crew);

                // 랜덤 방에 종속
                crew.transform.SetParent(assignRoom.transform);

                // crew GameObject 활성화 (오류 방지용)
                crew.gameObject.SetActive(true);

                // CrewMember 컴포넌트 활성화
                CrewMember member = crew.GetComponent<CrewMember>();
                if (member != null)
                    member.enabled = true;

                // box collider 2d 컴포넌트 활성화
                BoxCollider2D col = crew.GetComponent<BoxCollider2D>();
                if (col != null)
                    col.enabled = true;
            }
        }
    }

    /// <summary>
    /// 함선 내 랜덤한 방 반환
    /// </summary>
    /// <returns></returns>
    public Room FindRandomRoom()
    {
        List<Room> rooms = parentShip.GetAllRooms();
        if (rooms == null || rooms.Count == 0)
            return null;

        return rooms[Random.Range(0, rooms.Count)];
    }
}
