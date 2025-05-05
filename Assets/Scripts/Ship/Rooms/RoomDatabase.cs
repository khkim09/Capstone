using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 방 데이터베이스 클래스
/// 모든 방 타입, 창고 타입, 생활시설 타입에 대한 데이터를 관리합니다.
/// </summary>
[CreateAssetMenu(fileName = "RoomDatabase", menuName = "Room/Room Database")]
public class RoomDatabase : ScriptableObject
{
    [Header("일반 방 데이터")] public List<RoomDataEntry> roomEntries = new();

    [Header("창고 데이터")] public List<StorageRoomDataEntry> storageRoomEntries = new();

    [Header("생활시설 데이터")] public List<LifeSupportRoomDataEntry> lifeSupportRoomEntries = new();

    [Header("선원 생활관 데이터")] public List<CrewQuartersDataEntry> crewQuartersDataEntries = new();

    // 효율적인 조회를 위한 딕셔너리들
    private Dictionary<RoomType, RoomData> roomDataMap;
    private Dictionary<StorageType, Dictionary<StorageSize, StorageRoomBaseData>> storageDataMap;
    private Dictionary<LifeSupportRoomType, LifeSupportRoomData> lifeSupportDataMap;
    private Dictionary<CrewQuartersRoomSize, CrewQuartersRoomData> crewQuartersDataMap;

    [Serializable]
    public class RoomDataEntry
    {
        public RoomType roomType;
        public RoomData roomData;
    }

    [Serializable]
    public class StorageRoomDataEntry
    {
        public StorageType storageType;
        public StorageSize size;
        public StorageRoomBaseData roomData;
    }

    [Serializable]
    public class LifeSupportRoomDataEntry
    {
        public LifeSupportRoomType facilityType;
        public LifeSupportRoomData roomData;
    }

    [Serializable]
    public class CrewQuartersDataEntry
    {
        public CrewQuartersRoomSize size;
        public CrewQuartersRoomData roomData;
    }

    /// <summary>
    /// 모든 딕셔너리 초기화
    /// </summary>
    public void Initialize()
    {
        InitializeRoomDataMap();
        InitializeStorageDataMap();
        InitializeLifeSupportDataMap();
        InitializeCrewQuartersDataMap();
    }

    /// <summary>
    /// 일반 방 데이터 맵 초기화
    /// </summary>
    private void InitializeRoomDataMap()
    {
        roomDataMap = new Dictionary<RoomType, RoomData>();

        foreach (RoomDataEntry entry in roomEntries)
            if (entry.roomData != null)
                roomDataMap[entry.roomType] = entry.roomData;
            else
                Debug.LogWarning($"Missing room data for type: {entry.roomType}");
    }

    /// <summary>
    /// 창고 데이터 맵 초기화
    /// </summary>
    private void InitializeStorageDataMap()
    {
        storageDataMap = new Dictionary<StorageType, Dictionary<StorageSize, StorageRoomBaseData>>();

        foreach (StorageRoomDataEntry entry in storageRoomEntries)
            if (entry.roomData != null)
            {
                if (!storageDataMap.ContainsKey(entry.storageType))
                    storageDataMap[entry.storageType] = new Dictionary<StorageSize, StorageRoomBaseData>();

                storageDataMap[entry.storageType][entry.size] = entry.roomData;
            }
            else
            {
                Debug.LogWarning($"Missing storage room data for type: {entry.storageType}, size: {entry.size}");
            }
    }

    /// <summary>
    /// 생활시설 데이터 맵 초기화
    /// </summary>
    private void InitializeLifeSupportDataMap()
    {
        lifeSupportDataMap = new Dictionary<LifeSupportRoomType, LifeSupportRoomData>();

        foreach (LifeSupportRoomDataEntry entry in lifeSupportRoomEntries)
            if (entry.roomData != null)
                lifeSupportDataMap[entry.facilityType] = entry.roomData;
            else
                Debug.LogWarning($"Missing life support room data for type: {entry.facilityType}");
    }

    private void InitializeCrewQuartersDataMap()
    {
        crewQuartersDataMap = new Dictionary<CrewQuartersRoomSize, CrewQuartersRoomData>();

        foreach (CrewQuartersDataEntry entry in crewQuartersDataEntries)
            if (entry.roomData != null)
                crewQuartersDataMap[entry.size] = entry.roomData;
            else
                Debug.LogWarning($"Missing crew quarters room data for size: {entry.size}");
    }

    /// <summary>
    /// 방 유형에 해당하는 데이터 반환
    /// </summary>
    public RoomData GetRoomData(RoomType roomType)
    {
        if (roomDataMap == null) InitializeRoomDataMap();

        if (roomDataMap.TryGetValue(roomType, out RoomData data)) return data;

        Debug.LogWarning($"Room data not found for type: {roomType}");
        return null;
    }

    /// <summary>
    /// 창고 데이터 반환
    /// </summary>
    public StorageRoomBaseData GetStorageRoomData(StorageType storageType, StorageSize size)
    {
        if (storageDataMap == null) InitializeStorageDataMap();

        if (storageDataMap.TryGetValue(storageType, out Dictionary<StorageSize, StorageRoomBaseData> sizeMap) &&
            sizeMap.TryGetValue(size, out StorageRoomBaseData data))
            return data;

        Debug.LogWarning($"Storage room data not found for type: {storageType}, size: {size}");
        return null;
    }

    /// <summary>
    /// 생활시설 데이터 반환
    /// </summary>
    public LifeSupportRoomData GetLifeSupportRoomData(LifeSupportRoomType facilityType)
    {
        if (lifeSupportDataMap == null) InitializeLifeSupportDataMap();

        if (lifeSupportDataMap.TryGetValue(facilityType, out LifeSupportRoomData data)) return data;

        Debug.LogWarning($"Life support room data not found for type: {facilityType}");
        return null;
    }

    public CrewQuartersRoomData GetCrewQuartersRoomData(CrewQuartersRoomSize size)
    {
        if (crewQuartersDataMap == null) InitializeCrewQuartersDataMap();

        if (crewQuartersDataMap.TryGetValue(size, out CrewQuartersRoomData data)) return data;

        Debug.LogWarning($"Crew quarters room data not found for size: {size}");
        return null;
    }

    /// <summary>
    /// 모든 방 데이터 반환
    /// </summary>
    public List<RoomData> GetAllRoomData()
    {
        List<RoomData> result = new();

        foreach (RoomDataEntry entry in roomEntries)
            if (entry.roomData != null)
                result.Add(entry.roomData);

        return result;
    }
}
