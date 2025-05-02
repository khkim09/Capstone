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

        // 시스템 초기화시 외갑판 레벨 설정 (기본값 0)
        currentOuterHullLevel = 0;
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
    /// 방 주변에 외갑판을 생성하고 적절한 스프라이트를 적용합니다.
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

        // 외갑판 생성 작업
        HashSet<Vector2Int> hullTiles = new();
        Dictionary<Vector2Int, OuterHullTileInfo> hullInfo = new();

        // 방향 벡터 (상, 우, 하, 좌)
        Vector2Int[] directions = new Vector2Int[4]
        {
            new(0, 1), // 상
            new(1, 0), // 우
            new(0, -1), // 하
            new(-1, 0) // 좌
        };

        // 1단계: 각 점유 타일의 인접한 빈 타일을 외갑판으로 지정
        foreach (Vector2Int tile in occupiedTiles)
            for (int i = 0; i < 4; i++)
            {
                Vector2Int neighborTile = tile + directions[i];

                // 이미 방이 있는 경우 스킵
                if (occupiedTiles.Contains(neighborTile))
                    continue;

                // 그리드 범위 체크
                if (neighborTile.x < 0 || neighborTile.y < 0 ||
                    neighborTile.x >= parentShip.GetGridSize().x ||
                    neighborTile.y >= parentShip.GetGridSize().y)
                    continue;

                // 외갑판 타일 추가
                hullTiles.Add(neighborTile);

                // 외갑판 방향 정보 저장 (인접한 방 위치 기반)
                if (!hullInfo.ContainsKey(neighborTile)) hullInfo[neighborTile] = new OuterHullTileInfo();

                // 방향 플래그 설정 (반대 방향)
                int oppositeDir = (i + 2) % 4; // 0 -> 2, 1 -> 3, 2 -> 0, 3 -> 1
                hullInfo[neighborTile].directionFlags |= 1 << oppositeDir;
            }

        // 2단계: 각 외갑판 타일에 적절한 스프라이트 할당
        foreach (Vector2Int hullTile in hullTiles)
        {
            OuterHullTileInfo info = hullInfo[hullTile];
            int dirFlags = info.directionFlags;

            // 방향 플래그에 따라 스프라이트 인덱스 결정
            int spriteIndex = 0;

            // 모서리 체크 (두 방향으로 방에 인접한 경우)
            if (BitCount(dirFlags) == 2)
            {
                // 모서리 인덱스 결정 (상우:4, 하우:5, 하좌:6, 상좌:7)
                if ((dirFlags & 0b0101) == 0b0101) spriteIndex = 4; // 상우
                else if ((dirFlags & 0b0110) == 0b0110) spriteIndex = 5; // 하우
                else if ((dirFlags & 0b1010) == 0b1010) spriteIndex = 6; // 하좌
                else if ((dirFlags & 0b1001) == 0b1001) spriteIndex = 7; // 상좌
            }
            else
            {
                // 단일 방향 (상:0, 우:1, 하:2, 좌:3)
                for (int i = 0; i < 4; i++)
                    if ((dirFlags & (1 << i)) != 0)
                    {
                        spriteIndex = i;
                        break;
                    }
            }

            // 외갑판 게임 오브젝트 생성
            CreateOuterHull(hullTile, spriteIndex, level, outerHullData, outerHullPrefab);
        }

        Debug.Log($"Created {hullTiles.Count} outer hull tiles at level {level + 1}");
    }

    /// <summary>
    /// 비트 카운트 계산 (방향 플래그 카운팅용)
    /// </summary>
    private int BitCount(int n)
    {
        int count = 0;
        while (n > 0)
        {
            count += n & 1;
            n >>= 1;
        }

        return count;
    }

    /// <summary>
    /// 외갑판 타일 정보 클래스
    /// </summary>
    private class OuterHullTileInfo
    {
        // 방향 플래그 (비트마스크: 0001=상, 0010=우, 0100=하, 1000=좌)
        public int directionFlags = 0;
    }

    /// <summary>
    /// 지정된 위치에 외갑판을 생성합니다.
    /// </summary>
    /// <param name="hullPosition">외갑판 위치</param>
    /// <param name="spriteIndex">스프라이트 인덱스</param>
    /// <param name="level">외갑판 레벨 (0-2)</param>
    /// <param name="hullData">외갑판 데이터</param>
    /// <param name="hullPrefab">외갑판 프리팹</param>
    private void CreateOuterHull(Vector2Int hullPosition, int spriteIndex, int level, OuterHullData hullData,
        GameObject hullPrefab)
    {
        if (hullPrefab == null)
        {
            Debug.LogError("외갑판 프리팹이 설정되지 않았습니다.");
            return;
        }

        // 프리팹 인스턴스 생성
        GameObject hullObj = GameObject.Instantiate(hullPrefab, parentShip.transform);

        // 위치 설정 (그리드 -> 월드 좌표 변환)
        Vector3 worldPos = parentShip.GridToWorldPosition(hullPosition);
        hullObj.transform.position = worldPos + new Vector3(0, 0, 15f); // 약간 뒤에 배치

        // 레벨에 맞는 스프라이트 가져오기
        Sprite sprite = hullData.GetSpecificHullSprite(level, spriteIndex);

        // OuterHull 컴포넌트 설정
        OuterHull outerHull = hullObj.GetComponent<OuterHull>();
        if (outerHull == null) outerHull = hullObj.AddComponent<OuterHull>();

        // 데이터 참조 설정
        outerHull.SetOuterHullData(hullData);

        // 외갑판 초기화
        outerHull.Initialize(hullPosition, level, spriteIndex, sprite, parentShip);

        // 생성된 외갑판 저장
        currentOuterHulls.Add(outerHull);
    }

    /// <summary>
    /// 기존 외갑판 객체들을 모두 제거합니다.
    /// </summary>
    private void ClearExistingHulls()
    {
        foreach (OuterHull hull in currentOuterHulls)
            if (hull != null)
                GameObject.Destroy(hull.gameObject);

        currentOuterHulls.Clear();
    }
}
