using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Easy Save 3를 사용하여 함선(Ship) 객체의 직렬화 및 역직렬화를 담당하는 유틸리티 클래스
/// ShipFactory를 활용하여 객체 생성 및 복원을 처리합니다.
/// </summary>
public static class ShipSerialization
{
    /// <summary>
    /// 함선 전체를 저장합니다.
    /// </summary>
    /// <param name="ship">저장할 함선</param>
    /// <param name="filename">저장 파일명</param>
    public static void SaveShip(Ship ship, string filename)
    {
        if (ship == null)
            return;

        // 파일이 이미 존재한다면 해당 파일 삭제 (덮어쓰기)
        if (ES3.FileExists(filename))
            ES3.DeleteFile(filename);

        ES3Settings settings = new() { referenceMode = ES3.ReferenceMode.ByRefAndValue };
        ES3.Save("ship", ship.gameObject, filename, settings);

        if (Debug.isDebugBuild)
            Debug.Log($"Ship saved to {filename}");
    }

    /// <summary>
    /// 저장된 함선을 불러와 새 인스턴스를 생성합니다.
    /// </summary>
    /// <param name="filename">불러올 파일명</param>
    /// <returns>불러온 함선 객체, 실패 시 null 반환</returns>
    public static Ship LoadShip(string filename)
    {
        if (!ES3.FileExists(filename))
        {
            Debug.LogWarning($"Ship save file not found: {filename}");
            return null;
        }

        ES3Settings settings = new() { referenceMode = ES3.ReferenceMode.ByRefAndValue };

        GameObject shipObj = ES3.Load<GameObject>("ship", filename, settings);
        Ship loadedShip = shipObj.GetComponent<Ship>();

        // HACK : 제대로 참조가 연결되지 않아서 임의로 복구하는 방식으로 구현. 나중에 문제가 생기거나 비슷한 참조 관계의 객체를 직렬화할 때 문제가 생길 것임
        // foreach (TradingItem item in loadedShip.GetAllItems())
        //     item.Initialize(item.GetItemData(), item.amount, item.GetItemState());

        return loadedShip;
    }

