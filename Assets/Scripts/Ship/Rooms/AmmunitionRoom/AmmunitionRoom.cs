using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 탄약고를 나타내는 클래스.
/// 전투 관련 스탯 (재장전 속도, 데미지 보너스 등)에 기여하며, 손상 상태에 따라 성능이 달라집니다.
/// </summary>
public class AmmunitionRoom : Room<AmmunitionRoomData, AmmunitionRoomData.AmmunitionRoomLevel>
{
    /// <summary>
    /// 초기화 시 방 타입을 탄약고로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.Ammunition;
        workDirection=Vector2Int.up;
    }

    /// <summary>
    /// 이 방이 함선의 스탯에 기여하는 수치를 계산합니다.
    /// 작동 상태 및 손상 정도에 따라 기여도가 달라집니다.
    /// </summary>
    /// <returns>기여하는 ShipStat 값의 딕셔너리.</returns>
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
            if (damageCondition == DamageLevel.good)
            {
                contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
                contributions[ShipStat.ReloadTimeBonus] = currentRoomLevelData.reloadTimeBonus;
                contributions[ShipStat.DamageBonus] = currentRoomLevelData.damageBonus;
            }
            else if (damageCondition == DamageLevel.scratch)
            {
                AmmunitionRoomData.AmmunitionRoomLevel weakedRoomLevelData =
                    roomData.GetTypedRoomData(currentLevel - 1);
                contributions[ShipStat.PowerUsing] = weakedRoomLevelData.powerRequirement;
                contributions[ShipStat.ReloadTimeBonus] = weakedRoomLevelData.reloadTimeBonus;
                contributions[ShipStat.DamageBonus] = weakedRoomLevelData.damageBonus;
            }
        }

        if(workingCrew!=null)
        {
            float crewBonus = workingCrew.GetCrewSkillValue()[SkillType.AmmunitionSkill];
            contributions[ShipStat.ReloadTimeBonus] *= crewBonus;
            contributions[ShipStat.DamageBonus] *= crewBonus;
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
        if (crew.GetCrewSkillValue().ContainsKey(SkillType.AmmunitionSkill) && workingCrew ==null &&IsOperational())
        {
            workingCrew = crew;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 탄약고가 데미지를 받을 때 호출됩니다.
    /// 기본 데미지 처리 이후 이펙트를 갱신합니다.
    /// </summary>
    /// <param name="damage">받는 데미지 양.</param>
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);


        UpdateEffects();
    }
}
