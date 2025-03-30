using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 탄약고를 나타내는 클래스
/// </summary>
public class AmmunitionRoom : Room<AmmunitionRoomData, AmmunitionRoomData.AmmunitionRoomLevel>
{
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.Ammunition;
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
            // 탄약고 레벨 데이터에서 기여도
            contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
            contributions[ShipStat.ReloadTimeBonus] = currentRoomLevelData.reloadTimeBonus;
            contributions[ShipStat.DamageBonus] = currentRoomLevelData.damageBonus;
        }
        else
        {
            if (currentHitPoints < GetMaxHitPoints())
            {
                float healthRate = GetHealthPercentage();


                if (healthRate <= currentRoomLevelData.damageHitPointRate[RoomDamageLevel.DamageLevelOne])
                {
                    AmmunitionRoomData.AmmunitionRoomLevel weakedRoomLevelData =
                        roomData.GetTypedRoomData(currentLevel - 1);
                    contributions[ShipStat.PowerUsing] = weakedRoomLevelData.powerRequirement;
                    contributions[ShipStat.ReloadTimeBonus] = weakedRoomLevelData.reloadTimeBonus;
                    contributions[ShipStat.DamageBonus] = weakedRoomLevelData.damageBonus;
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
    /// 탄약고 손상 처리
    /// </summary>
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);


        UpdateEffects();
    }
}
