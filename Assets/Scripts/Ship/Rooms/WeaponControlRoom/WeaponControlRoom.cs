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
        workDirection=Vector2Int.left;
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

        if (isActive)
        {
            // 조준석 레벨 데이터에서 기여도
            contributions[ShipStat.Accuracy] = currentRoomLevelData.accuracy;
            contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
        }

        if (currentLevel > 1 && damageCondition == DamageLevel.scratch)
        {
            WeaponControlRoomData.WeaponControlRoomLevel weakedRoomLevelData =
                roomData.GetTypedRoomData(currentLevel - 1);
            contributions[ShipStat.PowerUsing] = weakedRoomLevelData.powerRequirement;
            contributions[ShipStat.Accuracy] = currentRoomLevelData.accuracy;
        }

        if (workingCrew != null)
        {
            float crewBonus = workingCrew.GetCrewSkillValue()[SkillType.WeaponSkill];
            contributions[ShipStat.Accuracy] *= crewBonus;
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
        if (crew.GetCrewSkillValue().ContainsKey(SkillType.WeaponSkill) && workingCrew ==null)
        {
            workingCrew = crew;
            return true;
        }
        return false;
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
