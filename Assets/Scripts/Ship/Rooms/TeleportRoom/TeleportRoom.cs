using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 함선의 텔레포트실(RoomType.Teleporter)을 나타내는 클래스.
/// 전력을 소비하며, 손상 상태에 따라 작동 여부가 제한됩니다.
/// </summary>
public class TeleportRoom : Room<TeleportRoomData, TeleportRoomData.TeleportRoomLevel>
{
    /// <summary>
    /// 초기화 시 방 타입을 Teleporter로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.Teleporter;
        workDirection = Vector2Int.zero;
    }

    /// <summary>
    /// 이 방이 함선 스탯에 기여하는 값을 계산합니다.
    /// 전력 사용량이 포함되며, 손상 시 단계별 기여도 감소 또는 작동 불능 처리됩니다.
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
            contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
        }

        if (currentLevel > 1 && damageCondition == DamageLevel.scratch)
        {
            TeleportRoomData.TeleportRoomLevel
                weakedRoomLevelData = roomData.GetTypedRoomData(currentLevel - 1);
            contributions[ShipStat.PowerUsing] = weakedRoomLevelData.powerRequirement;
        }

        return contributions;
    }

    /// <summary>
    /// 텔레포트실이 피해를 받을 때 호출됩니다.
    /// 이펙트를 갱신합니다.
    /// </summary>
    /// <param name="damage">받은 피해량.</param>
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        UpdateEffects();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="crew"></param>
    public override void OnCrewEnter(CrewMember crew)
    {
        base.OnCrewEnter(crew);

        // 0. 일단 텔포 방이 본인 모선인지 확인
        if (!crew.IsOwnShip())
            return;

        // TP 방 입장 기록
        crew.hasEnteredTPRoom = true;
    }
}
