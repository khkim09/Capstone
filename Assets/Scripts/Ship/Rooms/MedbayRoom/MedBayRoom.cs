using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 의무실(RoomType.MedBay)을 나타내는 클래스.
/// 체력 회복 기능(초당 회복량)을 제공하며, 전력을 소비합니다.
/// 손상 상태에 따라 성능이 저하됩니다.
/// </summary>
public class MedBayRoom : Room<MedBayRoomData, MedBayRoomData.MedBayRoomLevel>
{
    /// <summary>
    /// 초기화 시 방 타입을 MedBay로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();


        // 방 타입 설정
        roomType = RoomType.MedBay;
    }

    /// <summary>
    /// 이 방이 함선 스탯에 기여하는 값을 계산합니다.
    /// 작동 상태 및 손상 정도에 따라 회복량과 전력 사용량이 조정됩니다.
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
            // 배리어실 레벨 데이터에서 기여도
            contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
            contributions[ShipStat.HealPerSecond] = currentRoomLevelData.healPerSecond;
        }
        else
        {
            if (currentHitPoints < GetMaxHitPoints())
            {
                float healthRate = GetHealthPercentage();

                if (healthRate <= currentRoomLevelData.damageHitPointRate[RoomDamageLevel.DamageLevelOne])
                {
                    MedBayRoomData.MedBayRoomLevel weakedRoomLevelData = roomData.GetTypedRoomData(currentLevel - 1);
                    contributions[ShipStat.PowerUsing] = weakedRoomLevelData.powerRequirement;
                    contributions[ShipStat.HealPerSecond] = weakedRoomLevelData.healPerSecond;
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
    /// 의무실이 피해를 받을 때 호출됩니다.
    /// 시각 효과를 갱신합니다.
    /// </summary>
    /// <param name="damage">받는 피해량.</param>
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);


        UpdateEffects();
    }
}
