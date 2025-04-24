using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 방의 피해 단계 수준을 나타내는 열거형입니다.
/// </summary>
[Serializable]
public enum RoomDamageLevel
{
    /// <summary>1단계 피해 (경미한 손상 등).</summary>
    DamageLevelOne,

    /// <summary>2단계 피해 (심각한 손상 등).</summary>
    DamageLevelTwo
}

/// <summary>
/// 방 기능에 따라 나누는 카테고리 분류
/// </summary>
public enum RoomCategory
{
    Essential, // 필수 시설
    Auxiliary, // 보조 시설
    Living, // 생활 시설
    Storage, // 저장고
    Etc // 기타 (복도)
}

/// <summary>
/// 백업해 둘 방 정보 구조체
/// </summary>
[Serializable]
public struct RoomBackupData
{
    public RoomData roomData;
    public int level;
    public Vector2Int position;
    public RotationConstants.Rotation rotation;
}

/// <summary>
/// 모든 방 유형의 기본 데이터를 정의하는 추상 ScriptableObject.
/// 방의 레벨별 속성, 체력, 크기, 요구 자원 등을 설정합니다.
/// </summary>
public abstract class RoomData : ScriptableObject
{
    [Serializable]
    public class RoomLevel
    {
        /// <summary>레벨 이름 또는 표시용 이름</summary>
        public string roomName;

        /// <summary>해당 레벨 번호</summary>
        public int level;

        /// <summary>방의 타입</summary>
        public RoomType roomType;

        /// <summary>방 카테고리</summary>
        public RoomCategory category;

        /// <summary>최대 체력</summary>
        public int hitPoint;

        /// <summary>방 크기 (격자 단위, 가로x세로)</summary>
        public Vector2Int size;

        /// <summary>업그레이드 비용</summary>
        public int cost;

        /// <summary>작동에 필요한 최소 선원 수</summary>
        public int crewRequirement;

        /// <summary>요구 전력량</summary>
        public float powerRequirement;

        /// <summary>피해 단계별 체력 비율</summary>
        public RoomDamageRates damageHitPointRate;

        /// <summary>해당 레벨의 방 스프라이트</summary>
        public Sprite roomSprite;
        // TODO: 스프라이트 완성되면 각 Scriptable Object 에 스프라이트 추가할 것, roomPrefab(실제 배치될 방 prefab), previewPrefab(roomPrefab에서 alpha값만 0.5)

        /// <summary>이 방에 가능한 모든 문의 위치와 방향 목록</summary>
        public List<DoorPosition> possibleDoorPositions = new List<DoorPosition>();

        /// <summary>
        /// 방의 선원이 입장 시 우선적으로 배치되어야 할 타일의 좌표 목록입니다.
        /// </summary>
        public List<Vector2Int> crewEntryGridPriority = new List<Vector2Int>();
    }

    /// <summary>
    /// 이 RoomData에 포함된 모든 레벨을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public abstract List<RoomLevel> GetAllLevels();

    /// <summary>
    /// 주어진 레벨에 해당하는 RoomLevel 데이터를 반환합니다.
    /// 각 파생 클래스에서 구현해야 합니다.
    /// </summary>
    /// <param name="level">조회할 레벨 번호.</param>
    /// <returns>RoomLevel 데이터.</returns>
    public abstract RoomLevel GetRoomDataByLevel(int level);

    /// <summary>
    /// 기본 레벨 데이터를 초기화합니다.
    /// 파생 클래스에서 구현합니다.
    /// </summary>
    protected abstract void InitializeDefaultLevels();

    /// <summary>
    /// ScriptableObject가 활성화될 때 호출되며, 필요한 경우 기본 데이터를 초기화합니다.
    /// </summary>
    protected virtual void OnEnable()
    {
        bool needsInitialization = CheckIfNeedsInitialization();
        if (needsInitialization) InitializeDefaultLevels();
    }

    /// <summary>
    /// 데이터 초기화가 필요한지 판단하는 메서드입니다.
    /// 파생 클래스에서 구현합니다.
    /// </summary>
    /// <returns>초기화 필요 여부.</returns>
    protected abstract bool CheckIfNeedsInitialization();

    public RoomType GetRoomType()
    {
        return GetRoomDataByLevel(1).roomType;
    }

