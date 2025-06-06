﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/*
 * NOTE: 생활 시설 Data는 다른 시설과 다르게 Initialize 에 모든 정보가 쓰여있지 않다.
 *       생활 시설은 레벨로 분류되는 것이 아니라 종류가 다르기 때문에 그렇다.
 *       즉, 이 클래스를 수정할 경우 무조건 무조건 생성된 Scriptable Object 에 값을 할당할 것!!!!
 *       값이 날라갔을 때를 대비하여 기본값을 하단에 명기함.
 *
 *  오락실 : roomName = "room.lifesupport.game", size = (3,2), cost = 1200, powerRequirement = 20, crewMoraleBonus = 3
 *  수면실 : roomName = "room.lifesupport.sleep", size = (2,2), cost = 1200, crewMoraleBonus = 1
 *  사우나 : roomName = "room.lifesupport.sauna", size = (3,2), cost = 1500, crewMoraleBonus = 1
 *  영화관 : roomName = "room.lifesupport.theater",  size = (4,3), cost = 2000, powerRequirement = 20, crewMoraleBonus = 4
 *
 */

/// <summary>
/// 생활 시설의 데이터를 저장하는 ScriptableObject.
/// 종류별로 선원 사기 보너스를 제공하며, 일부는 전력을 소비합니다.
/// </summary>
[CreateAssetMenu(fileName = "LifeSupportRoomData", menuName = "RoomData/LifeSupportRoom Data")]
public class LifeSupportRoomData : RoomData<LifeSupportRoomData.LifeSupportRoomLevel>
{
    /// <summary>
    /// 생활 시설의 개별 종류 데이터를 정의하는 클래스.
    /// </summary>
    [System.Serializable]
    public class LifeSupportRoomLevel : RoomLevel
    {
        public int crewMoraleBonus;
    }

    /// <summary>
    /// 기본 생활 시설 데이터를 초기화합니다.
    /// (※ 최소값만 설정되며, 실제 값은 ScriptableObject 인스펙터에서 수동 입력 필요)
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<LifeSupportRoomLevel>
        {
            new()
            {
                roomType = RoomType.LifeSupport,
                category = RoomCategory.Living,
                level = 1,
                hitPoint = 100
            }
        };
    }
}
