using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Easy Save 3를 사용하여 방(Room) 객체의 직렬화 및 역직렬화를 담당하는 유틸리티 클래스
/// </summary>
public static class RoomSerialization
{
    /// <summary>
    /// 모든 방을 저장합니다.
    /// </summary>
    /// <param name="rooms">저장할 방 목록</param>
    /// <param name="filename">저장 파일명</param>
    public static void SaveAllRooms(List<Room> rooms, string filename)
    {
        // 파일이 이미 존재한다면 해당 파일 삭제 (덮어쓰기)
        if (ES3.FileExists(filename))
            ES3.DeleteFile(filename);

        // 모든 방 저장
        ES3.Save("roomCount", rooms.Count, filename);

        for (int i = 0; i < rooms.Count; i++)
            ES3.Save($"room_{i}", rooms[i], filename);
    }

    /// <summary>
    /// 함선에 설치된 모든 방을 저장합니다.
    /// </summary>
    /// <param name="ship">방이 설치된 함선</param>
    /// <param name="filename">저장 파일명</param>
    public static void SaveShipRooms(Ship ship, string filename)
    {
        if (ship == null)
            return;

        SaveAllRooms(ship.GetAllRooms(), filename);
    }

    /// <summary>
    /// 저장된 모든 방을 불러옵니다.
    /// </summary>
    /// <param name="filename">불러올 파일명</param>
    /// <returns>불러온 방 목록</returns>
    public static List<Room> LoadAllRoomsData(string filename)
    {
        List<Room> roomsData = new();

        if (!ES3.FileExists(filename))
            return roomsData;

        // 방 수 불러오기
        int roomCount = ES3.Load<int>("roomCount", filename);

        // 각 방 불러오기
        for (int i = 0; i < roomCount; i++)
            if (ES3.KeyExists($"room_{i}", filename))
            {
                Room room = ES3.Load<Room>($"room_{i}", filename);
                roomsData.Add(room);
            }

        return roomsData;
    }

    /// <summary>
    /// 함선에 모든 방을 복원합니다.
    /// </summary>
    /// <param name="filename">불러올 파일명</param>
    /// <param name="ship">대상 함선</param>
    /// <returns>복원된 방 수</returns>
    public static int RestoreAllRoomsToShip(string filename, Ship ship)
    {
        if (!ES3.FileExists(filename) || ship == null)
            return 0;

        // 기존 방 제거

        ship.RemoveAllRooms();

        // 백업 데이터로부터 방 정보 불러오기
        List<Room> roomsData = LoadAllRoomsData(filename);
        int restoredCount = 0;

        // RoomFactory를 통해 각 방 생성 및 추가
        foreach (Room roomData in roomsData)
        {
            ship.AddRoom(roomData);
            restoredCount++;
        }

        return restoredCount;
    }

    /// <summary>
    /// 방 데이터로부터 실제 Room 객체를 생성합니다.
    /// </summary>
    /// <param name="roomData">방 백업 데이터</param>
    /// <param name="ship">방이 추가될 함선</param>
    /// <returns>생성된 Room 객체</returns>
    private static Room CreateRoom(RoomBackupData roomData, Ship ship)
    {
        if (roomData.roomData == null)
            return null;

        // 방 타입에 맞는 GameObject 생성
        GameObject roomObject = new($"{roomData.roomData.GetRoomType()}");

        // 방 타입에 따라 적절한 Room 컴포넌트 추가
        Room newRoom = null;

        switch (roomData.roomData.GetRoomType())
        {
            case RoomType.Power:
                newRoom = roomObject.AddComponent<PowerRoom>();
                break;
            case RoomType.Engine:
                newRoom = roomObject.AddComponent<EngineRoom>();
                break;
            case RoomType.Oxygen:
                newRoom = roomObject.AddComponent<OxygenRoom>();
                break;
            case RoomType.Shield:
                newRoom = roomObject.AddComponent<ShieldRoom>();
                break;
            case RoomType.MedBay:
                newRoom = roomObject.AddComponent<MedBayRoom>();
                break;
            case RoomType.CrewQuarters:
                newRoom = roomObject.AddComponent<CrewQuartersRoom>();
                break;
            case RoomType.Storage:
                // 창고의 경우 보다 구체적인 타입 (일반, 온도조절, 동물) 확인이 필요
                // roomData.roomData의 타입을 확인하여 적절한 창고 타입 생성
                if (roomData.roomData is StorageRoomBaseData storageRoomData)
                {
                    if (storageRoomData.GetStorageType() == StorageType.Animal)
                        newRoom = roomObject.AddComponent<StorageRoomAnimal>();
                    else if (storageRoomData.GetStorageType() == StorageType.Temperature)
                        newRoom = roomObject.AddComponent<StorageRoomTemperature>();
                    else
                        newRoom = roomObject.AddComponent<StorageRoomRegular>();
                }

                break;

            // 다른 방 타입들도 필요에 따라 추가
            default:
                Debug.LogError($"Unknown room type: {roomData.roomData.GetRoomType()}");
                GameObject.Destroy(roomObject);
                return null;
        }

        // 방 초기화
        if (newRoom != null)
        {
            // 방 데이터 및 위치/회전 설정
            newRoom.SetRoomData(roomData.roomData);
            newRoom.position = roomData.position;
            newRoom.currentRotation = roomData.rotation;

            // 레벨 설정 및 초기화
            newRoom.Initialize(roomData.level);

            // 함선의 자식으로 설정
            roomObject.transform.SetParent(ship.transform);

            // 회전 적용
            newRoom.RotateRoom((int)roomData.rotation);
        }

        return newRoom;
    }

    /// <summary>
    /// 특정 타입의 방만 저장합니다.
    /// </summary>
    /// <param name="ship">방이 설치된 함선</param>
    /// <param name="roomType">저장할 방 타입</param>
    /// <param name="filename">저장 파일명</param>
    public static void SaveRoomsByType(Ship ship, RoomType roomType, string filename)
    {
        if (ship == null)
            return;

        List<Room> specificRooms = ship.GetAllRooms().FindAll(room => room.GetRoomType() == roomType);
        SaveAllRooms(specificRooms, filename);
    }

    /// <summary>
    /// 특정 방을 직렬화하고 백업 데이터로 반환합니다.
    /// </summary>
    /// <param name="room">직렬화할 방</param>
    /// <returns>방 백업 데이터</returns>
    public static RoomBackupData SerializeRoomToData(Room room)
    {
        if (room == null)
            return new RoomBackupData();

        return new RoomBackupData
        {
            roomData = room.GetRoomData(),
            level = room.GetCurrentLevel(),
            position = room.position,
            rotation = room.currentRotation
        };
    }
}
