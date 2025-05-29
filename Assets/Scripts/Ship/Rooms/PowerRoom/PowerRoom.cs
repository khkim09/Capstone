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
        workDirection=Vector2Int.up;
    }

    public override void Initialize(int level)
    {
        base.Initialize(level);
        isPowerRequested = true;
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

        if (isActive)
        {
            contributions[ShipStat.PowerCapacity] = currentRoomLevelData.powerRequirement;
        }

        if (currentLevel > 1 && damageCondition == DamageLevel.scratch)
        {
            PowerRoomData.PowerRoomLevel weakedRoomLevelData = roomData.GetTypedRoomData(currentLevel - 1);
            contributions[ShipStat.PowerCapacity] = weakedRoomLevelData.powerRequirement;
        }

        if (workingCrew != null)
        {
            float crewBonus = workingCrew.GetCrewSkillValue()[SkillType.MedBaySkill];
            contributions[ShipStat.PowerCapacity] *= crewBonus;
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
        if (crew.GetCrewSkillValue().ContainsKey(SkillType.PowerSkill) && workingCrew ==null && IsOperational())
        {
            workingCrew = crew;
            return true;
        }
        return false;
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
