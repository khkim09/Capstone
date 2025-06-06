﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 산소실(RoomType.Oxygen)을 나타내는 클래스.
/// 산소 생성량 및 전력 사용량에 기여하며, 손상 상태에 따라 작동이 제한됩니다.
/// </summary>
public class OxygenRoom : Room<OxygenRoomData, OxygenRoomData.OxygenLevel>
{
    /// <summary>
    /// 초기화 시 방 타입을 Oxygen으로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.Oxygen;
        workDirection = Vector2Int.zero;
    }

    /// <summary>
    /// 이 방이 함선 스탯에 기여하는 값을 계산합니다.
    /// 작동 여부 및 손상 상태에 따라 산소 생성량과 전력 소비량이 조정됩니다.
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
            // 산소실 레벨 데이터에서 기여도
            contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
            contributions[ShipStat.OxygenGeneratePerSecond] = currentRoomLevelData.oxygenSupplyPerSecond;
        }

        if (currentLevel > 1 && damageCondition == DamageLevel.scratch)
        {
            OxygenRoomData.OxygenLevel weakedRoomLevelData = roomData.GetTypedRoomData(currentLevel - 1);
            contributions[ShipStat.PowerUsing] = weakedRoomLevelData.powerRequirement;
            contributions[ShipStat.OxygenGeneratePerSecond] = weakedRoomLevelData.oxygenSupplyPerSecond;
        }
        return contributions;
    }

    /// <summary>
    /// 산소실이 피해를 받을 때 호출됩니다.
    /// 이펙트를 갱신합니다.
    /// </summary>
    /// <param name="damage">받은 피해량.</param>
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);


        UpdateEffects();
    }
}