    /// <summary>
    /// 저장된 함선 데이터를 기존 함선 객체에 복원합니다.
    /// </summary>
    /// <param name="filename">불러올 파일명</param>
    /// <param name="targetShip">대상 함선 객체</param>
    /// <returns>복원 성공 여부</returns>
    public static bool RestoreShip(string filename, Ship targetShip)
    {
        if (!ES3.FileExists(filename) || targetShip == null)
            return false;

        try
        {
            // 저장된 함선 데이터 불러오기
            Ship savedShipData = ES3.Load<Ship>("ship", filename);
            if (savedShipData == null)
                return false;

            // 1. 기존 방 제거
            targetShip.RemoveAllRooms();

            // 2. 기존 승무원 제거
            targetShip.RemoveAllCrews();

            // 3. 기존 무기 제거
            List<ShipWeapon> existingWeapons = new(targetShip.GetAllWeapons());
            foreach (ShipWeapon weapon in existingWeapons)
                targetShip.RemoveWeapon(weapon);

            foreach (Room room in targetShip.GetAllRooms())
                if (room is StorageRoomBase storageRoom)
                    storageRoom.RemoveAllItems();


            // 4. 함선 이름 복원
            targetShip.shipName = savedShipData.shipName;

            // 5. 룸 복원
            foreach (Room room in savedShipData.GetAllRooms())
            {
                Room newRoom = GameObjectFactory.Instance.CreateRoomObject(room);
                targetShip.AddRoom(newRoom);
            }


            // 6. 승무원 복원
            foreach (CrewBase crew in savedShipData.GetAllCrew()) targetShip.AddCrew(crew);

            // 7. 무기 복원
            foreach (ShipWeapon weapon in savedShipData.GetAllWeapons()) targetShip.AddWeapon(weapon);

            if (Debug.isDebugBuild)
                Debug.Log($"Ship restored from {filename}");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error restoring ship: {e.Message} - {e.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// 빠른 저장 기능 - 기본 경로에 함선을 저장합니다.
    /// </summary>
    /// <param name="ship">저장할 함선</param>
    /// <param name="slotIndex">저장 슬롯 번호 (선택 사항, 기본값은 0)</param>
    /// <returns>저장 성공 여부</returns>
    public static bool QuickSave(Ship ship, int slotIndex = 0)
    {
        if (ship == null)
            return false;

        string savePath = $"{Application.persistentDataPath}/ship_save_slot_{slotIndex}.es3";
        SaveShip(ship, savePath);
        return true;
    }

    /// <summary>
    /// 빠른 불러오기 기능 - 기본 경로에서 함선을 새로 생성하여 반환합니다.
    /// </summary>
    /// <param name="slotIndex">저장 슬롯 번호 (선택 사항, 기본값은 0)</param>
    /// <returns>생성된 함선 객체, 실패 시 null</returns>
    public static Ship QuickLoad(int slotIndex = 0)
    {
        string savePath = $"{Application.persistentDataPath}/ship_save_slot_{slotIndex}.es3";
        return LoadShip(savePath);
    }

    /// <summary>
    /// 빠른 불러오기 기능 - 기본 경로에서 함선을 불러와 기존 함선에 복원합니다.
    /// </summary>
    /// <param name="targetShip">복원할 대상 함선</param>
    /// <param name="slotIndex">저장 슬롯 번호 (선택 사항, 기본값은 0)</param>
    /// <returns>불러오기 성공 여부</returns>
    public static bool QuickRestore(Ship targetShip, int slotIndex = 0)
    {
        if (targetShip == null)
            return false;

        string savePath = $"{Application.persistentDataPath}/ship_save_slot_{slotIndex}.es3";
        return RestoreShip(savePath, targetShip);
    }

    /// <summary>
    /// 저장된 함선이 존재하는지 확인합니다.
    /// </summary>
    /// <param name="slotIndex">저장 슬롯 번호 (선택 사항, 기본값은 0)</param>
    /// <returns>저장된 함선이 있으면 true, 없으면 false</returns>
    public static bool SaveExists(int slotIndex = 0)
    {
        string savePath = $"{Application.persistentDataPath}/ship_save_slot_{slotIndex}.es3";
        return ES3.FileExists(savePath) && ES3.KeyExists("ship", savePath);
    }

    /// <summary>
    /// 저장 슬롯 관련 정보를 가져옵니다.
    /// </summary>
    /// <param name="slotIndex">저장 슬롯 번호</param>
    /// <returns>저장 정보</returns>
    public static ShipSaveInfo GetSaveInfo(int slotIndex)
    {
        string savePath = $"{Application.persistentDataPath}/ship_save_slot_{slotIndex}.es3";

        if (!SaveExists(slotIndex))
            return null;

        try
        {
            // 기본 정보만 로드해서 반환 (전체 함선을 로드하지 않음)
            ShipSaveInfo info = new()
            {
                slotIndex = slotIndex,
                saveTime = System.IO.File.GetLastWriteTime(savePath),
                shipName = ES3.Load<string>("ship.shipName", savePath, "Unknown Ship")
            };

            return info;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading save info: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 모든 저장된 함선 슬롯의 정보를 가져옵니다.
    /// </summary>
    /// <param name="maxSlots">확인할 최대 슬롯 수</param>
    /// <returns>저장 정보 목록</returns>
    public static List<ShipSaveInfo> GetAllSaveInfos(int maxSlots = 10)
    {
        List<ShipSaveInfo> result = new();

        for (int i = 0; i < maxSlots; i++)
        {
            ShipSaveInfo info = GetSaveInfo(i);
            if (info != null)
                result.Add(info);
        }

        return result;
    }
}

/// <summary>
/// 함선 저장 슬롯 정보를 담는 클래스
/// </summary>
public class ShipSaveInfo
{
    public int slotIndex;
    public string shipName;
    public DateTime saveTime;

    public string FormattedDateTime => saveTime.ToString("yyyy-MM-dd HH:mm:ss");
}
