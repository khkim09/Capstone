using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 창고 타입의 기본 클래스.
/// 저장 가능한 아이템의 타입, 보관/제거/저하 로직 등을 공통으로 정의합니다.
/// </summary>
public abstract class StorageRoomBase : Room<StorageRoomBaseData, StorageRoomBaseData.StorageRoomBaseLevel>
{
    /// <summary>이 창고의 저장 타입 (예: 식량, 자원, 상품 등).</summary>
    [SerializeField] protected StorageType storageType;

    /// <summary>저장된 아이템 목록.</summary>
    protected Dictionary<string, TradableItem> storedItems = new();


    /// <summary>
    /// 초기화 시 방 타입을 Storage로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        roomType = RoomType.Storage;
    }

    /// <summary>
    /// 이 방이 함선 스탯에 기여하는 값을 계산합니다.
    /// 작동 여부에 따라 전력 사용량을 기여합니다.
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
    /// 현재 창고의 저장 타입을 반환합니다.
    /// </summary>
    public StorageType GetStorageType()
    {
        return storageType;
    }

    /// <summary>
    /// 해당 아이템을 저장할 수 있는지 여부를 반환합니다.
    /// 파생 클래스에서 구현합니다.
    /// </summary>
    public abstract bool CanStoreItem(TradableItem item);

    /// <summary>
    /// 아이템을 창고에 추가합니다.
    /// </summary>
    public virtual bool AddItem(TradableItem item, int quantity)
    {
        if (!CanStoreItem(item))
            return false;

        return true;
    }

    /// <summary>
    /// 아이템을 창고에서 제거합니다.
    /// </summary>
    public virtual bool RemoveItem(TradableItem item, int quantity)
    {
        return true;
    }

    /// <summary>
    /// 워프 후 아이템 상태를 확인 및 갱신합니다.
    /// </summary>
    public virtual void CheckItemsAfterWarp()
    {
    }

    /// <summary>
    /// 아이템의 상태를 저하시킵니다.
    /// 아이템은 점점 더 손상된 상태로 변경됩니다.
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
