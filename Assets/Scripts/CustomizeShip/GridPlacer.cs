using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선 커스터마이징을 위한 그리드 시스템을 생성하고 방/무기 배치를 관리하는 클래스.
/// 타일 생성, 배치 가능 여부 판단, 실제 설치 등을 처리합니다.
/// </summary>
public class GridPlacer : MonoBehaviour
{
    /// <summary>
    /// 생성할 타일의 프리팹입니다.
    /// </summary>
    public GameObject tilePrefab;

    /// <summary>
    /// 생성된 그리드 타일들의 부모 오브젝트입니다.
    /// </summary>
    public Transform gridTiles;

    /// <summary>
    /// 배치가 이루어질 설계도
    /// </summary>
    public BlueprintShip targetBlueprintShip;

    /// <summary>
    /// 기존 함선
    /// </summary>
    public Ship playerShip;

    /// <summary>
    /// 공통 roomPrefab
    /// </summary>
    public GameObject roomPrefab;

    /// <summary>
    /// 공통 weaponPrefab
    /// </summary>
    public GameObject weaponPrefab;

    /// <summary>
    /// 현재 도안 설계 상태에서 점유된 타일들 (모든 배치된 오브젝트 기준)
    /// </summary>
    public HashSet<Vector2Int> occupiedGridTiles = new();

    [SerializeField] private Vector2Int gridSize = new(60, 60);
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;

    /// <summary>
    /// 게임 시작 시 그리드 타일을 생성합니다.
    /// </summary>
    private void Start()
    {
        GenerateTiles();
    }

