using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 창고 타입의 기본 클래스
/// </summary>
public abstract class StorageRoomBase : Room<StorageRoomBaseData, StorageRoomBaseData.StorageRoomBaseLevel>
{
    [SerializeField] protected StorageType storageType;

    // 저장된 아이템 목록
    protected Dictionary<string, TradableItem> storedItems = new();

    protected override void Start()
    {
        base.Start();
        roomType = RoomType.Storage;
    }

    /// <summary>
    /// 이 방의 스탯 기여도 계산 (공통 로직)
    /// </summary>
    public override Dictionary<ShipStat, float> GetStatContributions()
    {
        // 기본 기여도 가져오기 (작동 상태 체크 등)
        Dictionary<ShipStat, float> contributions = base.GetStatContributions();

        // 작동 상태가 아니면 기여도 없음
        if (!IsOperational() || currentRoomLevelData == null)
            return contributions;

        // 창고 레벨 데이터에서 기여도
        contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;

        return contributions;
    }

    /// <summary>
    /// 창고 타입 반환
    /// </summary>
    public StorageType GetStorageType()
    {
        return storageType;
    }

    /// <summary>
    /// 물품 보관 가능 여부 확인 (타입별 오버라이드)
    /// </summary>
    public abstract bool CanStoreItem(TradableItem item);

    /// <summary>
    /// 물품 추가
    /// </summary>
    public virtual bool AddItem(TradableItem item, int quantity)
    {
        if (!CanStoreItem(item))
            return false;

        return true;
    }

    /// <summary>
    /// 물품 제거
    /// </summary>
    public virtual bool RemoveItem(TradableItem item, int quantity)
    {
        return true;
    }

    /// <summary>
    /// 워프 후 아이템 상태 체크
    /// </summary>
    public virtual void CheckItemsAfterWarp()
    {
    }

    /// <summary>
    /// 물품 상태 저하
    /// </summary>
    protected virtual void DegradeItemState(TradableItem item)
    {
        switch (item.itemState)
        {
            case ItemState.Normal:
                item.itemState = ItemState.SlightlyDamaged;
                break;
            case ItemState.SlightlyDamaged:
                item.itemState = ItemState.ModeratelyDamaged;
                break;
            case ItemState.ModeratelyDamaged:
                item.itemState = ItemState.Unsellable;
                break;
        }
    }
}
