using UnityEngine;

/// <summary>
/// 온도 조절 창고를 나타내는 클래스
/// </summary>
public class StorageRoomTemperature : StorageRoomBase
{
    // 전력 공급 중단 시 일반 창고처럼 작동
    private bool hasEnoughPower = false;

    protected override void Start()
    {
        base.Start();
        storageType = StorageType.Temperature;
    }

    protected override void UpdateRoom()
    {
        base.UpdateRoom();

        // 전력 공급 상태 확인
        hasEnoughPower = IsOperational() && isPowered;
    }

    /// <summary>
    /// 온도 조절 창고는 온도 제어가 필요한 물품을 보관 가능
    /// 단, 전력이 공급되지 않는 경우 일반 창고와 동일
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
