using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 엔진실(RoomType.Engine)을 나타내는 클래스.
/// 회피율, 연료 효율, 연료 소모량 등에 기여하며, 손상 상태에 따라 성능이 저하됩니다.
/// </summary>
public class EngineRoom : Room<EngineRoomData, EngineRoomData.EngineRoomLevel>
{
    /// <summary>
    /// 초기화 시 방 타입을 엔진실로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.Engine;
        workDirection=Vector2Int.up;
    }


    /// <summary>
    /// 이 방이 함선 스탯에 기여하는 값을 계산합니다.
    /// 작동 상태 및 손상 정도에 따라 수치가 조정됩니다.
    /// </summary>
    /// <returns>스탯 기여도 딕셔너리.</returns>
    public override Dictionary<ShipStat, float> GetStatContributions()
    {
        // 기본 기여도 가져오기 (작동 상태 체크 등)
        Dictionary<ShipStat, float> contributions = base.GetStatContributions();

        // 작동 상태가 아니면 기여도 없음
        if (!IsOperational() || currentRoomLevelData == null)
            return contributions;

        if (isActive)
        {
            contributions[ShipStat.DodgeChance] = currentRoomLevelData.avoidEfficiency;
            contributions[ShipStat.FuelEfficiency] = currentRoomLevelData.fuelEfficiency;
            contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
            contributions[ShipStat.FuelConsumption] = currentRoomLevelData.fuelConsumption;
        }

        if (currentLevel > 1 && damageCondition == DamageLevel.scratch)
        {
            EngineRoomData.EngineRoomLevel weakedRoomLevelData = roomData.GetTypedRoomData(currentLevel - 1);
            contributions[ShipStat.DodgeChance] = weakedRoomLevelData.avoidEfficiency;
            contributions[ShipStat.FuelEfficiency] = weakedRoomLevelData.fuelEfficiency;
            contributions[ShipStat.PowerUsing] = weakedRoomLevelData.powerRequirement;
            contributions[ShipStat.FuelConsumption] = weakedRoomLevelData.fuelConsumption;
        }

        if (workingCrew != null)
        {
            float crewBonus = workingCrew.GetCrewSkillValue()[SkillType.EngineSkill];
            contributions[ShipStat.DodgeChance] *= crewBonus;
            contributions[ShipStat.FuelEfficiency] *= crewBonus;
            contributions[ShipStat.FuelConsumption] *= crewBonus;
        }

        return contributions;
    }

    /// <summary>
    /// 선원이 작업을 해도 되냐고 물어보고 되면 workingCrew로 할당해주고 아님 말고
    /// </summary>
    /// <param name="crew"></param>
    /// <returns></returns>
    public override bool CanITouch(CrewMember crew)
    {
        if (crew.GetCrewSkillValue().ContainsKey(SkillType.EngineSkill) && workingCrew ==null)
        {
            workingCrew = crew;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 엔진실이 피해를 받을 때 호출됩니다.
    /// 효과를 갱신하고, 손상 시 시각 효과를 적용할 수 있습니다.
    /// </summary>
    /// <param name="damage">받는 피해량.</param>
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
