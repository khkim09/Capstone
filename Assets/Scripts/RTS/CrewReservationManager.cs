using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 선원의 타일 점유를 전역에서 통합 관리하는 매니저 - 싱글톤
/// </summary>
public static class CrewReservationManager
{
    /// <summary>
    /// 타일 예약제를 위한 매니저 - 싱글톤
    /// </summary>
    //public static CrewReservationManager Instance { get; private set; }

    /// <summary>
    /// 함선별 타일 예약 정보 관리용 딕셔너리
    /// </summary>
    private static Dictionary<Ship, Dictionary<Vector2Int, CrewMember>> tileOccupancy = new();

    // /// <summary>
    // /// 싱글톤 연결
    // /// </summary>
    // private void Awake()
    // {
    //     if (Instance != null && Instance != this)
    //     {
    //         Destroy(gameObject);
    //         return;
    //     }
    //     Instance = this;
    // }

    /// <summary>
    /// 함선의 검사 타일이 선원에 의한 점유 여부 반환
    /// </summary>
    /// <param name="ship"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    public static bool IsTileOccupied(Ship ship, Vector2Int tile)
    {
        return tileOccupancy.ContainsKey(ship) && tileOccupancy[ship].ContainsKey(tile);
    }

    /// <summary>
    /// 함선의 검사 타일을 점유 중인 선원 반환
    /// </summary>
    /// <param name="ship"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    public static CrewMember GetOccupyingCrew(Ship ship, Vector2Int tile)
    {
        return IsTileOccupied(ship, tile) ? tileOccupancy[ship][tile] : null;
    }

    /// <summary>
    /// 현재 타일에서 선원 제거 (정지 상태에서 이동 명령 수신 시)
    /// </summary>
    /// <param name="ship"></param>
    /// <param name="tile"></param>
    public static void ExitTile(Ship ship, Room room, Vector2Int tile, CrewMember crew)
    {
        if (tileOccupancy.ContainsKey(ship))
            tileOccupancy[ship].Remove(tile);
        else
            Debug.LogError($"[ExitTile] {crew.race}가 해당 위치에 없는데 점유 해제 시도");

        // 선원 나감 처리, 오브젝트 종속 해제
        room.OnCrewExit(crew);
        crew.transform.SetParent(null);
    }

    /// <summary>
    /// 타일 예약 (AssignPathAndMove(), CancelAndRedirect()에서 호출)
    /// </summary>
    /// <param name="ship"></param>
    /// <param name="tile"></param>
    /// <param name="crew"></param>
    public static void ReserveTile(Ship ship, Room room, Vector2Int tile, CrewMember crew)
    {
        if (!tileOccupancy.ContainsKey(ship))
            tileOccupancy[ship] = new Dictionary<Vector2Int, CrewMember>();

        tileOccupancy[ship][tile] = crew;

        // 선원 입장 처리, 오브젝트를 방으로 종속
        // room.OnCrewEnter(crew);
        crew.transform.SetParent(room.transform);
    }

    /// <summary>
    /// 특정 함선 전체 타일 점유 정보 초기화
    /// </summary>
    /// <param name="ship"></param>
    public static void ClearAllReservations(Ship ship)
    {
        if (tileOccupancy.ContainsKey(ship))
            tileOccupancy[ship].Clear();
    }
}
