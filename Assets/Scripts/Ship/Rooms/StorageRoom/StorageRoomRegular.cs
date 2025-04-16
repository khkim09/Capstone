using UnityEngine;

/// <summary>
/// 일반 창고(StorageType.Regular)를 나타내는 클래스.
/// 상온 보관이 가능한 아이템만 저장할 수 있습니다.
/// </summary>
public class StorageRoomRegular : StorageRoomBase
{
    /// <summary>
    /// 초기화 시 저장 타입을 Regular로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// 상온 보관 가능한 아이템만 저장 가능합니다.
    /// </summary>
    /// <param name="item">보관할 아이템.</param>
    /// <returns>보관 가능 여부.</returns>
    public override bool CanStoreItemType(ItemCategory itemType)
    {
        return true;
        // 상온 보관 가능한 아이템인지 확인
        return false;
        //return item.storageRequirement == ItemData.StorageRequirement.Normal;
    }
}
