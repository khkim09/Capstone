using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Easy Save 3를 사용하여 무역 아이템(TradingItem)의 직렬화 및 역직렬화를 담당하는 유틸리티 클래스
/// </summary>
public static class TradingItemSerialization
{
    /// <summary>
    /// 모든 무역 아이템을 저장합니다.
    /// </summary>
    /// <param name="items">저장할 무역 아이템 목록</param>
    /// <param name="filename">저장 파일명</param>
    public static void SaveAllTradingItems(List<TradingItem> items, string filename)
    {
        // 파일이 이미 존재한다면 해당 파일 삭제 (덮어쓰기)
        if (ES3.FileExists(filename))
            ES3.DeleteFile(filename);

        // 모든 무역 아이템 저장
        ES3.Save("itemCount", items.Count, filename);

        ES3Settings settings = new() { referenceMode = ES3.ReferenceMode.ByRef };

        for (int i = 0; i < items.Count; i++)
            ES3.Save($"tradingItem_{i}", items[i], filename, settings);
    }

    /// <summary>
    /// 창고에 저장된 모든 무역 아이템을 저장합니다.
    /// </summary>
    /// <param name="storage">무역 아이템이 저장된 창고</param>
    /// <param name="filename">저장 파일명</param>
    public static void SaveStorageItems(StorageRoomBase storage, string filename)
    {
        if (storage == null)
            return;

        SaveAllTradingItems(storage.storedItems, filename);
    }

    /// <summary>
    /// 함선의 모든 창고에 저장된 무역 아이템을 저장합니다.
    /// </summary>
    /// <param name="ship">무역 아이템이 저장된 함선</param>
    /// <param name="filename">저장 파일명</param>
    public static void SaveShipItems(Ship ship, string filename)
    {
        if (ship == null)
            return;

        List<TradingItem> allItems = new();

        foreach (Room room in ship.GetAllRooms())
            if (room is StorageRoomBase storage)
                allItems.AddRange(storage.storedItems);

        SaveAllTradingItems(allItems, filename);
    }

    /// <summary>
    /// 저장된 모든 무역 아이템을 불러옵니다.
    /// </summary>
    /// <param name="filename">불러올 파일명</param>
    /// <returns>불러온 무역 아이템 목록</returns>
    public static List<TradingItem> LoadAllTradingItems(string filename)
    {
        List<TradingItem> items = new();

        if (!ES3.FileExists(filename))
            return items;
        ES3Settings settings = new() { referenceMode = ES3.ReferenceMode.ByRef };
        // 아이템 수 불러오기
        int itemCount = ES3.Load<int>("itemCount", filename);

        // 각 아이템 불러오기
        for (int i = 0; i < itemCount; i++)
            if (ES3.KeyExists($"tradingItem_{i}", filename))
            {
                TradingItem item = ES3.Load<TradingItem>($"tradingItem_{i}", filename, settings);
                if (item != null)
                    items.Add(item);
            }

        return items;
    }

    /// <summary>
    /// 창고에 모든 무역 아이템을 복원합니다.
    /// </summary>
    /// <param name="filename">불러올 파일명</param>
    /// <param name="storage">대상 창고</param>
    /// <returns>복원된 무역 아이템 수</returns>
    public static int RestoreAllItemsToStorage(string filename, StorageRoomBase storage)
    {
        if (!ES3.FileExists(filename) || storage == null)
            return 0;

        // 기존 아이템 제거
        List<TradingItem> existingItems = new(storage.storedItems);
        foreach (TradingItem item in existingItems)
            storage.RemoveItem(item);

        List<TradingItem> items = LoadAllTradingItems(filename);

        // ItemFactory를 통해 아이템 인스턴스 생성 및 추가
        foreach (TradingItem item in items) storage.AddItem(item, item.GetGridPosition(), item.rotation);

        return items.Count;
    }

    /// <summary>
    /// 함선의 모든 창고에 무역 아이템을 복원합니다.
    /// </summary>
    /// <param name="filename">불러올 파일명</param>
    /// <param name="ship">대상 함선</param>
    /// <returns>복원된 무역 아이템 수</returns>
    public static int RestoreAllItemsToShip(Ship ship, string filename)
    {
        if (!ES3.FileExists(filename) || ship == null)
            return 0;

        foreach (Room room in ship.GetAllRooms())
            if (room is StorageRoomBase storage)
                storage.DestroyAllItems();

        List<TradingItem> items = LoadAllTradingItems(filename);
        int restoredCount = 0;

        // 각 아이템을 원래 있던 창고에 복원
        foreach (TradingItem item in items)
        {
            // 새 아이템 인스턴스 생성
            TradingItem newItem =
                GameObjectFactory.Instance.CreateItemObject(item);

            // 적절한 창고 찾기
            StorageRoomBase targetStorage = newItem.GetParentStorage();


            // 적합한 창고를 찾았다면 아이템 추가
            if (targetStorage != null)
            {
                targetStorage.RemoveAllItems();

                if (targetStorage.AddItem(newItem, newItem.GetGridPosition(), newItem.rotation))
                    restoredCount++;
                else
                    Debug.LogError("문제 있음");
            }
            else
            {
                GameObject.Destroy(newItem.gameObject); // 창고를 찾지 못한 경우 객체 삭제
            }
        }

        return restoredCount;
    }

    /// <summary>
    /// 아이템이 속한 창고를 찾습니다.
    /// </summary>
    /// <param name="item">찾을 아이템</param>
    /// <returns>아이템이 속한 창고. 없으면 null.</returns>
    private static StorageRoomBase FindParentStorage(TradingItem item)
    {
        if (item == null)
            return null;

        // 모든 스토리지 검색
        foreach (StorageRoomBase storage in GameObject.FindObjectsOfType<StorageRoomBase>())
            if (storage.storedItems.Contains(item))
                return storage;

        return null;
    }
}
