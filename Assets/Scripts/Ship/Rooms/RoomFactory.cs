using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 방 생성 및 관리를 담당하는 클래스
/// </summary>
public class RoomFactory : MonoBehaviour
{
    [SerializeField] private RoomDatabase roomDatabase;
    [SerializeField] private GameObject roomPrefab; // 방 프리팹

    private void Awake()
    {
        InitializeDatabase();
    }

    /// <summary>
    /// 데이터베이스 초기화
    /// </summary>
    private void InitializeDatabase()
    {
        if (roomDatabase != null)
            roomDatabase.InitializeDictionary();
        else
            Debug.LogError("Room Database not assigned!");
    }

    /// <summary>
    /// 특정 방 타입의 RoomData 반환
    /// </summary>
    /// <param name="roomType">방 타입</param>
    /// <returns>RoomData</returns>
    public RoomData GetRoomData(RoomType roomType)
    {
        return roomDatabase.GetRoomData(roomType);
    }

    /// <summary>
    /// 특정 방 타입의 RoomData를 제네릭 타입으로 반환
    /// </summary>
    /// <typeparam name="T">RoomData 상속 타입</typeparam>
    /// <param name="roomType">방 타입</param>
    /// <returns>요청한 타입의 RoomData</returns>
    public T GetTypedRoomData<T>(RoomType roomType) where T : RoomData
    {
        return roomDatabase.GetTypedRoomData<T>(roomType);
    }

    /// <summary>
    /// 새로운 방 인스턴스 생성
    /// </summary>
    /// <param name="roomType">방 타입</param>
    /// <param name="level">방 레벨</param>
    /// <param name="position">그리드 위치</param>
    /// <param name="rotation">회전 값</param>
    /// <returns>생성된 방 인스턴스</returns>
    public Room CreateRoomInstance(RoomType roomType, int level, Vector2Int position,
        RotationConstants.Rotation rotation = RotationConstants.Rotation.Rotation0)
    {
        // 프리팹 체크
        if (roomPrefab == null)
        {
            Debug.LogError("방 프리팹이 할당되지 않았습니다!");
            return null;
        }

        // RoomData 체크
        RoomData roomData = GetRoomData(roomType);
        if (roomData == null)
        {
            Debug.LogError($"방 타입 {roomType}에 대한 데이터가 없습니다!");
            return null;
        }

        // 방 인스턴스 생성
        GameObject roomObject = Instantiate(roomPrefab);
        roomObject.name = $"Room_{roomType}_{level}";

        // 방 타입에 맞는 컴포넌트 추가
        Room room = AddRoomComponent(roomObject, roomType);
        if (room == null)
        {
            Debug.LogError($"방 타입 {roomType}에 대한 컴포넌트를 추가할 수 없습니다!");
            Destroy(roomObject);
            return null;
        }

        // 방 초기화
        room.SetRoomData(roomData);
        room.position = position;
        room.currentRotation = rotation;
        room.Initialize(level);

        // 월드 위치 설정
        Vector2Int roomSize = room.GetSize();
        // 회전이 있는 경우 사이즈 적용
        if ((int)rotation % 2 != 0) // 90도 또는 270도
            roomSize = new Vector2Int(roomSize.y, roomSize.x);

        roomObject.transform.position = ShipGridHelper.GetRoomWorldPosition(position, roomSize);
        roomObject.transform.rotation = Quaternion.Euler(0, 0, -(int)rotation * 90f);

        return room;
    }

    /// <summary>
    /// 기존 방 객체를 복제하여 새 인스턴스 생성
    /// </summary>
    /// <param name="room">복제할 방 객체</param>
    /// <returns>생성된 방 인스턴스</returns>
    public Room CreateRoomObject(Room room)
    {
        if (roomPrefab == null)
        {
            Debug.LogError("방 프리팹이 할당되지 않았습니다!");
            return null;
        }

        // 새 게임 오브젝트 생성
        GameObject roomObject = Instantiate(roomPrefab);

        // 방 타입에 맞는 컴포넌트 추가
        Room roomComponent = AddRoomComponent(roomObject, room.roomType);
        if (roomComponent == null)
        {
            Debug.LogError($"방 타입 {room.roomType}에 대한 컴포넌트를 추가할 수 없습니다!");
            Destroy(roomObject);
            return null;
        }

        // 속성 복사
        roomComponent.CopyFrom(room);

        // 이름 설정
        roomObject.name = $"Room_{room.roomType}_{room.GetCurrentLevel()}";

        return roomComponent;
    }

    /// <summary>
    /// 방 타입에 따른 올바른 Room 컴포넌트 추가
    /// </summary>
    /// <param name="roomObject">방 게임오브젝트</param>
    /// <param name="roomType">방 타입</param>
    /// <returns>추가된 Room 컴포넌트</returns>
    private Room AddRoomComponent(GameObject roomObject, RoomType roomType)
    {
        // 기존 Room 컴포넌트 제거 (있는 경우)
        Room existingRoom = roomObject.GetComponent<Room>();
        if (existingRoom != null)
            Destroy(existingRoom);

        // 방 타입에 따라 다른 컴포넌트 추가
        switch (roomType)
        {
            case RoomType.Cockpit:
                return roomObject.AddComponent<CockpitRoom>();
            case RoomType.Engine:
                return roomObject.AddComponent<EngineRoom>();
            // 다른 방 타입들에 대해 추가
            // case RoomType.Shield:
            //     return roomObject.AddComponent<ShieldRoom>();
            // 등등...
            default:
                Debug.LogWarning($"방 타입 {roomType}에 대한 구체적인 컴포넌트가 정의되지 않았습니다. 기본 Room 컴포넌트를 사용합니다.");
                return roomObject.AddComponent<Room>();
        }
    }


    /// <summary>
    /// 모든 방 데이터 목록을 반환
    /// </summary>
    /// <returns>모든 방 데이터 목록</returns>
    public List<RoomData> GetAllRoomData()
    {
        return roomDatabase.GetAllRoomData();
    }
}
