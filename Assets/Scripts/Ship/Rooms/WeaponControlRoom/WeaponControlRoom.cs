using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 조준석(RoomType.WeaponControl)을 나타내는 클래스.
/// 명중률(Accuracy)에 기여하며, 전력을 소비합니다.
/// 손상 시 성능이 저하되거나 비작동 상태가 됩니다.
/// </summary>
public class WeaponControlRoom : Room<WeaponControlRoomData, WeaponControlRoomData.WeaponControlRoomLevel>
{
    /// <summary>
    /// 초기화 시 방 타입을 WeaponControl로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.WeaponControl;
    }

    /// <summary>
    /// 이 방이 함선 스탯에 기여하는 값을 계산합니다.
    /// 작동 상태 및 손상 정도에 따라 전력 사용량과 명중률 보너스가 달라집니다.
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
            // 조준석 레벨 데이터에서 기여도
            contributions[ShipStat.Accuracy] = currentRoomLevelData.accuracy;
            contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
        }
        else
        {
            if (currentHitPoints < GetMaxHitPoints())
            {
                float healthRate = GetHealthPercentage();

                if (healthRate <= currentRoomLevelData.damageHitPointRate[RoomDamageLevel.DamageLevelOne])
                {
                    WeaponControlRoomData.WeaponControlRoomLevel weakedRoomLevelData =
                        roomData.GetTypedRoomData(currentLevel - 1);
                    contributions[ShipStat.PowerUsing] = weakedRoomLevelData.powerRequirement;
                    contributions[ShipStat.Accuracy] = currentRoomLevelData.accuracy;
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
    /// 조준석이 피해를 받을 때 호출됩니다.
    /// 이펙트를 갱신합니다.
    /// </summary>
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);


        UpdateEffects();
    }
}
