using UnityEngine;

/// <summary>
/// 일반 창고를 나타내는 클래스
/// </summary>
public class StorageRoomRegular : StorageRoomBase
{
    protected override void Start()
    {
        base.Start();
        storageType = StorageType.Regular;
    }

    /// <summary>
    /// 일반 창고는 상온 보관 가능한 물품만 보관 가능
    /// </summary>
    public override bool CanStoreItem(TradableItem item)
    {
        // 상온 보관 가능한 아이템인지 확인
        return false;
        //return item.storageRequirement == ItemData.StorageRequirement.Normal;
    }
}
