using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HullPlacementInfo
{
    public OuterHullSystem.HullType type;
    public int directionIndex;
    public bool useVariation = false;
}

/// <summary>
/// 실제 설계도 데이터를 관리하는 인스턴스 클래스. 방/무기 배치, 가격 계산, 중심 좌표 제공.
/// </summary>
public class BlueprintShip : MonoBehaviour
{
    [SerializeField] private Transform outerHulls;
    [SerializeField] private Vector2Int gridSize = new(60, 60);
    [SerializeField] private List<IBlueprintPlaceable> placedBlueprintObjects = new();

    /// <summary>
    /// 함선 외갑판 레벨 (0: 레벨 1, 1: 레벨 2, 2: 레벨 3) - 함선 전체에 적용되는 하나의 값
    /// </summary>
    [SerializeField] private int currentHullLevel = 0;
    public int CurrentHullLevel => currentHullLevel;

    // 이전 코드와의 호환성을 위한 프로퍼티
    public List<BlueprintRoom> PlacedBlueprintRooms =>
        placedBlueprintObjects.OfType<BlueprintRoom>().ToList();

    public List<BlueprintWeapon> PlacedBlueprintWeapons =>
        placedBlueprintObjects.OfType<BlueprintWeapon>().ToList();

    /// <summary>
    /// 현재 설계도에 배치된 방 리스트
    /// </summary>
    public IReadOnlyList<BlueprintRoom> GetPlacedBlueprintRooms()
    {
        return PlacedBlueprintRooms;
    }

    /// <summary>
    /// 현재 설계도에 배치된 무기 리스트
    /// </summary>
    public IReadOnlyList<BlueprintWeapon> GetPlacedBlueprintWeapons()
    {
        return PlacedBlueprintWeapons;
    }

    /// <summary>
    /// 함선 설계도의 총 가격 반환
    /// </summary>
    /// <returns></returns>
    public int GetTotalBPCost()
    {
        int sum = 0;

        foreach (IBlueprintPlaceable placeable in placedBlueprintObjects)
            sum += placeable.GetCost();

        return sum;
    }

    /// <summary>
    /// 설계도에 배치된 모든 오브젝트 리스트 초기화
    /// </summary>
    public void ClearPlacedBPObjects()
    {
        placedBlueprintObjects.Clear();
    }

    /// <summary>
    /// 현재 함선의 외갑판 레벨 설정 - 모든 무기에 적용
    /// </summary>
    /// <param name="level">설정할 외갑판 레벨 (0-2)</param>
    public void SetHullLevel(int level)
    {
        if (level >= 0 && level < 3)
        {
            currentHullLevel = level;

            // 모든 무기에 일괄 적용
            foreach (BlueprintWeapon weapon in PlacedBlueprintWeapons)
                weapon.SetHullLevel(level);

            Debug.Log($"Ship hull level set to {level + 1}");
        }
    }

    /// <summary>
    /// 현재 함선의 외갑판 레벨 반환
    /// </summary>
    /// <returns>현재 외갑판 레벨 (0-2)</returns>
    public int GetHullLevel()
    {
        return currentHullLevel;
    }

    /// <summary>
    /// 유저가 배치한 모든 오브젝트의 중심값 호출 (함선의 중심을 기준으로 유저에게 보이는 UI 설계)
    /// </summary>
    public Vector2Int CenterPosition
    {
        get
        {
            if (placedBlueprintObjects.Count == 0)
                return gridSize / 2;

            // 모든 점유 타일 수집
            List<Vector2Int> allTiles = new();
            foreach (IBlueprintPlaceable placeable in placedBlueprintObjects)
                allTiles.AddRange(placeable.GetOccupiedTiles());

            int minX = allTiles.Min(t => t.x);
            int maxX = allTiles.Max(t => t.x);
            int minY = allTiles.Min(t => t.y);
            int maxY = allTiles.Max(t => t.y);

            return new Vector2Int((minX + maxX) / 2, (minY + maxY) / 2);
        }
    }

