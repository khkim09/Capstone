using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 전력실(RoomType.Power)을 나타내는 클래스.
/// 전력 생산량(PowerCapacity)에 기여하며, 손상 상태에 따라 성능이 저하됩니다.
/// </summary>
public class PowerRoom : Room<PowerRoomData, PowerRoomData.PowerRoomLevel>
{
    /// <summary>
    /// 초기화 시 방 타입을 Power로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.Power;
    }

    /// <summary>
    /// 이 방이 함선 스탯에 기여하는 값을 계산합니다.
    /// 작동 여부 및 손상 상태에 따라 전력 생산량이 달라집니다.
    /// </summary>
    /// <returns>스탯 기여도 딕셔너리.</returns>
    public override Dictionary<ShipStat, float> GetStatContributions()
    {
        // 기본 기여도 가져오기 (작동 상태 체크 등)
        Dictionary<ShipStat, float> contributions = base.GetStatContributions();

        // 작동 상태가 아니면 기여도 없음
        if (!IsOperational() || currentRoomLevelData == null)
            return contributions;

        if (currentLevel == 1)
        {
            // 전력실 레벨 데이터에서 기여도
            contributions[ShipStat.PowerCapacity] = currentRoomLevelData.powerRequirement;
        }
        else
        {
            if (currentHitPoints < GetMaxHitPoints())
            {
                float healthRate = GetHealthPercentage();

                if (healthRate <= currentRoomLevelData.damageHitPointRate[RoomDamageLevel.DamageLevelOne])
                {
                    PowerRoomData.PowerRoomLevel weakedRoomLevelData = roomData.GetTypedRoomData(currentLevel - 1);
                    contributions[ShipStat.PowerCapacity] = weakedRoomLevelData.powerRequirement;
                }
                else if (healthRate <= currentRoomLevelData.damageHitPointRate[RoomDamageLevel.DamageLevelTwo])
                {
                    isActive = false;
                }
            }
        }


        return contributions;
    }

    /// <summary>
    /// 전력실이 피해를 받을 때 호출됩니다.
    /// 이펙트를 갱신합니다.
    /// </summary>
    /// <param name="damage">받은 피해량.</param>
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);


        UpdateEffects();
    }
}
