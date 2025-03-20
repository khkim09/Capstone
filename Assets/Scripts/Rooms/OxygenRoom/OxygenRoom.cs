using System.Collections.Generic;
using UnityEngine;

public class OxygenRoom : Room<OxygenRoomData, OxygenRoomData.OxygenLevel>
{
    [Header("산소실 효과")] [SerializeField] private ParticleSystem OxygenParticles;
    [SerializeField] private AudioSource oxygenSound;

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
            contributions[ShipStat.OxygenPerSecond] = currentRoomLevelData.oxygenSupplyPerSecond;
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
                    contributions[ShipStat.OxygenPerSecond] = weakedRoomLevelData.oxygenSupplyPerSecond;
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
