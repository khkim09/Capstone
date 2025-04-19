using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 거래 아이템의 직렬화 및 역직렬화를 담당하는 유틸리티 클래스
/// </summary>
public static class TradingItemSerialization
{
    /// <summary>
    /// 아이템 직렬화 데이터 클래스
    /// </summary>
    [Serializable]
    public class ItemSerializationData
    {
        // 아이템 데이터 ID (스크립터블 오브젝트 참조용)
        public int itemId;

        // 수량 및 상태 정보
        public int amount;
        public ItemState itemState = ItemState.Normal;

        // 위치 정보
        public Vector2Int gridPosition;
        public RotationConstants.Rotation rotation;

        // 가격 관련 정보 (캐싱된 가격이 있는 경우)
        public float? cachedPrice;

        // 저장된 스토리지 정보 (소속된 창고)
        public string storageId; // 창고 식별자
    }

    /// <summary>
    /// 아이템을 직렬화하여 데이터 객체로 변환
    /// </summary>
    /// <param name="item">직렬화할 아이템</param>
    /// <returns>직렬화된 아이템 데이터</returns>
    public static ItemSerializationData SerializeItem(TradingItem item)
    {
        if (item == null)
            return null;

        return new ItemSerializationData
        {
            itemId = item.GetItemId(),
            amount = item.amount,
            itemState = item.GetItemState(),
            gridPosition = item.GetGridPosition(),
            rotation = (RotationConstants.Rotation)item.GetRotation(),
            cachedPrice = item.GetCurrentPrice()
            // TODO: 창고 ID 기록해야 함.
        };
    }

    /// <summary>
    /// 모든 아이템을 직렬화
    /// </summary>
    /// <param name="items">직렬화할 아이템 목록</param>
    /// <returns>직렬화된 아이템 데이터 목록</returns>
    public static List<ItemSerializationData> SerializeAllItems(List<TradingItem> items)
    {
        List<ItemSerializationData> result = new();

        if (items == null)
            return result;

        foreach (TradingItem item in items)
        {
            ItemSerializationData data = SerializeItem(item);
            if (data != null)
                result.Add(data);
        }

        return result;
    }

    /// <summary>
    /// 창고의 모든 아이템을 직렬화
    /// </summary>
    /// <param name="storage">대상 창고</param>
    /// <returns>직렬화된 아이템 데이터 목록</returns>
    public static List<ItemSerializationData> SerializeStorageItems(StorageRoomBase storage)
    {
        if (storage == null)
            return new List<ItemSerializationData>();

        return SerializeAllItems(storage.GetStoredItems());
    }

    /// <summary>
    /// 직렬화된 아이템 데이터로 아이템 객체 생성
    /// </summary>
    /// <param name="data">직렬화된 아이템 데이터</param>
    /// <param name="storage">배치할 창고</param>
    /// <returns>생성된 아이템 객체</returns>
    public static TradingItem DeserializeItem(ItemSerializationData data, StorageRoomBase storage)
    {
        if (data == null || storage == null)
            return null;

        // 아이템 매니저를 통해 아이템 생성
        TradingItem item = GameObjectFactory.Instance.ItemFactory.CreateItemInstance(data.itemId, data.amount);

        if (item != null)
        {
            // 아이템 상태 설정
            item.SetItemState(data.itemState);

            // 회전 설정
            item.Rotate(data.rotation);

            // 그리드 위치 설정
            item.SetGridPosition(data.gridPosition);

            // 부모 스토리지 설정
            item.SetParentStorage(storage);

            // 가격 설정 (캐싱된 가격이 있는 경우)
            if (data.cachedPrice.HasValue)
                // 강제로 가격 초기화를 위한 내부 필드 직접 접근이 필요할 수 있음
                // 현재 구현에서는 불가능하므로 RecalculatePrice()를 호출하는 방식으로 대체
                item.RecalculatePrice();

            // 창고에 아이템 배치
            storage.AddItem(item, data.gridPosition, data.rotation);
        }

        return item;
    }

    /// <summary>
    /// 창고에 모든 아이템 복원
    /// </summary>
    /// <param name="itemDataList">직렬화된 아이템 데이터 목록</param>
    /// <param name="storage">대상 창고</param>
    /// <returns>복원된 아이템 수</returns>
    public static int DeserializeAllItems(List<ItemSerializationData> itemDataList, StorageRoomBase storage)
    {
        if (itemDataList == null || storage == null)
            return 0;

        int restoredCount = 0;

        // 기존 아이템 제거
        storage.DestroyAllItems();


        // 아이템 복원
        foreach (ItemSerializationData data in itemDataList)
        {
            TradingItem item = DeserializeItem(data, storage);
            if (item != null)
                restoredCount++;
        }

        return restoredCount;
    }

    /// <summary>
    /// 아이템 데이터를 JSON 문자열로 변환
    /// </summary>
    /// <param name="data">직렬화할 아이템 데이터</param>
    /// <returns>JSON 문자열</returns>
    public static string ToJson(ItemSerializationData data)
    {
        if (data == null)
            return "{}";

        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }

    /// <summary>
    /// 아이템 데이터 목록을 JSON 문자열로 변환
    /// </summary>
    /// <param name="dataList">직렬화할 아이템 데이터 목록</param>
    /// <returns>JSON 문자열</returns>
    public static string ToJson(List<ItemSerializationData> dataList)
    {
        if (dataList == null)
            return "[]";

        return JsonConvert.SerializeObject(dataList, Formatting.Indented);
    }

    /// <summary>
    /// JSON 문자열에서 아이템 데이터 복원
    /// </summary>
    /// <param name="json">JSON 문자열</param>
    /// <returns>복원된 아이템 데이터</returns>
    public static ItemSerializationData FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonConvert.DeserializeObject<ItemSerializationData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"아이템 데이터 역직렬화 오류: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// JSON 문자열에서 아이템 데이터 목록 복원
    /// </summary>
    /// <param name="json">JSON 문자열</param>
    /// <returns>복원된 아이템 데이터 목록</returns>
    public static List<ItemSerializationData> FromJsonList(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<ItemSerializationData>();

        try
        {
            return JsonConvert.DeserializeObject<List<ItemSerializationData>>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"아이템 데이터 목록 역직렬화 오류: {e.Message}");
            return new List<ItemSerializationData>();
        }
    }
}
