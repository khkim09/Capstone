using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/*
 * NOTE: 선원 선실 Data는 다른 시설과 다르게 Initialize 에 모든 정보가 쓰여있지 않다.
 *       선실은 레벨로 분류되는 것이 아니라 종류가 다르기 때문에 그렇다.
 *       즉, 이 클래스를 수정할 경우 무조건 무조건 생성된 Scriptable Object 에 값을 할당할 것!!!!
 *       값이 날라갔을 때를 대비하여 기본값을 하단에 명기함.
 *
 *  생활관 : roomName = "room.crewquarters.basic", maxCrewCapacity = 6, size = (2,4), cost = 1200
 *  큰 생활관 : roomName = "room.crewquarters.big", maxCrewCapacity = 10, size = (3,4), cost = 2000
 *  개인 생활관 : roomName = "room.crewquarters.personal", maxCrewCapcity = 1, size = (2,1), cost = 500, crewMoraleBonus = 1
 *  호화 생활관 : roomName = "room.crewquarters.goodpersonal", maxCrewCapacity = 1, size = (2,1), cost = 700, powerRequirement = 5, crewMoraleBonus = 3
 *
 */

/// <summary>
/// 승무원 선실의 데이터를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "CrewQuartersRoomData", menuName = "RoomData/CrewQuartersRoom Data")]
public class CrewQuartersRoomData : RoomData<CrewQuartersRoomData.CrewQuartersRoomLevel>
{
    [System.Serializable]
    public class CrewQuartersRoomLevel : RoomLevel
    {
        public int maxCrewCapacity;
        public int crewMoraleBonus;
    }

    /// <summary>
    /// 기본 승무원 선실 데이터 초기화
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<CrewQuartersRoomLevel> { new() { level = 1, hitPoint = 100 } };
    }
}
