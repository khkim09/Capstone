using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 산소실을 나타내는 클래스
/// </summary>
public class OxygenRoom : Room<OxygenRoomData, OxygenRoomData.OxygenLevel>
{
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.Oxygen;
    }

    /// <summary>
    /// 이 방의 스탯 기여도 계산
    /// </summary>
    public override Dictionary<ShipStat, float> GetStatContributions()
    {
        // 기본 기여도 가져오기 (작동 상태 체크 등)
        Dictionary<ShipStat, float> contributions = base.GetStatContributions();

        // 작동 상태가 아니면 기여도 없음
        if (!IsOperational() || currentRoomLevelData == null)
            return contributions;

        if (currentLevel == 1)
        {
            // 산소실 레벨 데이터에서 기여도
            contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
            contributions[ShipStat.OxygenGeneratePerSecond] = currentRoomLevelData.oxygenSupplyPerSecond;
        }
        else
        {
            if (currentHitPoints < GetMaxHitPoints())
            {
                float healthRate = currentHitPoints / GetMaxHitPoints();

                if (healthRate <= currentRoomLevelData.damageHitPointRate[RoomDamageLevel.DamageLevelOne])
                {
                    OxygenRoomData.OxygenLevel weakedRoomLevelData = roomData.GetTypedRoomData(currentLevel - 1);
                    contributions[ShipStat.PowerUsing] = weakedRoomLevelData.powerRequirement;
                    contributions[ShipStat.OxygenGeneratePerSecond] = weakedRoomLevelData.oxygenSupplyPerSecond;
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
    /// 산소실 손상 처리
    /// </summary>
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);


        UpdateEffects();
    }
}
