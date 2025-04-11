using System.Collections.Generic;
using UnityEngine;

/*
 * NOTE: 창고 Data는 다른 시설과 다르게 Initialize 에 모든 정보가 쓰여있지 않다.
 *       창고는 레벨로 분류되는 것이 아니라 종류가 다르기 때문에 그렇다.
 *       즉, 이 클래스를 수정할 경우 무조건 무조건 생성된 Scriptable Object 에 값을 할당할 것!!!!
 */

/// <summary>
/// 모든 창고 타입의 기본 데이터를 저장하는 ScriptableObject.
/// 창고는 레벨이 아닌 종류로 구분되며, ScriptableObject 인스턴스에 직접 데이터 입력이 필요합니다.
/// </summary>
[CreateAssetMenu(fileName = "StorageRoomData", menuName = "RoomData/StorageRoom Data")]
public class StorageRoomBaseData : RoomData<StorageRoomBaseData.StorageRoomBaseLevel>
{
    public StorageType storageType;

    /// <summary>
    /// 공통 창고 데이터 구조.
    /// 종류별 저장 용량 등을 정의합니다.
    /// </summary>
    [System.Serializable]
    public class StorageRoomBaseLevel : RoomLevel
    {
        // 모든 창고의 공통적인 속성들
        public int storageCapacity; // 기본 저장 용량
    }

    /// <summary>
    /// 기본 창고 데이터를 초기화합니다. (테스트 목적의 최소값만 설정)
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels =
            new List<StorageRoomBaseLevel> { new() { roomType = RoomType.Storage, level = 1, hitPoint = 100 } };
    }

    public StorageType GetStorageType()
    {
        return storageType;
    }
}
