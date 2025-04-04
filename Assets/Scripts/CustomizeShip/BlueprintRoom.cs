using UnityEngine;


/// <summary>
/// 설계도에 배치된 방 정보.
/// </summary>
public class BlueprintRoom : MonoBehaviour
{
    /// <summary>RoomData 참조</summary>
    public RoomData roomData;

    /// <summary>선택된 레벨 인덱스 (0~2)</summary>
    public int levelIndex;

    /// <summary>배치 위치</summary>
    public Vector2Int position;

    /// <summary>레벨별 데이터 접근자</summary>
    public RoomData.RoomLevel levelData => roomData.GetRoomData(levelIndex);

    /// <summary>해당 레벨의 설치 비용</summary>
    public int roomCost => levelData.cost;

    /// <summary>해당 레벨의 크기</summary>
    public Vector2Int roomSize => levelData.size;
}
