using UnityEngine;

/// <summary>
/// 동물 우리를 나타내는 클래스
/// </summary>
public class StorageRoomAnimal : StorageRoomBase
{
    protected override void Start()
    {
        base.Start();
        storageType = StorageType.Animal;
    }

    /// <summary>
    /// 동물 우리는 동물 카테고리 물품만 보관 가능
    /// </summary>
    public override bool CanStoreItem(TradableItem item)
    {
        // 동물 카테고리 아이템인지 확인
        // return item.category == TradableItem.Category.Animal;
        return true;
    }
}