    /// <summary>
    /// 인자 gridPos에 위치한 오브젝트 반환
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public IBlueprintPlaceable GetObjectAt(Vector2Int gridPos)
    {
        foreach (IBlueprintPlaceable placeable in placedBlueprintObjects)
            if (placeable.GetOccupiedTiles().Contains(gridPos))
                return placeable;
        return null;
    }

    /// <summary>
    /// 설계도에 오브젝트를 추가하고 가격을 갱신합니다.
    /// </summary>
    /// <param name="placeable">배치 가능 오브젝트</param>
    public void AddPlaceable(IBlueprintPlaceable placeable)
    {
        placedBlueprintObjects.Add(placeable);

        // 무기인 경우 현재 함선의 외갑판 레벨 적용
        if (placeable is BlueprintWeapon weapon)
            weapon.SetHullLevel(currentHullLevel);
    }

    /// <summary>
    /// 설계도에서 오브젝트를 제거하고 가격을 갱신합니다.
    /// </summary>
    /// <param name="placeable">배치 가능 오브젝트</param>
    public void RemovePlaceable(IBlueprintPlaceable placeable)
    {
        placedBlueprintObjects.Remove(placeable);
    }

    /// <summary>
    /// 설계도 초기화
    /// </summary>
    public void ClearRooms()
    {
        foreach (IBlueprintPlaceable placeable in placedBlueprintObjects.ToList())
            Destroy(((MonoBehaviour)placeable).gameObject);

        placedBlueprintObjects.Clear();
    }

    #region bpship 용 외갑판

    private List<GameObject> previewOuterHulls = new();

