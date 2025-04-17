using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// </summary>
public class StorageSystem : ShipSystem
{
    /// <summary>
    /// </summary>
    /// <param name="ship">초기화 대상 함선 객체.</param>
    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);
    }

    /// <summary>
    /// 매 프레임마다 호출되어 시스템 상태를 갱신합니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
    public override void Update(float deltaTime)
    {
    }

    public List<TradingItem> GetAllItems()
    {
        List<TradingItem> allItems = new();
        foreach (Room room in parentShip.GetAllRooms())
            if (room is StorageRoomBase storageRoom)
                allItems.AddRange(storageRoom.GetStoredItems());

        return allItems;
    }

    public List<TradingItem> GetAllItemsByCategory(ItemCategory category)
    {
        List<TradingItem> allItems = new();
        foreach (Room room in parentShip.GetAllRooms())
            if (room is StorageRoomBase storageRoom)
                allItems.AddRange(storageRoom.GetStoredItemsByCategory(category));

        return allItems;
    }

    public List<TradingItem> GetAllItemsById(int id)
    {
        List<TradingItem> allItems = new();
        foreach (Room room in parentShip.GetAllRooms())
            if (room is StorageRoomBase storageRoom)
                allItems.AddRange(storageRoom.GetStoredItemsById(id));

        return allItems;
    }

    public List<TradingItem> GetAllItemsByName(string name)
    {
        List<TradingItem> allItems = new();
        foreach (Room room in parentShip.GetAllRooms())
            if (room is StorageRoomBase storageRoom)
                allItems.AddRange(storageRoom.GetStoredItemsByName(name));

        return allItems;
    }

    public void SetOtherRoomsGray()
    {
        foreach (Room room in parentShip.GetAllRooms())
        {
            if (!(room is StorageRoomBase storageRoom))
            {
                SpriteRenderer spriteRenderer = room.GetComponent<SpriteRenderer>();
                spriteRenderer.color = Color.gray;
            }
        }
    }
}
