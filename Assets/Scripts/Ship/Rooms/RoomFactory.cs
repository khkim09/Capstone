using UnityEngine;

/// <summary>
/// 함선 내의 모든 방을 생성하고 관리하는 팩토리 클래스.
/// 방 인스턴스 생성, 복제 등을 담당합니다.
/// </summary>
public class RoomFactory : MonoBehaviour
{
    [Header("데이터베이스 참조")] [SerializeField] private RoomDatabase roomDatabase;

    [Header("방 프리팹 참조")] [SerializeField] private GameObject roomPrefab; // 모든 방 생성의 기본이 되는 단일 프리팹

    private void Awake()
    {
        InitializeRoomDatabase();
    }

    /// <summary>
    /// 방 데이터베이스 초기화
    /// </summary>
    private void InitializeRoomDatabase()
    {
        if (roomDatabase != null)
            roomDatabase.Initialize();
        else
            Debug.LogError("Room Database not assigned!");
    }

    /// <summary>
    /// 방 유형에 따른 데이터 반환
    /// </summary>
    /// <param name="roomType">방 유형</param>
    /// <returns>해당 유형의 방 데이터</returns>
    public RoomData GetRoomData(RoomType roomType)
    {
        if (roomDatabase == null)
        {
            Debug.LogError("Room Database is not assigned!");
            return null;
        }

        return roomDatabase.GetRoomData(roomType);
    }

    /// <summary>
    /// 창고 데이터 반환
    /// </summary>
    /// <param name="storageType">창고 유형</param>
    /// <param name="size">창고 크기</param>
    /// <returns>해당 유형과 크기의 창고 데이터</returns>
    public StorageRoomBaseData GetStorageRoomData(StorageType storageType, StorageSize size)
    {
        if (roomDatabase == null)
        {
            Debug.LogError("Room Database is not assigned!");
            return null;
        }

        return roomDatabase.GetStorageRoomData(storageType, size);
    }

    /// <summary>
    /// 생활시설 데이터 반환
    /// </summary>
    /// <param name="facilityType">시설 유형</param>
    /// <returns>해당 유형의 생활시설 데이터</returns>
    public LifeSupportRoomData GetLifeSupportRoomData(LifeSupportRoomType facilityType)
    {
        if (roomDatabase == null)
        {
            Debug.LogError("Room Database is not assigned!");
            return null;
        }

        return roomDatabase.GetLifeSupportRoomData(facilityType);
    }

    public CrewQuartersRoomData GetCrewQuartersRoomData(CrewQuartersRoomSize size)
    {
        if (roomDatabase == null)
        {
            Debug.LogError("Room Database is not assigned!");
            return null;
        }

        return roomDatabase.GetCrewQuartersRoomData(size);
    }

    /// <summary>
    /// 지정된 방 유형의 인스턴스 생성
    /// </summary>
    /// <param name="roomType">생성할 방 유형</param>
    /// <param name="level">방 레벨</param>
    /// <returns>생성된 방 인스턴스</returns>
    public Room CreateRoomInstance(RoomType roomType, int level = 1)
    {
        RoomData roomData = GetRoomData(roomType);
        if (roomData == null)
        {
            Debug.LogError($"Room data not found for type: {roomType}");
            return null;
        }

        GameObject roomObject = Instantiate(roomPrefab);
        roomObject.name = $"Room_{roomType}";

        Room roomInstance = AttachRoomComponent(roomObject, roomType);
        if (roomInstance == null)
        {
            Debug.LogError($"Failed to create room component for type: {roomType}");
            Destroy(roomObject);
            return null;
        }

        roomInstance.SetRoomData(roomData);
        roomInstance.Initialize(level);

        return roomInstance;
    }

    /// <summary>
    /// 창고 인스턴스 생성
    /// </summary>
    /// <param name="storageType">창고 유형</param>
    /// <param name="size">창고 크기</param>
    /// <param name="level">방 레벨</param>
    /// <returns>생성된 창고 인스턴스</returns>
    public StorageRoomBase CreateStorageRoomInstance(StorageType storageType, StorageSize size, int level = 1)
    {
        StorageRoomBaseData storageData = GetStorageRoomData(storageType, size);
        if (storageData == null)
        {
            Debug.LogError($"Storage room data not found for type: {storageType}, size: {size}");
            return null;
        }

        GameObject roomObject = Instantiate(roomPrefab);
        roomObject.name = $"Storage_{storageType}_{size}";

        StorageRoomBase storageInstance = AttachStorageRoomComponent(roomObject, storageType);
        if (storageInstance == null)
        {
            Debug.LogError($"Failed to create storage room component for type: {storageType}");
            Destroy(roomObject);
            return null;
        }

        storageInstance.SetRoomData(storageData);
        storageInstance.SetCurrentLevel(1);
        storageInstance.Initialize(level);

        return storageInstance;
    }

    /// <summary>
    /// 생활시설 인스턴스 생성
    /// </summary>
    /// <param name="facilityType">시설 유형</param>
    /// <param name="level">방 레벨</param>
    /// <returns>생성된 생활시설 인스턴스</returns>
    public LifeSupportRoom CreateLifeSupportRoomInstance(LifeSupportRoomType facilityType, int level = 1)
    {
        LifeSupportRoomData lifeSupportData = GetLifeSupportRoomData(facilityType);
        if (lifeSupportData == null)
        {
            Debug.LogError($"Life support room data not found for type: {facilityType}");
            return null;
        }

        GameObject roomObject = Instantiate(roomPrefab);
        roomObject.name = $"LifeSupport_{facilityType}";

        LifeSupportRoom lifeSupportInstance = roomObject.AddComponent<LifeSupportRoom>();
        lifeSupportInstance.SetRoomData(lifeSupportData);
        lifeSupportInstance.Initialize(level);

        return lifeSupportInstance;
    }

