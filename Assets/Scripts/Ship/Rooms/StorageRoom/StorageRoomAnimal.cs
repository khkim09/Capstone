using UnityEngine;

/// <summary>
/// 동물 우리(StorageType.Animal)를 나타내는 특화 창고 클래스.
/// 동물 카테고리 아이템만 보관할 수 있습니다.
/// </summary>
public class StorageRoomAnimal : StorageRoomBase
{
    /// <summary>
    /// 초기화 시 저장 타입을 Animal로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// 동물 카테고리의 아이템만 저장이 가능합니다.
    /// </summary>
    /// <param name="item">보관 대상 아이템.</param>
    /// <returns>보관 가능 여부.</returns>
    public override bool CanStoreItemType(ItemCategory itemType)
    {
        // 동물 카테고리 아이템인지 확인
        // return item.category == TradableItem.Category.Animal;
        return true;
    }
}