    /// <summary>
    /// 회전 적용된 문 위치 리스트 반환 함수
    /// </summary>
    /// <param name="levelIndex"></param>
    /// <param name="basePos"></param>
    /// <param name="rot"></param>
    /// <returns></returns>
    public List<DoorPosition> GetDoorPositionsWithDirection(int levelIndex, Vector2Int originalPos, RotationConstants.Rotation rotation)
    {
        List<DoorPosition> worldDoorPositions = new List<DoorPosition>();

        RoomLevel level = GetRoomDataByLevel(levelIndex);
        if (level == null || level.possibleDoorPositions == null)
            return worldDoorPositions;

        foreach (DoorPosition localDoor in level.possibleDoorPositions)
        {
            // 1. 회전 적용 offset 계산
            Vector2Int rotatedOffset = RoomRotationUtility.RotateDoorPos(localDoor.position, rotation);

            // 2. 그리드 상 문 위치 게산
            Vector2Int worldPos = originalPos + rotatedOffset;

            // 3. 문 방향 회전 적용
            DoorDirection rotatedDir = LocalDirToWorldDir(localDoor.direction, rotation);

            worldDoorPositions.Add(new DoorPosition(worldPos, rotatedDir));
        }

        return worldDoorPositions;
    }

    private DoorDirection LocalDirToWorldDir(DoorDirection localDir, RotationConstants.Rotation rotation)
    {
        DoorDirection worldDir;
        switch (rotation)
        {
            case RotationConstants.Rotation.Rotation0:
                worldDir = localDir;
                break;
            case RotationConstants.Rotation.Rotation90:
                if (localDir == DoorDirection.North)
                    worldDir = DoorDirection.East;
                else if (localDir == DoorDirection.East)
                    worldDir = DoorDirection.South;
                else if (localDir == DoorDirection.South)
                    worldDir = DoorDirection.West;
                else
                    worldDir = DoorDirection.North;
                break;
            case RotationConstants.Rotation.Rotation180:
                if (localDir == DoorDirection.North)
                    worldDir = DoorDirection.South;
                else if (localDir == DoorDirection.East)
                    worldDir = DoorDirection.West;
                else if (localDir == DoorDirection.South)
                    worldDir = DoorDirection.North;
                else
                    worldDir = DoorDirection.East;
                break;
            case RotationConstants.Rotation.Rotation270:
                if (localDir == DoorDirection.North)
                    worldDir = DoorDirection.West;
                else if (localDir == DoorDirection.East)
                    worldDir = DoorDirection.North;
                else if (localDir == DoorDirection.South)
                    worldDir = DoorDirection.East;
                else
                    worldDir = DoorDirection.South;
                break;
            default:
                worldDir = localDir;
                break;
        }
        return worldDir;
    }
}

/// <summary>
/// RoomData의 제네릭 확장 버전.
/// 각 방 타입에 특화된 RoomLevel 데이터를 제네릭으로 정의할 수 있습니다.
/// </summary>
/// <typeparam name="T">RoomLevel을 상속한 특화된 타입.</typeparam>
public abstract class RoomData<T> : RoomData where T : RoomData.RoomLevel
{
    /// <summary>레벨별 방 데이터 리스트.</summary>
    [SerializeField] public List<T> RoomLevels = new();

    /// <summary>
    /// 모든 레벨 반환
    /// </summary>
    /// <returns></returns>
    public override List<RoomLevel> GetAllLevels()
    {
        return RoomLevels.Cast<RoomLevel>().ToList();
    }

    /// <summary>
    /// 주어진 레벨에 해당하는 RoomLevel 데이터를 반환합니다.
    /// </summary>
    /// <param name="level">조회할 레벨 번호.</param>
    /// <returns>RoomLevel 데이터.</returns>
    public override RoomLevel GetRoomDataByLevel(int level)
    {
        return GetTypedRoomData(level);
    }

    /// <summary>
    /// 제네릭 타입으로 명확하게 캐스팅된 RoomLevel 데이터를 반환합니다.
    /// </summary>
    /// <param name="level">조회할 레벨 번호.</param>
    /// <returns>T 타입의 RoomLevel 데이터.</returns>
    public T GetTypedRoomData(int level)
    {
        if (level <= 0 || level > RoomLevels.Count)
            return null;
        return RoomLevels[level - 1];
    }

    /// <summary>
    /// RoomLevels 리스트가 비어 있거나 null인지 확인하여 초기화가 필요한지 판단합니다.
    /// </summary>
    /// <returns>초기화 필요 여부.</returns>
    protected override bool CheckIfNeedsInitialization()
    {
        // RoomLevels가 비어있거나 null이면 초기화 필요
        return RoomLevels == null || RoomLevels.Count == 0;
    }
}

