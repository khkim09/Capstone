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
 *  생활관 : roomName = "room.crewquarters.basic", hitPoint = 100, maxCrewCapacity = 6, size = (4, 2), cost = 1200
 *  큰 생활관 : roomName = "room.crewquarters.big", hitPoint = 160, maxCrewCapacity = 10, size = (4, 3), cost = 2000
 *
 *  숙소들의 피해 단계는 1단계는 존재하지 않고 2단계만 존재한다 (25%)
 */

/// <summary>
/// 승무원 선실의 데이터를 저장하는 ScriptableObject.
/// 선실은 "레벨"이 아닌 "종류"로 구분되며, 종류별로 수용 인원과 사기 보너스 등이 달라집니다.
/// </summary>
[CreateAssetMenu(fileName = "CrewQuartersRoomData", menuName = "RoomData/CrewQuartersRoom Data")]
public class CrewQuartersRoomData : RoomData<CrewQuartersRoomData.CrewQuartersRoomLevel>
{
    /// <summary>
    /// 선실의 종류별 데이터를 정의하는 클래스.
    /// 수용 인원과 선원 사기 보너스를 포함합니다.
    /// </summary>
    [System.Serializable]
    public class CrewQuartersRoomLevel : RoomLevel
    {
        /// <summary>최대 수용 가능한 선원 수.</summary>
        public int maxCrewCapacity;

        /// <summary>선원 사기 보너스.</summary>
        public int crewMoraleBonus;
    }

    /// <summary>
    /// 기본 데이터 초기화. (최소 레벨 1, 체력 100만 설정됨)
    /// 실제 데이터는 ScriptableObject 인스턴스에 수동 입력 필요.
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<CrewQuartersRoomLevel>
        {
            new()
            {
                roomName = "room.crewquarters.basic",
                roomType = RoomType.CrewQuarters,
                category = RoomCategory.Essential,
                level = 1,
                hitPoint = 100,
                size = new Vector2Int(4, 2),
                cost = 1200,
                powerRequirement = 0,
                crewRequirement = 0,
                damageHitPointRate = RoomDamageRates.Create(0f, 25f),
                possibleDoorPositions = new List<DoorPosition>()
                {
                    new(new Vector2Int(3, 0), DoorDirection.East)
                },
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new (0, 1),
                    new (0, 0),
                    new (1, 1),
                    new (1, 0),
                    new (2, 1),
                    new (2, 0),
                    new (3, 1),
                    new (3, 0)
                }
            },
            new()
            {
                roomName = "room.crewquarters.big",
                roomType = RoomType.CrewQuarters,
                category = RoomCategory.Essential,
                level = 1,
                hitPoint = 160,
                size = new Vector2Int(4, 3),
                cost = 2000,
                powerRequirement = 0,
                crewRequirement = 0,
                damageHitPointRate = RoomDamageRates.Create(0f, 40f),
                possibleDoorPositions = new List<DoorPosition>()
                {
                    new(new Vector2Int(3, 1), DoorDirection.East)
                },
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new (0, 2),
                    new (0, 0),
                    new (0, 1),
                    new (1, 2),
                    new (1, 0),
                    new (1, 1),
                    new (2, 2),
                    new (2, 0),
                    new (2, 1),
                    new (3, 2),
                    new (3, 0),
                    new (3, 1)
                }
            }
        };
    }
}
