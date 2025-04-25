using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 방 관련 데이터를 관리하는 중앙 데이터베이스
/// </summary>
[CreateAssetMenu(fileName = "RoomDatabase", menuName = "RoomData/RoomDatabase")]
public class RoomDatabase : ScriptableObject
{
    // 방 타입별 RoomData 목록
    [SerializeField] private List<RoomData> allRoomData = new();

    // 런타임용 방 타입별 RoomData 딕셔너리
    private Dictionary<RoomType, RoomData> roomDataByType;

    /// <summary>
    /// 방 타입별 RoomData dictionary 제공
    /// </summary>
    public Dictionary<RoomType, RoomData> ByType
    {
        get
        {
            if (roomDataByType == null || roomDataByType.Count == 0) InitializeDictionary();
            return roomDataByType;
        }
    }

    /// <summary>
    /// 데이터베이스 초기화
    /// </summary>
    public void InitializeDictionary()
    {
        roomDataByType = new Dictionary<RoomType, RoomData>();
        foreach (RoomData data in allRoomData) roomDataByType[data.GetRoomType()] = data;

        Debug.Log($"RoomDatabase 초기화 완료: {roomDataByType.Count}개 방 정보 로드");
    }

    /// <summary>
    /// Unity 이벤트: ScriptableObject가 로드될 때 호출됨
    /// </summary>
    private void OnEnable()
    {
        InitializeDictionary();
    }

    /// <summary>
    /// 특정 방 타입의 RoomData 반환
    /// </summary>
    /// <param name="roomType">방 타입</param>
    /// <returns>RoomData</returns>
    public RoomData GetRoomData(RoomType roomType)
    {
        if (roomDataByType == null || roomDataByType.Count == 0) InitializeDictionary();

        if (roomDataByType.TryGetValue(roomType, out RoomData data)) return data;

        Debug.LogWarning($"방 타입 {roomType}에 대한 데이터를 찾을 수 없습니다.");
        return null;
    }

    /// <summary>
    /// 특정 방 타입의 RoomData를 제네릭 타입으로 반환
    /// </summary>
    /// <typeparam name="T">RoomData 상속 타입</typeparam>
    /// <param name="roomType">방 타입</param>
    /// <returns>요청한 타입의 RoomData</returns>
    public T GetTypedRoomData<T>(RoomType roomType) where T : RoomData
    {
        RoomData data = GetRoomData(roomType);
        if (data is T typedData) return typedData;

        Debug.LogWarning($"방 타입 {roomType}에 대한 {typeof(T).Name} 데이터를 찾을 수 없습니다.");
        return null;
    }

    /// <summary>
    /// 모든 방 데이터 목록을 반환
    /// </summary>
    /// <returns>모든 방 데이터 목록</returns>
    public List<RoomData> GetAllRoomData()
    {
        return new List<RoomData>(allRoomData);
    }
}
