using UnityEngine;

/// <summary>
/// 온도 조절 창고(StorageType.Temperature)를 나타내는 클래스.
/// 전력이 공급되면 냉장/냉동 보관 가능, 전력이 끊기면 일반 창고처럼 작동합니다.
/// </summary>
public class StorageRoomTemperature : StorageRoomBase
{
    /// <summary>전력이 충분히 공급되고 있는지 여부.</summary>
    private bool hasEnoughPower = false;

    /// <summary>
    /// 초기화 시 저장 타입을 Temperature로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        storageType = StorageType.Temperature;
    }

    /// <summary>
    /// 매 프레임 전력 상태를 확인하여 보관 가능 여부에 반영합니다.
    /// </summary>
    protected override void UpdateRoom()
    {
        base.UpdateRoom();

        // 전력 공급 상태 확인
        hasEnoughPower = IsOperational() && isPowered;
    }

    /// <summary>
    /// 전력이 공급되면 냉장/냉동 아이템 보관 가능,
    /// 공급되지 않으면 상온 아이템만 보관 가능합니다.
    /// </summary>
    public override bool CanStoreItem(TradableItem item)
    {
        if (!hasEnoughPower)
        {
            // 전력 부족 시 일반 창고와 동일 기능
            //return item.storageRequirement == TradableItem.StorageRequirement.Normal;
        }

        // 온도 제어가 필요한 아이템인지 확인
        return true;
    }

    /// <summary>
    /// 전력 상태를 설정하고, 전력 차단 시 보관 아이템 상태를 체크합니다.
    /// </summary>
    public override void SetPowerStatus(bool powered, bool requested)
    {
        base.SetPowerStatus(powered, requested);

        // 전력 상태 변경 시 아이템 확인
        if (!powered && hasEnoughPower)
            // 전력 공급이 끊겼을 때 아이템 상태 체크
            CheckItemsAfterWarp();

        hasEnoughPower = powered;
    }
}