    /// <summary>
    /// 외갑판 세팅
    /// </summary>
    /// <param name="level"></param>
    /// <param name="previewOuterHullPrefab"></param>
    public void SetBPHullLevel(int level, GameObject previewOuterHullPrefab)
    {
        ClearPreviewOuterHulls();

        currentHullLevel = level;
        if (level < 0)
            return;

        // 1. 무기 외갑판 sprite 적용
        if (PlacedBlueprintWeapons.Count > 0)
            foreach (BlueprintWeapon weapon in PlacedBlueprintWeapons)
                weapon.SetHullLevel(currentHullLevel);

        OuterHullData outerHullData = GameManager.Instance.playerShip.outerHullData;
        if (outerHullData == null || previewOuterHullPrefab == null)
        {
            Debug.LogError("외갑판 데이터 또는 프리팹이 설정되지 않았습니다.");
            return;
        }

        // 방 그리드 좌표 세트 생성 (점유 중인 타일)
        HashSet<Vector2Int> occupiedTiles = new();

        if (PlacedBlueprintRooms.Count <= 0)
            return;

        foreach (BlueprintRoom room in PlacedBlueprintRooms)
        {
            Vector2Int size = room.bpRoomData.GetRoomDataByLevel(room.bpLevelIndex).size;
            Vector2Int rotatedSize = RoomRotationUtility.GetRotatedSize(size, room.bpRotation);
            List<Vector2Int> tiles = RoomRotationUtility.GetOccupiedGridPositions(room.bpPosition, rotatedSize, room.bpRotation);

            foreach (Vector2Int tile in tiles)
                occupiedTiles.Add(tile);
        }

        // 외갑판 정보를 저장할 딕셔너리
        // 각 타일 위치에 설치할 외갑판 정보를 저장
        Dictionary<Vector2Int, List<HullPlacementInfo>> hullPlacements = new();

        // 방향 벡터 (상하좌우)
        Vector2Int[] straightDirections = new Vector2Int[4]
        {
                new(0, -1), // 하 (0)
                new(-1, 0), // 좌 (1)
                new(0, 1), // 상 (2)
                new(1, 0) // 우 (3)
        };

        // 대각선 방향 벡터 (하좌, 상좌, 상우, 하우)
        Vector2Int[] diagonalDirections = new Vector2Int[4]
        {
                new(-1, -1), // 하좌 (0)
                new(-1, 1), // 상좌 (1)
                new(1, 1), // 상우 (2)
                new(1, -1) // 하우 (3)
        };

        // 1. 직선 방향 외갑판 처리
        // 모든 방의 상하좌우 인접 타일 검사
        foreach (Vector2Int tile in occupiedTiles)
            // 상하좌우 4방향 검사
            for (int i = 0; i < 4; i++)
            {
                Vector2Int neighborTile = tile + straightDirections[i];

                // 방이 있는 타일은 건너뜀
                if (occupiedTiles.Contains(neighborTile))
                    continue;

                // 타일 정보 초기화
                if (!hullPlacements.ContainsKey(neighborTile))
                    hullPlacements[neighborTile] = new List<HullPlacementInfo>();

                // 직선 방향 외갑판 정보 생성
                HullPlacementInfo straightInfo = new();
                int oppositeDir = (i + 2) % 4; // 0->2(하->상), 1->3(좌->우), 2->0(상->하), 3->1(우->좌)
                straightInfo.type = OuterHullSystem.HullType.Straight;
                straightInfo.directionIndex = oppositeDir;
                straightInfo.useVariation = Random.Range(0, 100) < 30; // 30% 확률로 변형 사용

                // 중복 검사 및 추가
                AddPlacementIfNotDuplicate(hullPlacements[neighborTile], straightInfo);
            }

        // 2. 외부 모서리 외갑판 처리
        foreach (Vector2Int tile in occupiedTiles)
            // 대각선 4방향 검사 (하좌, 상좌, 상우, 하우)
            for (int i = 0; i < 4; i++)
            {
                // 방 기준 대각선 위치의 타일
                Vector2Int diagonalTile = tile + diagonalDirections[i];

                // 범위를 벗어나거나 방이 있는 경우 건너뜀
                if (occupiedTiles.Contains(diagonalTile))
                    continue;

                // 대각선 반대 방향 인덱스 계산 (0->2, 1->3, 2->0, 3->1)
                int oppositeCornerIndex = (i + 2) % 4;

                // 대각선 위치에서 확인해야 할 두 방향 결정
                // 예: 하좌(0) 대각선 위치면 우(3)와 상(2) 방향을 확인
                //     상좌(1) 대각선 위치면 우(3)와 하(0) 방향을 확인
                //     상우(2) 대각선 위치면 좌(1)와 하(0) 방향을 확인
                //     하우(3) 대각선 위치면 좌(1)와 상(2) 방향을 확인
                int dirToCheck1 = -1;
                int dirToCheck2 = -1;

                switch (i)
                {
                    case 0: // 하좌 대각선이면 우(3)와 상(2) 확인
                        dirToCheck1 = 3;
                        dirToCheck2 = 2;
                        break;
                    case 1: // 상좌 대각선이면 우(3)와 하(0) 확인
                        dirToCheck1 = 3;
                        dirToCheck2 = 0;
                        break;
                    case 2: // 상우 대각선이면 좌(1)와 하(0) 확인
                        dirToCheck1 = 1;
                        dirToCheck2 = 0;
                        break;
                    case 3: // 하우 대각선이면 좌(1)와 상(2) 확인
                        dirToCheck1 = 1;
                        dirToCheck2 = 2;
                        break;
                }

                // 해당 방향의 타일 확인
                Vector2Int checkTile1 = diagonalTile + straightDirections[dirToCheck1];
                Vector2Int checkTile2 = diagonalTile + straightDirections[dirToCheck2];

                // 두 방향 모두 방이 없는 경우에만 외부 모서리 설치
                bool canPlaceOuterCorner = !occupiedTiles.Contains(checkTile1) && !occupiedTiles.Contains(checkTile2);

                if (canPlaceOuterCorner)
                {
                    // 타일 정보 초기화
                    if (!hullPlacements.ContainsKey(diagonalTile))
                        hullPlacements[diagonalTile] = new List<HullPlacementInfo>();

                    // 외부 모서리 정보 생성 - 위치는 방의 반대쪽 모서리 방향 (예: 좌하단에 방이 있으면 우상단 모서리)
                    HullPlacementInfo outerCornerInfo = new();
                    outerCornerInfo.type = OuterHullSystem.HullType.OuterCorner;
                    outerCornerInfo.directionIndex = oppositeCornerIndex;

                    AddPlacementIfNotDuplicate(hullPlacements[diagonalTile], outerCornerInfo);
                }
            }

        // 3. 내부 모서리 외갑판 처리
        foreach (Vector2Int tile in occupiedTiles)
            // 방 기준 상하좌우 4방향 순회
            for (int i = 0; i < 4; i++)
            {
                // 방 기준 인접한 타일
                Vector2Int adjacentTile = tile + straightDirections[i];

                // 범위를 벗어나거나 방이 있는 경우 건너뜀
                if (occupiedTiles.Contains(adjacentTile))
                    continue;

                // adjacentTile 기준으로 확인해야 할 두 방향
                int[] checkDirs = new int[2];

                switch (i)
                {
                    case 0: // 하단 방향을 순회했다면 (방은 상단에 있음)
                            // 좌측과 우측 확인
                        checkDirs[0] = 1; // 좌
                        checkDirs[1] = 3; // 우
                        break;
                    case 1: // 좌측 방향을 순회했다면 (방은 우측에 있음)
                            // 상단과 하단 확인
                        checkDirs[0] = 0; // 하
                        checkDirs[1] = 2; // 상
                        break;
                    case 2: // 상단 방향을 순회했다면 (방은 하단에 있음)
                            // 좌측과 우측 확인
                        checkDirs[0] = 1; // 좌
                        checkDirs[1] = 3; // 우
                        break;
                    case 3: // 우측 방향을 순회했다면 (방은 좌측에 있음)
                            // 상단과 하단 확인
                        checkDirs[0] = 0; // 하
                        checkDirs[1] = 2; // 상
                        break;
                }

                // 두 방향에 대해 처리
                for (int j = 0; j < 2; j++)
                {
                    int checkDir = checkDirs[j];

                    // adjacentTile 기준으로 해당 방향의 타일
                    Vector2Int checkTile = adjacentTile + straightDirections[checkDir];

                    // 범위를 벗어나거나 방이 없는 경우 건너뜀
                    if (!occupiedTiles.Contains(checkTile))
                        continue;

                    int innerCornerDir = -1;

                    // 내부 모서리 방향 결정
                    if (i == 0) // 하단 방향을 순회한 경우 (방은 상단에 있음)
                    {
                        if (checkDir == 1) // 좌측 확인 - 좌상단 내부 모서리
                            innerCornerDir = 1; // 상좌
                        else if (checkDir == 3) // 우측 확인 - 우상단 내부 모서리
                            innerCornerDir = 2; // 상우
                    }
                    else if (i == 1) // 좌측 방향을 순회한 경우 (방은 우측에 있음)
                    {
                        if (checkDir == 0) // 하단 확인 - 우하단 내부 모서리
                            innerCornerDir = 3; // 하우
                        else if (checkDir == 2) // 상단 확인 - 우상단 내부 모서리
                            innerCornerDir = 2; // 상우
                    }
                    else if (i == 2) // 상단 방향을 순회한 경우 (방은 하단에 있음)
                    {
                        if (checkDir == 1) // 좌측 확인 - 좌하단 내부 모서리
                            innerCornerDir = 0; // 하좌
                        else if (checkDir == 3) // 우측 확인 - 우하단 내부 모서리
                            innerCornerDir = 3; // 하우
                    }
                    else if (i == 3) // 우측 방향을 순회한 경우 (방은 좌측에 있음)
                    {
                        if (checkDir == 0) // 하단 확인 - 좌하단 내부 모서리
                            innerCornerDir = 0; // 하좌
                        else if (checkDir == 2) // 상단 확인 - 좌상단 내부 모서리
                            innerCornerDir = 1; // 상좌
                    }

                    // 타일 정보 초기화
                    if (!hullPlacements.ContainsKey(adjacentTile))
                        hullPlacements[adjacentTile] = new List<HullPlacementInfo>();

                    // 내부 모서리 정보 생성
                    HullPlacementInfo innerCornerInfo = new();
                    innerCornerInfo.type = OuterHullSystem.HullType.InnerCorner;
                    innerCornerInfo.directionIndex = innerCornerDir;

                    AddPlacementIfNotDuplicate(hullPlacements[adjacentTile], innerCornerInfo);
                }
            }

        // 3. 모든 외갑판 생성
        foreach (KeyValuePair<Vector2Int, List<HullPlacementInfo>> kvp in hullPlacements)
        {
            Vector2Int hullPosition = kvp.Key;
            List<HullPlacementInfo> placements = kvp.Value;

            // 각 타일별 배치 정보에 따라 외갑판 생성
            foreach (HullPlacementInfo placementInfo in placements)
            {
                int spriteIndex = CalculateSpriteIndex(placementInfo);
                CreateOuterHull(hullPosition, spriteIndex, level, outerHullData, previewOuterHullPrefab, placementInfo.type);
            }
        }
    }

