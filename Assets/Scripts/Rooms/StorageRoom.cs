using UnityEngine;

public class StorageRoom : Room
{
    public Vector2Int gridSize; // 창고 그리드 크기

    // public Dictionary<Item, int> inventory; // 보관 물품
    public StorageType storageType; // 일반/온도조절/동물우리
}

public enum StorageType
{
    Normal,
    Temperature,
    Animal
}
