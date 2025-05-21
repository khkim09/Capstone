using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 방 데이터 직렬화를 위한 클래스
/// </summary>
[Serializable]
public class BlueprintRoomSaveData
{
    public RoomData bpRoomData;
    public int bpLevelIndex;
    public Vector2Int bpPosition;
    public Constants.Rotations.Rotation bpRotation;
}

/// <summary>
/// 무기 데이터 직렬화를 위한 클래스
/// </summary>
[Serializable]
public class BlueprintWeaponSaveData
{
    public ShipWeaponData bpWeaponData;
    public Vector2Int bpPosition;
    public ShipWeaponAttachedDirection bpDirection;
    // public int hullLevel; // 외갑판 레벨 저장 (0: 레벨 1, 1: 레벨 2, 2: 레벨 3)
}

/// <summary>
/// 플레이어의 함선 설계도 배치 상태를 게임 중에만 저장하는 클래스.
/// </summary>
public static class BlueprintLayoutSaver
{
    // 게임 실행 중에만 메모리에 저장되는 정적 변수
    private static List<BlueprintRoomSaveData> savedRooms = new();
    private static List<BlueprintWeaponSaveData> savedWeapons = new();
    private static int savedHullLevel = -1;

    public static void SaveLayout(BlueprintRoom[] rooms, BlueprintWeapon[] weapons, int hullLevel)
    {
        SaveRoomLayout(rooms);
        SaveWeaponLayout(weapons);
        savedHullLevel = hullLevel;
    }

    /// <summary>
    /// 현재 방 배치 상태를 저장합니다.
    /// </summary>
    /// <param name="bpRooms">저장할 방 배열</param>
    public static void SaveRoomLayout(BlueprintRoom[] bpRooms)
    {
        // 기존 저장 데이터 초기화
        savedRooms.Clear();

        foreach (BlueprintRoom room in bpRooms)
        {
            // null 체크
            if (room == null || room.bpRoomData == null)
                continue;

            BlueprintRoomSaveData saveData = new()
            {
                bpRoomData = room.bpRoomData,
                bpLevelIndex = room.bpLevelIndex,
                bpPosition = room.bpPosition,
                bpRotation = room.bpRotation
            };

            savedRooms.Add(saveData);
        }

        Debug.Log($"방 배치 상태 저장 완료: {savedRooms.Count}개의 방");
    }

    /// <summary>
    /// 현재 무기 배치 상태를 저장합니다.
    /// </summary>
    /// <param name="bpWeapons">저장할 무기 배열</param>
    public static void SaveWeaponLayout(BlueprintWeapon[] bpWeapons)
    {
        // 기존 저장 데이터 초기화
        savedWeapons.Clear();

        foreach (BlueprintWeapon weapon in bpWeapons)
        {
            // null 체크
            if (weapon == null || weapon.bpWeaponData == null)
                continue;

            BlueprintWeaponSaveData saveData = new()
            {
                bpWeaponData = weapon.bpWeaponData,
                bpPosition = weapon.bpPosition,
                bpDirection = weapon.bpAttachedDirection,
                // hullLevel = weapon.GetHullLevel() // 외갑판 레벨 저장
            };

            savedWeapons.Add(saveData);
        }

        Debug.Log($"무기 배치 상태 저장 완료: {savedWeapons.Count}개의 무기");
    }

    /// <summary>
    /// 저장된 방 배치 데이터를 로드합니다.
    /// </summary>
    /// <returns>저장된 방 배치 리스트</returns>
    public static List<BlueprintRoomSaveData> LoadRoomLayout()
    {
        return new List<BlueprintRoomSaveData>(savedRooms);
    }

    /// <summary>
    /// 저장된 무기 배치 데이터를 로드합니다.
    /// </summary>
    /// <returns>저장된 무기 배치 리스트</returns>
    public static List<BlueprintWeaponSaveData> LoadWeaponLayout()
    {
        return new List<BlueprintWeaponSaveData>(savedWeapons);
    }

    public static int LoadHullLevel()
    {
        return savedHullLevel;
    }

    /// <summary>
    /// 모든 저장 데이터를 초기화합니다.
    /// </summary>
    public static void ClearAllData()
    {
        savedRooms.Clear();
        savedWeapons.Clear();
        savedHullLevel = -1;
        Debug.Log("모든 설계도 배치 데이터 초기화 완료");
    }
}
