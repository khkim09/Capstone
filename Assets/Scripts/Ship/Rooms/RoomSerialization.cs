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
        ES3Settings settings = new() { referenceMode = ES3.ReferenceMode.ByRef };

        // 모든 방 저장
        ES3.Save("roomCount", rooms.Count, filename);

        for (int i = 0; i < rooms.Count; i++)
            ES3.Save($"room_{i}", rooms[i], filename, settings);
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
        ES3Settings settings = new() { referenceMode = ES3.ReferenceMode.ByRef };

        // 방 수 불러오기
        int roomCount = ES3.Load<int>("roomCount", filename);

        // 각 방 불러오기
        for (int i = 0; i < roomCount; i++)
            if (ES3.KeyExists($"room_{i}", filename))
            {
                Room room = ES3.Load<Room>($"room_{i}", filename, settings);
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
            Room newRoom = GameObjectFactory.Instance.CreateRoomObject(roomData);
            ship.AddRoom(newRoom);
            restoredCount++;
        }

        return restoredCount;
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
}
