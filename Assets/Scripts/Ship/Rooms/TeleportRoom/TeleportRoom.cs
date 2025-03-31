using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 텔레포트실(RoomType.Teleporter)을 나타내는 클래스.
/// 전력을 소비하며, 손상 상태에 따라 작동 여부가 제한됩니다.
/// </summary>
public class TeleportRoom : Room<TeleportRoomData, TeleportRoomData.TeleportRoomLevel>
{
    /// <summary>
    /// 초기화 시 방 타입을 Teleporter로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.Teleporter;
    }

    /// <summary>
    /// 이 방이 함선 스탯에 기여하는 값을 계산합니다.
    /// 전력 사용량이 포함되며, 손상 시 단계별 기여도 감소 또는 작동 불능 처리됩니다.
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
            // 텔레포트실 레벨 데이터에서 기여도
            contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
        }
        else
        {
            if (currentHitPoints < GetMaxHitPoints())
            {
                float healthRate = GetHealthPercentage();

                if (healthRate <= currentRoomLevelData.damageHitPointRate[RoomDamageLevel.DamageLevelOne])
                {
                    TeleportRoomData.TeleportRoomLevel
                        weakedRoomLevelData = roomData.GetTypedRoomData(currentLevel - 1);
                    contributions[ShipStat.PowerUsing] = weakedRoomLevelData.powerRequirement;
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
    /// 텔레포트실이 피해를 받을 때 호출됩니다.
    /// 이펙트를 갱신합니다.
    /// </summary>
    /// <param name="damage">받은 피해량.</param>
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);


        UpdateEffects();
    }
}