    public CrewQuartersRoom CreateCrewQuartersRoomInstance(CrewQuartersRoomSize size, int level = 1)
    {
        CrewQuartersRoomData crewQuartersData = GetCrewQuartersRoomData(size);
        if (crewQuartersData == null)
        {
            Debug.LogError($"Crew quarters room data not found for size: {size}");
            return null;
        }

        GameObject roomObject = Instantiate(roomPrefab);
        roomObject.name = $"CrewQuarters_{size}";

        CrewQuartersRoom crewQuartersInstance = roomObject.AddComponent<CrewQuartersRoom>();
        crewQuartersInstance.SetRoomData(crewQuartersData);
        crewQuartersInstance.Initialize(level);

        return crewQuartersInstance;
    }

    /// <summary>
    /// 기존 방과 동일한 방 인스턴스 생성
    /// </summary>
    /// <param name="sourceRoom">복제할 방</param>
    /// <returns>복제된 방 인스턴스</returns>
    public Room CreateRoomObject(Room sourceRoom)
    {
        if (sourceRoom == null)
        {
            Debug.LogError("Source room is null!");
            return null;
        }

        GameObject roomObject = Instantiate(roomPrefab);
        roomObject.name = $"Room_{sourceRoom.name}";

        Room roomInstance;

        // 타입에 따라 적절한 컴포넌트 부착
        if (sourceRoom is StorageRoomBase storageRoom)
        {
            // 스토리지룸인 경우
            roomInstance = AttachStorageRoomComponent(roomObject, storageRoom.GetStorageType());

            // 스토리지 관련 초기화 호출
            roomInstance.SetRoomData(sourceRoom.GetRoomData());
            roomInstance.Initialize(sourceRoom.GetCurrentLevel());

            // 이후 스토리지 특화 데이터 복사
            roomInstance.CopyFrom(sourceRoom);
        }
        else
        {
            // 일반 룸인 경우
            roomInstance = AttachRoomComponent(roomObject, sourceRoom.GetRoomType());

            // 기본 초기화 호출
            roomInstance.SetRoomData(sourceRoom.GetRoomData());
            roomInstance.Initialize(sourceRoom.GetCurrentLevel());

            // 이후 데이터 복사
            roomInstance.CopyFrom(sourceRoom);
        }

        if (roomInstance == null)
        {
            Debug.LogError($"Failed to create room component for room: {sourceRoom.name}");
            Destroy(roomObject);
            return null;
        }

        return roomInstance;
    }

    /// <summary>
    /// 방 유형에 따라 적절한 컴포넌트 부착
    /// </summary>
    private Room AttachRoomComponent(GameObject roomObject, RoomType roomType)
    {
        Room roomComponent = null;

        switch (roomType)
        {
            case RoomType.Cockpit:
                roomComponent = roomObject.AddComponent<CockpitRoom>();
                break;
            case RoomType.Engine:
                // 엔진룸 구현 필요
                roomComponent = roomObject.AddComponent<EngineRoom>();
                break;
            case RoomType.WeaponControl:
                // 무기 제어실 구현 필요
                roomComponent = roomObject.AddComponent<WeaponControlRoom>();
                break;
            case RoomType.Shield:
                // 실드 제어실 구현 필요
                roomComponent = roomObject.AddComponent<ShieldRoom>();
                break;
            case RoomType.Power:
                // 전력실 구현 필요
                roomComponent = roomObject.AddComponent<PowerRoom>();
                break;
            case RoomType.Oxygen:
                // 산소실 구현 필요
                roomComponent = roomObject.AddComponent<OxygenRoom>();
                break;
            case RoomType.MedBay:
                // 의무실 구현 필요
                roomComponent = roomObject.AddComponent<MedBayRoom>();
                break;
            case RoomType.Teleporter:
                // 텔레포터 구현 필요
                roomComponent = roomObject.AddComponent<TeleportRoom>();
                break;
            case RoomType.Ammunition:
                // 탄약고 구현 필요
                roomComponent = roomObject.AddComponent<AmmunitionRoom>();
                break;
            case RoomType.CrewQuarters:
                // 선원실 구현 필요
                roomComponent = roomObject.AddComponent<CrewQuartersRoom>();
                break;
            case RoomType.Corridor:
                // 복도 구현 필요
                roomComponent = roomObject.AddComponent<CorridorRoom>();
                break;
            case RoomType.LifeSupport:
                roomComponent = roomObject.AddComponent<LifeSupportRoom>();
                break;
            default:
                Debug.LogError($"Unknown room type: {roomType}");
                break;
        }

        return roomComponent;
    }

    /// <summary>
    /// 창고 유형에 따라 적절한 컴포넌트 부착
    /// </summary>
    private StorageRoomBase AttachStorageRoomComponent(GameObject roomObject, StorageType storageType)
    {
        StorageRoomBase storageComponent = null;

        switch (storageType)
        {
            case StorageType.Regular:
                storageComponent = roomObject.AddComponent<StorageRoomRegular>();
                break;
            case StorageType.Temperature:
                storageComponent = roomObject.AddComponent<StorageRoomTemperature>();
                break;
            case StorageType.Animal:
                storageComponent = roomObject.AddComponent<StorageRoomAnimal>();
                break;
            default:
                Debug.LogError($"Unknown storage type: {storageType}");
                break;
        }

        return storageComponent;
    }
}
