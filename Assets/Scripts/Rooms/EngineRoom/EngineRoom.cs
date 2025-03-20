using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 엔진실을 나타내는 클래스
/// </summary>
public class EngineRoom : Room<EngineRoomData, EngineRoomData.EngineRoomLevel>
{
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.Engine;
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
            // 엔진실 레벨 데이터에서 기여도 추가
            contributions[ShipStat.DodgeChance] = currentRoomLevelData.avoidEfficiency;
            contributions[ShipStat.FuelEfficiency] = currentRoomLevelData.fuelEfficiency;
            contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
        }
        else
        {
            if (currentHitPoints < GetMaxHitPoints())
            {
                float healthRate = currentHitPoints / GetMaxHitPoints();

                if (healthRate <= currentRoomLevelData.damageHitPointRate[RoomDamageLevel.DamageLevelOne])
                {
                    EngineRoomData.EngineRoomLevel weakedRoomLevelData = roomData.GetTypedRoomData(currentLevel - 1);
                    contributions[ShipStat.DodgeChance] = weakedRoomLevelData.avoidEfficiency;
                    contributions[ShipStat.FuelEfficiency] = weakedRoomLevelData.fuelEfficiency;
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
    /// 엔진실 손상 처리
    /// </summary>
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        // // 엔진 손상 시 특수 효과
        // if (currentHitPoints < GetMaxHitPoints() * 0.3f && engineParticles != null)
        // {
        //     ParticleSystem.EmissionModule emission = engineParticles.emission;
        //     emission.rateOverTime = Mathf.Lerp(10f, 30f, 1f - currentHitPoints / GetMaxHitPoints());
        //
        //     ParticleSystem.MainModule main = engineParticles.main;
        //     main.startColor = new Color(1f, 0.5f, 0f, 0.8f); // 손상 시 주황색 파티클
        // }

        // 엔진 효과 업데이트
        UpdateEffects();
    }
}
