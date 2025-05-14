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
        workDirection=Vector2Int.up;
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

        if (isActive)
        {
            // 배리어실 레벨 데이터에서 기여도
            contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
            contributions[ShipStat.HealPerSecond] = currentRoomLevelData.healPerSecond;
        }

        if (currentLevel > 1 && damageCondition == DamageLevel.scratch)
        {
            MedBayRoomData.MedBayRoomLevel weakedRoomLevelData = roomData.GetTypedRoomData(currentLevel - 1);
            contributions[ShipStat.PowerUsing] = weakedRoomLevelData.powerRequirement;
            contributions[ShipStat.HealPerSecond] = weakedRoomLevelData.healPerSecond;
        }

        if (workingCrew != null)
        {
            float crewBonus = workingCrew.GetCrewSkillValue()[SkillType.MedBaySkill];
            contributions[ShipStat.HealPerSecond] *= crewBonus;
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
        if (crew.GetCrewSkillValue().ContainsKey(SkillType.MedBaySkill) && workingCrew ==null)
        {
            workingCrew = crew;
            return true;
        }
        return false;
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