    /// <summary>
    /// 함선 커스터마이징에 사용할 그리드 생성
    /// 모든 tile object는 gridTiles 오브젝트의 자식으로 생성됩니다.
    /// </summary>
    public void GenerateTiles()
    {
        for (int x = 0; x < gridSize.x; x++)
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 pos = GetWorldPositionFromGrid(new Vector2Int(x, y));
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, gridTiles);
                tile.transform.localScale = Vector3.one * Constants.Grids.CellSize;
                tile.transform.position += new Vector3(0, 0, 17);
            }
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표로 변환합니다.
    /// </summary>
    /// <param name="gridPos">변환할 그리드 좌표.</param>
    /// <returns>해당 위치의 월드 좌표.</returns>
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return gridOrigin + new Vector3((gridPos.x + 0.5f) * Constants.Grids.CellSize,
            (gridPos.y + 0.5f) * Constants.Grids.CellSize, 0f);
    }

    /// <summary>
    /// 실제 월드 위치 반환
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public Vector3 GetWorldPositionFromGrid(Vector2Int gridPos)
    {
        return transform.TransformPoint(GridToWorldPosition(gridPos));
    }

    /// <summary>
    /// 월드 좌표를 그리드 좌표로 변환합니다.
    /// </summary>
    /// <param name="worldPos">변환할 월드 좌표.</param>
    /// <returns>해당 위치의 그리드 좌표.</returns>
    public Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        Vector3 local = new Vector3(worldPos.x, worldPos.y, 0) - gridOrigin;
        return new Vector2Int(Mathf.FloorToInt(local.x / Constants.Grids.CellSize),
            Mathf.FloorToInt(local.y / Constants.Grids.CellSize));
    }

    /// <summary>
    /// 설게도 화면에서 camera의 시작 위치 보정
    /// 설계도에 방이 없을 시 그리드 중앙 시작
    /// 설계도에 방이 있을 경우 방 들의 중앙 위치에서 시작
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCameraStartPositionToBP()
    {
        // 모든 배치 가능 오브젝트 가져오기
        List<IBlueprintPlaceable> allPlaceables = new();

        // 방 가져오기
        BlueprintRoom[] allBPRooms = targetBlueprintShip.GetComponentsInChildren<BlueprintRoom>();
        foreach (BlueprintRoom room in allBPRooms)
            allPlaceables.Add(room);

        // 무기 가져오기
        BlueprintWeapon[] allBPWeapons = targetBlueprintShip.GetComponentsInChildren<BlueprintWeapon>();
        foreach (BlueprintWeapon weapon in allBPWeapons)
            allPlaceables.Add(weapon);

        // 배치된 오브젝트 없으면 그리드 중앙
        if (allPlaceables.Count == 0)
            return GetWorldPositionFromGrid(gridSize / 2);

        // 전체 타일 평균 위치 계산
        List<Vector2Int> allTiles = new();
        foreach (IBlueprintPlaceable placeable in allPlaceables)
            allTiles.AddRange(placeable.GetOccupiedTiles());

        Vector2 average = Vector2.zero;
        foreach (Vector2Int tile in allTiles)
            average += (Vector2)tile;

        average /= allTiles.Count;

        return GetWorldPositionFromGrid(Vector2Int.RoundToInt(average));
    }

    /// <summary>
    /// 기존 소유 함선을 이루는 방들의 중심으로 카메라 시작 위치 보정
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCameraStartPositionToOriginShip()
    {
        List<Room> allRooms = playerShip.GetAllRooms();
        List<ShipWeapon> allWeapons = playerShip.GetAllWeapons(); // 함선의 모든 무기 가져오기 (구현 필요)

        // 배치된 방 없으면 그리드 중앙
        if (allRooms.Count == 0 && allWeapons.Count == 0)
            return GetWorldPositionFromGrid(gridSize / 2);

        // 전체 타일 평균 위치 계산
        List<Vector2Int> allTiles = new();

        foreach (Room room in allRooms)
            allTiles.AddRange(room.GetOccupiedTiles());

        foreach (ShipWeapon weapon in allWeapons)
        {
            // 무기가 점유하는 타일 추가 (구현 필요)
            Vector2Int pos = weapon.GetGridPosition();
            allTiles.Add(pos);
            allTiles.Add(new Vector2Int(pos.x + 1, pos.y));
        }

        Vector2 average = Vector2.zero;
        foreach (Vector2Int tile in allTiles)
            average += (Vector2)tile;

        average /= allTiles.Count;

        return GetWorldPositionFromGrid(Vector2Int.RoundToInt(average));
    }

    /// <summary>
    /// 해당 타일이 그리드 범위 내에 있는지 검사
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public bool IsInGrid(Vector2Int tile)
    {
        if (tile.x < 0 || tile.x >= gridSize.x || tile.y < 0 || tile.y >= gridSize.y)
            return false;
        return true;
    }

    /// <summary>
    /// 해당 타일이 현재 다른 오브젝트에 의해 점유되어 있는지 검사
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public bool IsTileOccupied(Vector2Int tile)
    {
        return occupiedGridTiles.Contains(tile);
    }

    /// <summary>
    /// 해당 오브젝트가 점유하고 있는 모든 타일을 전체 점유 목록에 추가
    /// </summary>
    /// <param name="placeable">배치 가능 오브젝트</param>
    public void MarkObjectOccupied(IBlueprintPlaceable placeable)
    {
        foreach (Vector2Int tile in placeable.GetOccupiedTiles())
            occupiedGridTiles.Add(tile);
    }

    /// <summary>
    /// 특정 오브젝트 제거 시 점유 타일 목록에서 제거
    /// </summary>
    /// <param name="placeable">배치 가능 오브젝트</param>
    public void UnMarkObjectOccupied(IBlueprintPlaceable placeable)
    {
        foreach (Vector2Int tile in placeable.GetOccupiedTiles())
            occupiedGridTiles.Remove(tile);
    }

    /// <summary>
    /// 디버깅 용 점유 타일 반환
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Vector2Int> GetAllOccupiedTiles()
    {
        return occupiedGridTiles;
    }

    /// <summary>
    /// 해당 오브젝트를 현재 위치와 회전 상태로 배치 가능한지 검사
    /// </summary>
    public bool CanPlaceObject(IBlueprintPlaceable placeable, Vector2Int position, object rotation)
    {
        // 1) 기존 상태 저장
        Vector2Int originalPos = placeable.GetGridPosition();
        object originalRot = placeable.GetRotation();

        // 2) 새 위치 및 회전 설정
        placeable.SetGridPosition(position);
        if (rotation != null && placeable is BlueprintRoom room)
        {
            room.bpRotation = (Constants.Rotations.Rotation)rotation;
            room.bpRoomSize = RoomRotationUtility.GetRotatedSize(
                room.bpRoomData.GetRoomDataByLevel(room.bpLevelIndex).size,
                room.bpRotation
            );
        }
        else if (rotation != null && placeable is BlueprintWeapon weapon)
        {
            weapon.bpAttachedDirection = (ShipWeaponAttachedDirection)rotation;
        }

        // 3) 점유 타일 업데이트
        if (placeable is BlueprintRoom room2)
            room2.UpdateOccupiedTiles();
        else if (placeable is BlueprintWeapon weapon2)
            weapon2.UpdateOccupiedTiles();

        List<Vector2Int> tilesToOccupy = placeable.GetOccupiedTiles();

        // 4) 그리드 범위 벗어나는지 검사
        foreach (Vector2Int tile in tilesToOccupy)
            if (!IsInGrid(tile))
            {
                // 복원
                placeable.SetGridPosition(originalPos);
                if (placeable is BlueprintRoom room3)
                {
                    room3.bpRotation = (Constants.Rotations.Rotation)originalRot;
                    room3.bpRoomSize = RoomRotationUtility.GetRotatedSize(
                        room3.bpRoomData.GetRoomDataByLevel(room3.bpLevelIndex).size,
                        room3.bpRotation
                    );
                    room3.UpdateOccupiedTiles();
                }
                else if (placeable is BlueprintWeapon weapon3)
                {
                    weapon3.bpAttachedDirection = (ShipWeaponAttachedDirection)originalRot;
                    weapon3.UpdateOccupiedTiles();
                }

                return false;
            }

        // 5) 겹침 체크 (수정된 부분)
        foreach (Vector2Int tile in tilesToOccupy)
            if (IsTileOccupied(tile))
            {
                // 복원
                placeable.SetGridPosition(originalPos);
                if (placeable is BlueprintRoom room4)
                {
                    room4.bpRotation = (Constants.Rotations.Rotation)originalRot;
                    room4.bpRoomSize = RoomRotationUtility.GetRotatedSize(
                        room4.bpRoomData.GetRoomDataByLevel(room4.bpLevelIndex).size,
                        room4.bpRotation
                    );
                    room4.UpdateOccupiedTiles();
                }
                else if (placeable is BlueprintWeapon weapon4)
                {
                    weapon4.bpAttachedDirection = (ShipWeaponAttachedDirection)originalRot;
                    weapon4.UpdateOccupiedTiles();
                }

                return false;
            }

        // 6) 모든 검사를 통과하면 원래 상태로 복원하고 true 반환
        placeable.SetGridPosition(originalPos);
        if (placeable is BlueprintRoom room5)
        {
            room5.bpRotation = (Constants.Rotations.Rotation)originalRot;
            room5.bpRoomSize = RoomRotationUtility.GetRotatedSize(
                room5.bpRoomData.GetRoomDataByLevel(room5.bpLevelIndex).size,
                room5.bpRotation
            );
            room5.UpdateOccupiedTiles();
        }
        else if (placeable is BlueprintWeapon weapon5)
        {
            weapon5.bpAttachedDirection = (ShipWeaponAttachedDirection)originalRot;
            weapon5.UpdateOccupiedTiles();
        }

        return true;
    }

    /// <summary>
    /// 해당 방을 현재 위치와 회전 상태로 배치 가능한지 검사 (이전 버전과의 호환성 유지)
    /// </summary>
    public bool CanPlaceRoom(RoomData data, int level, Vector2Int position, Constants.Rotations.Rotation rotation)
    {
        RoomData.RoomLevel levelData = data.GetRoomDataByLevel(level);
        Vector2Int size = RoomRotationUtility.GetRotatedSize(levelData.size, rotation);

        List<Vector2Int> tilesToOccupy = RoomRotationUtility.GetOccupiedGridPositions(position, size, rotation);

        // 그리드 범위 벗어나는지 체크
        foreach (Vector2Int tile in tilesToOccupy)
            if (!IsInGrid(tile))
                return false;

        // 겹침 체크
        foreach (Vector2Int tile in tilesToOccupy)
            if (IsTileOccupied(tile))
                return false;

        return true;
    }

    /// <summary>
    /// 해당 무기를 현재 위치와 부착 방향으로 배치 가능한지 검사
    /// </summary>
    public bool CanPlaceWeapon(ShipWeaponData data, Vector2Int position, ShipWeaponAttachedDirection direction)
    {
        List<Vector2Int> tilesToOccupy = RoomRotationUtility.GetOccupiedGridPositions(position, new Vector2Int(2, 1),
            Constants.Rotations.Rotation.Rotation0);

        // 그리드 범위 벗어나는지 체크
        foreach (Vector2Int tile in tilesToOccupy)
            if (!IsInGrid(tile))
                return false;

        // 겹침 체크
        foreach (Vector2Int tile in tilesToOccupy)
            if (IsTileOccupied(tile))
                return false;

        return true;
    }

    /// <summary>
    /// 실제 방을 해당 위치에 배치함
    /// </summary>
    public void PlaceRoom(RoomData data, int level, Vector2Int position, Constants.Rotations.Rotation rotation)
    {
        Vector2Int size = RoomRotationUtility.GetRotatedSize(data.GetRoomDataByLevel(level).size, rotation);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(size, rotation);
        Vector3 worldPos = GetWorldPositionFromGrid(position) + (Vector3)offset;

        GameObject bpRoomGO = Instantiate(roomPrefab, targetBlueprintShip.transform);
        bpRoomGO.transform.position = worldPos + new Vector3(0, 0, 10f);
        bpRoomGO.transform.rotation = Quaternion.Euler(0, 0, -(int)rotation * 90);

        BlueprintRoom bpRoom = bpRoomGO.GetComponent<BlueprintRoom>();
        bpRoom.SetGridPlacer(this);
        bpRoom.Initialize(data, level, position, rotation);
        bpRoom.SetBlueprint(targetBlueprintShip);

        MarkObjectOccupied(bpRoom);

        targetBlueprintShip.AddPlaceable(bpRoom);
    }

    /// <summary>
    /// 실제 무기를 해당 위치에 배치함
    /// </summary>
    /// <returns>생성된 BlueprintWeapon 인스턴스</returns>
    public BlueprintWeapon PlaceWeapon(ShipWeaponData data, Vector2Int position, ShipWeaponAttachedDirection direction)
    {
        if (data == null)
            return null;

        Vector2Int size = new(2, 1);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(size, Constants.Rotations.Rotation.Rotation0);
        Vector3 worldPos = GetWorldPositionFromGrid(position) + (Vector3)offset;

        GameObject bpWeaponGO = Instantiate(weaponPrefab, targetBlueprintShip.transform);
        bpWeaponGO.transform.position = worldPos + new Vector3(0, 0, 10f);

        BlueprintWeapon bpWeapon = bpWeaponGO.GetComponent<BlueprintWeapon>();
        bpWeapon.SetGridPlacer(this);
        bpWeapon.Initialize(data, position, direction);

        // 설계도 함선의 외갑판 레벨 적용 (함선에 추가하기 전)
        int currentHullLevel = targetBlueprintShip.GetHullLevel();
        bpWeapon.SetHullLevel(currentHullLevel);
        bpWeapon.SetBlueprint(targetBlueprintShip);

        MarkObjectOccupied(bpWeapon);

        // 함선에 무기 추가 (BlueprintShip.AddPlaceable 메서드에서 다시 한번 외갑판 레벨 적용)
        targetBlueprintShip.AddPlaceable(bpWeapon);

        return bpWeapon;
    }

    #region BPPreviewArea 용 함수

    /// <summary>
    /// 실제 방을 해당 위치에 배치함
    /// </summary>
    public GameObject PlacePreviewRoom(RoomData data, int level, Vector2Int position, Constants.Rotations.Rotation rotation, Transform previewRoot, List<Vector2Int> tilePos)
    {
        Vector2Int size = RoomRotationUtility.GetRotatedSize(data.GetRoomDataByLevel(level).size, rotation);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(size, rotation);
        Vector3 worldPos = GetWorldPositionFromGrid(position) + (Vector3)offset;

        GameObject previewObj = Instantiate(roomPrefab, previewRoot);
        previewObj.transform.position = worldPos + new Vector3(0, 0, 10f);
        previewObj.transform.rotation = Quaternion.Euler(0, 0, -(int)rotation * 90);
        previewObj.name = $"PreviewRoom_{data.name}";

        BlueprintRoom bpRoom = previewObj.GetComponent<BlueprintRoom>();
        // bpRoom.SetGridPlacer(this);
        bpRoom.Initialize(data, level, position, rotation);
        // bpRoom.SetBlueprint(targetBlueprintShip);

        foreach (Vector2Int tile in bpRoom.GetOccupiedTiles())
            tilePos.Add(tile);

        return previewObj;
    }

    /// <summary>
    /// 실제 무기를 해당 위치에 배치함
    /// </summary>
    /// <returns>생성된 BlueprintWeapon 인스턴스</returns>
    public GameObject PlacePreviewWeapon(ShipWeaponData data, Vector2Int position, ShipWeaponAttachedDirection direction, Transform previewRoot, List<Vector2Int> tilePos)
    {
        if (data == null)
            return null;

        Vector2Int size = new(2, 1);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(size, Constants.Rotations.Rotation.Rotation0);
        Vector3 worldPos = GetWorldPositionFromGrid(position) + (Vector3)offset;

        GameObject previewObj = Instantiate(weaponPrefab, previewRoot);
        previewObj.transform.position = worldPos + new Vector3(0, 0, 10f);
        previewObj.name = $"PreviewWeapon_{data.name}";

        BlueprintWeapon bpWeapon = previewObj.GetComponent<BlueprintWeapon>();
        // bpWeapon.SetGridPlacer(this);
        bpWeapon.Initialize(data, position, direction);

        // 설계도 함선의 외갑판 레벨 적용 (함선에 추가하기 전)
        int currentHullLevel = targetBlueprintShip.GetHullLevel();
        bpWeapon.SetHullLevel(currentHullLevel);
        // bpWeapon.SetBlueprint(targetBlueprintShip);

        foreach (Vector2Int tile in bpWeapon.GetOccupiedTiles())
            tilePos.Add(tile);

        // 함선에 무기 추가 (BlueprintShip.AddPlaceable 메서드에서 다시 한번 외갑판 레벨 적용)
        // targetBlueprintShip.AddPlaceable(bpWeapon);

        return previewObj;
    }

    #endregion
}