    /// <summary>
    /// 중복 검사 후 배치 정보 추가
    /// </summary>
    private void AddPlacementIfNotDuplicate(List<HullPlacementInfo> placements, HullPlacementInfo newInfo)
    {
        // 동일한 유형과 방향의 외갑판이 이미 있는지 확인
        foreach (HullPlacementInfo existing in placements)
            if (existing.type == newInfo.type && existing.directionIndex == newInfo.directionIndex)
                return; // 중복이면 추가하지 않음

        // 중복이 아니면 추가
        placements.Add(newInfo);
    }

    /// <summary>
    /// 외갑판 배치 정보에 따른 스프라이트 인덱스 계산
    /// </summary>
    private int CalculateSpriteIndex(HullPlacementInfo info)
    {
        switch (info.type)
        {
            case OuterHullSystem.HullType.Straight:
                // 기본(0-3) 또는 변형(4-7)
                return info.directionIndex + (info.useVariation ? 4 : 0);

            case OuterHullSystem.HullType.OuterCorner:
                // 외부 모서리: 8-11
                return info.directionIndex + 8;

            case OuterHullSystem.HullType.InnerCorner:
                // 내부 모서리: 12-15
                return info.directionIndex + 12;

            default:
                return 0;
        }
    }

    /// <summary>
    /// 지정된 위치에 외갑판을 생성합니다.
    /// </summary>
    /// <param name="hullPosition">외갑판 위치</param>
    /// <param name="spriteIndex">스프라이트 인덱스
    /// (0-3: 하좌상우 기본, 4-7: 하좌상우 변형, 8-11: 외부 모서리, 12-15: 내부 모서리)</param>
    /// <param name="level">외갑판 레벨 (0-2)</param>
    /// <param name="hullData">외갑판 데이터</param>
    /// <param name="hullPrefab">외갑판 프리팹</param>
    /// <param name="hullType">외갑판 유형</param>
    private void CreateOuterHull(Vector2Int hullPosition, int spriteIndex, int level, OuterHullData hullData,
        GameObject hullPrefab, OuterHullSystem.HullType hullType)
    {
        if (hullPrefab == null)
        {
            Debug.LogError("외갑판 프리팹이 설정되지 않았습니다.");
            return;
        }

        GameObject hullObj = GameObject.Instantiate(hullPrefab, outerHulls);
        Vector3 worldPos = GameManager.Instance.playerShip.GetWorldPositionFromGrid(hullPosition);

        // Z 위치 조정 (유형에 따라 레이어링)
        float zOffset = 15f; // 직선 방향은 가장 뒤에

        switch (hullType)
        {
            case OuterHullSystem.HullType.InnerCorner:
                // 내부 모서리는 맨 앞에 배치
                zOffset = 14f;
                break;

            case OuterHullSystem.HullType.OuterCorner:
                // 외부 모서리는 중간에 배치
                zOffset = 14.5f;
                break;
        }

        hullObj.transform.position = worldPos + new Vector3(0, 0, zOffset);

        SpriteRenderer sr = hullObj.GetComponent<SpriteRenderer>();
        sr.sprite = hullData.GetSpecificHullSprite(level, spriteIndex);

        previewOuterHulls.Add(hullObj);
    }

    public void ClearPreviewOuterHulls()
    {
        foreach (GameObject go in previewOuterHulls)
            Destroy(go);
        previewOuterHulls.Clear();
    }

    #endregion
}
