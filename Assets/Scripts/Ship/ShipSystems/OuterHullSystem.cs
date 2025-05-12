using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 외갑판 시스템.
/// 공격에 대한 피해를 감소시키는 기능을 담당합니다.
/// </summary>
public class OuterHullSystem : ShipSystem
{
    /// <summary>
    /// 현재 생성된 외갑판 객체들
    /// </summary>
    private List<OuterHull> currentOuterHulls = new();

    /// <summary>
    /// 외부 선체 시스템의 현재 레벨을 나타냅니다.
    /// 이 레벨은 들어오는 공격에 대해 적용되는 피해 감소량에 영향을 줄 수 있습니다.
    /// </summary>
    private int currentOuterHullLevel;

    /// <summary>
    /// 시스템 초기화
    /// </summary>
    /// <param name="ship">소유 함선</param>
    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);
    }

    /// <summary>
    /// 매 프레임마다 호출되어 시스템 상태를 갱신합니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
    public override void Update(float deltaTime)
    {
    }

    /// <summary>
    /// 외부 선체의 피해 감소율을 적용하여 실질적인 피해량을 계산합니다.
    /// </summary>
    /// <param name="damage">입력된 원래 피해량.</param>
    /// <returns>피해 감소가 적용된 최종 피해량.</returns>
    public float ReduceDamage(float damage)
    {
        float damageAfterHull = damage * (100 - GetShipStat(ShipStat.DamageReduction)) / 100;
        return damageAfterHull;
    }

    /// <summary>
    /// 외부 선체 시스템의 현재 레벨을 반환합니다.
    /// </summary>
    /// <returns>외부 선체 시스템의 현재 레벨.</returns>
    public int GetOuterHullLevel()
    {
        return currentOuterHullLevel;
    }

    /// <summary>
    /// 함선의 외갑판 레벨을 설정합니다. 모든 무기의 외형도 함께 업데이트합니다.
    /// </summary>
    /// <param name="level">설정할 외갑판 레벨 (0-2)</param>
    public void SetOuterHullLevel(int level)
    {
        if (level >= 0 && level < 3)
        {
            currentOuterHullLevel = level;

            // 외갑판 레벨이 변경되면 모든 무기 스프라이트 업데이트
            List<ShipWeapon> weapons = parentShip.GetAllWeapons();
            foreach (ShipWeapon weapon in weapons) weapon.ApplyRotationSprite(level);

            // 모든 외갑판 객체의 레벨 업데이트
            foreach (OuterHull hull in currentOuterHulls)
                if (hull != null)
                    hull.SetLevel(level);

            Debug.Log(
                $"Ship outer hull level set to {level + 1}, updated {weapons.Count} weapons and {currentOuterHulls.Count} hull tiles");
        }
        else
        {
            Debug.LogWarning($"Invalid outer hull level: {level}. Valid range is 0-2.");
        }
    }

    /// <summary>
    /// 외갑판의 시각적 표현을 업데이트합니다.
    /// 방 주변에 외갑판을 생성하고, 필요한 위치에 내부 모서리 외갑판도 생성합니다.
    /// </summary>
    /// <param name="level">적용할 외갑판 레벨 (0-2)</param>
    public void UpdateVisuals(int level)
    {
        // 기존 외갑판 제거
        ClearExistingHulls();

        // 함선으로부터 외갑판 데이터와 프리팹 가져오기
        OuterHullData outerHullData = parentShip.GetOuterHullData();
        GameObject outerHullPrefab = parentShip.GetOuterHullPrefab();

        if (outerHullData == null || outerHullPrefab == null)
        {
            Debug.LogError("외갑판 데이터 또는 프리팹이 설정되지 않았습니다.");
            return;
        }

        // 방 그리드 좌표 세트 생성 (점유 중인 타일)
        HashSet<Vector2Int> occupiedTiles = new();
        foreach (Room room in parentShip.GetAllRooms())
        {
            // 모든 방의 점유 타일 수집
            Vector2Int roomPos = room.position;
            Vector2Int roomSize = room.GetSize();
            Vector2Int rotatedSize = RoomRotationUtility.GetRotatedSize(roomSize, room.currentRotation);

            List<Vector2Int> roomTiles =
                RoomRotationUtility.GetOccupiedGridPositions(roomPos, rotatedSize, room.currentRotation);
            foreach (Vector2Int tile in roomTiles) occupiedTiles.Add(tile);
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

                // 그리드 범위 체크
                if (IsOutOfBounds(neighborTile))
                    continue;

                // 방이 있는 타일은 건너뜀
                if (occupiedTiles.Contains(neighborTile))
                    continue;

                // 타일 정보 초기화
                if (!hullPlacements.ContainsKey(neighborTile))
                    hullPlacements[neighborTile] = new List<HullPlacementInfo>();

                // 직선 방향 외갑판 정보 생성
                HullPlacementInfo straightInfo = new();
                int oppositeDir = (i + 2) % 4; // 0->2(하->상), 1->3(좌->우), 2->0(상->하), 3->1(우->좌)
                straightInfo.type = HullType.Straight;
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
                if (IsOutOfBounds(diagonalTile) || occupiedTiles.Contains(diagonalTile))
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
                bool canPlaceOuterCorner =
                    !IsOutOfBounds(checkTile1) && !occupiedTiles.Contains(checkTile1) &&
                    !IsOutOfBounds(checkTile2) && !occupiedTiles.Contains(checkTile2);

                if (canPlaceOuterCorner)
                {
                    // 타일 정보 초기화
                    if (!hullPlacements.ContainsKey(diagonalTile))
                        hullPlacements[diagonalTile] = new List<HullPlacementInfo>();

                    // 외부 모서리 정보 생성 - 위치는 방의 반대쪽 모서리 방향 (예: 좌하단에 방이 있으면 우상단 모서리)
                    HullPlacementInfo outerCornerInfo = new();
                    outerCornerInfo.type = HullType.OuterCorner;
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
                if (IsOutOfBounds(adjacentTile) || occupiedTiles.Contains(adjacentTile))
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
                    if (IsOutOfBounds(checkTile) || !occupiedTiles.Contains(checkTile))
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
                    innerCornerInfo.type = HullType.InnerCorner;
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
                CreateOuterHull(hullPosition, spriteIndex, level, outerHullData, outerHullPrefab, placementInfo.type);
            }
        }

        Debug.Log($"Created {currentOuterHulls.Count} outer hull tiles at level {level + 1}");
    }

    /// <summary>
    /// 내부 모서리 타일 추가
    /// </summary>
    private void AddInnerCorner(List<HullPlacementInfo> placements, int directionIndex)
    {
        HullPlacementInfo innerCornerInfo = new();
        innerCornerInfo.type = HullType.InnerCorner;
        innerCornerInfo.directionIndex = directionIndex;
        AddPlacementIfNotDuplicate(placements, innerCornerInfo);
    }

    /// <summary>
    /// 외부 모서리 타일 추가
    /// </summary>
    private void AddOuterCorner(List<HullPlacementInfo> placements, int directionIndex)
    {
        HullPlacementInfo outerCornerInfo = new();
        outerCornerInfo.type = HullType.OuterCorner;
        outerCornerInfo.directionIndex = directionIndex;
        AddPlacementIfNotDuplicate(placements, outerCornerInfo);
    }

    /// <summary>
    /// 그리드 범위를 벗어났는지 확인
    /// </summary>
    private bool IsOutOfBounds(Vector2Int pos)
    {
        return pos.x < 0 || pos.y < 0 ||
               pos.x >= parentShip.GetGridSize().x ||
               pos.y >= parentShip.GetGridSize().y;
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
            case HullType.Straight:
                // 기본(0-3) 또는 변형(4-7)
                return info.directionIndex + (info.useVariation ? 4 : 0);

            case HullType.OuterCorner:
                // 외부 모서리: 8-11
                return info.directionIndex + 8;

            case HullType.InnerCorner:
                // 내부 모서리: 12-15
                return info.directionIndex + 12;

            default:
                return 0;
        }
    }

    /// <summary>
    /// 외갑판 유형
    /// </summary>
    private enum HullType
    {
        Straight, // 상하좌우 직선
        OuterCorner, // 외부 모서리
        InnerCorner // 내부 모서리
    }

    /// <summary>
    /// 외갑판 배치 정보
    /// </summary>
    private class HullPlacementInfo
    {
        // 외갑판 유형
        public HullType type = HullType.Straight;

        // 방향 인덱스
        // Straight: 0=하, 1=좌, 2=상, 3=우
        // OuterCorner/InnerCorner: 0=하좌, 1=상좌, 2=상우, 3=하우
        public int directionIndex = 0;

        // 변형 사용 여부 (직선 방향에만 적용)
        public bool useVariation = false;
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
        GameObject hullPrefab, HullType hullType)
    {
        if (hullPrefab == null)
        {
            Debug.LogError("외갑판 프리팹이 설정되지 않았습니다.");
            return;
        }

        // 프리팹 인스턴스 생성
        GameObject hullObj = GameObject.Instantiate(hullPrefab, parentShip.transform);

        // 위치 설정 (그리드 -> 월드 좌표 변환)
        Vector3 worldPos = parentShip.GetWorldPositionFromGrid(hullPosition);

        // Z 위치 조정 (유형에 따라 레이어링)
        float zOffset = 15f; // 직선 방향은 가장 뒤에

        switch (hullType)
        {
            case HullType.InnerCorner:
                // 내부 모서리는 맨 앞에 배치
                zOffset = 14f;
                break;

            case HullType.OuterCorner:
                // 외부 모서리는 중간에 배치
                zOffset = 14.5f;
                break;
        }

        hullObj.transform.position = worldPos + new Vector3(0, 0, zOffset);

        // 레벨에 맞는 스프라이트 가져오기
        Sprite sprite = hullData.GetSpecificHullSprite(level, spriteIndex);

        if (sprite == null)
        {
            Debug.LogWarning($"외갑판 스프라이트가 없습니다: 레벨 {level}, 인덱스 {spriteIndex}");
            GameObject.Destroy(hullObj);
            return;
        }

        // OuterHull 컴포넌트 설정
        OuterHull outerHull = hullObj.GetComponent<OuterHull>();
        if (outerHull == null) outerHull = hullObj.AddComponent<OuterHull>();

        // 데이터 참조 설정
        outerHull.SetOuterHullData(hullData);

        // 외갑판 초기화
        outerHull.Initialize(hullPosition, level, spriteIndex, sprite, parentShip);

        // 디버그 로그
        string typeStr = hullType.ToString();
        string dirStr = "";

        if (hullType == HullType.Straight)
        {
            string[] dirs = { "하", "좌", "상", "우" };
            dirStr = dirs[spriteIndex % 4];
            if (spriteIndex >= 4) dirStr += "(변형)";
        }
        else
        {
            string[] corners = { "하좌", "상좌", "상우", "하우" };
            dirStr = corners[spriteIndex % 4];
        }

        // Debug.Log($"외갑판 생성: 위치 {hullPosition}, 유형 {typeStr}, 방향 {dirStr}, 스프라이트 {spriteIndex}");

        // 생성된 외갑판 저장
        currentOuterHulls.Add(outerHull);
    }

    /// <summary>
    /// 기존 외갑판 객체들을 모두 제거합니다.
    /// </summary>
    public void ClearExistingHulls()
    {
        foreach (OuterHull hull in currentOuterHulls)
            if (hull != null)
                GameObject.Destroy(hull.gameObject);

        currentOuterHulls.Clear();
    }
}
