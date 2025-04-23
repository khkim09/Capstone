using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 선원(CrewBase) 객체의 직렬화 및 역직렬화를 담당하는 유틸리티 클래스
/// CrewDatabase를 사용하여 종족별 기본 정보를 관리합니다.
/// </summary>
public static class CrewSerialization
{
    /// <summary>
    /// 선원 직렬화 데이터 클래스
    /// </summary>
    [Serializable]
    public class CrewSerializationData
    {
        // 기본 정보
        public string crewName; // 선원 이름
        public bool isPlayerControlled; // 플레이어 소속 여부
        public CrewRace race; // 선원 종족

        // 상태 정보
        public float health; // 현재 체력
        public float maxHealth; // 최대 체력
        public float attack; // 공격력
        public float defense; // 방어력
        public float learningSpeed; // 학습 속도
        public bool needsOxygen; // 산소 필요 여부
        public CrewStatus status; // 상태
        public bool isAlive; // 생존 여부

        // 위치 정보
        public Vector2 position; // 현재 위치
        public Vector2 targetPosition; // 목표 위치
        public string currentRoomId; // 현재 방 ID (직렬화/역직렬화용)

        // 스킬 정보
        public Dictionary<SkillType, float> skills; // 스킬 레벨
        public Dictionary<SkillType, float> equipAdditionalSkills; // 장비 추가 스킬

        // 장비 정보
        public int? equippedAssistantId;

        // 이동 관련
        public bool isMoving; // 이동 중 여부
        public float moveSpeed; // 이동 속도
    }

    /// <summary>
    /// 선원을 직렬화하여 데이터 객체로 변환
    /// </summary>
    /// <param name="crew">직렬화할 선원</param>
    /// <returns>직렬화된 선원 데이터</returns>
    public static CrewSerializationData SerializeCrew(CrewBase crew)
    {
        if (crew == null)
            return null;

        CrewSerializationData data = new()
        {
            // 기본 정보
            crewName = crew.crewName,
            isPlayerControlled = crew.isPlayerControlled,
            race = crew.race,

            // 상태 정보
            health = crew.health,
            maxHealth = crew.maxHealth,
            attack = crew.attack,
            defense = crew.defense,
            learningSpeed = crew.learningSpeed,
            needsOxygen = crew.needsOxygen,
            status = crew.status,
            isAlive = crew.isAlive,

            // 위치 정보
            position = crew.position,
            targetPosition = crew.targetPosition,
            currentRoomId = crew.currentRoom?.GetInstanceID().ToString(),

            // 스킬 정보 - 딕셔너리 복사
            skills = new Dictionary<SkillType, float>(crew.skills),
            equipAdditionalSkills = new Dictionary<SkillType, float>(crew.equipAdditionalSkills),

            // 장비 정보
            equippedAssistantId = crew.equippedAssistant?.eqId ?? 0,

            // 이동 관련
            isMoving = crew.isMoving,
            moveSpeed = crew.moveSpeed
        };

        return data;
    }

    /// <summary>
    /// 모든 선원을 직렬화
    /// </summary>
    /// <param name="crews">직렬화할 선원 목록</param>
    /// <returns>직렬화된 선원 데이터 목록</returns>
    public static List<CrewSerializationData> SerializeAllCrews(List<CrewBase> crews)
    {
        List<CrewSerializationData> result = new();

        if (crews == null)
            return result;

        foreach (CrewBase crew in crews)
        {
            CrewSerializationData data = SerializeCrew(crew);
            if (data != null)
                result.Add(data);
        }

        return result;
    }

    /// <summary>
    /// 함선에 탑승 중인 모든 선원 직렬화
    /// </summary>
    /// <param name="ship">선원이 탑승 중인 함선</param>
    /// <returns>직렬화된 선원 데이터 목록</returns>
    public static List<CrewSerializationData> SerializeShipCrews(Ship ship)
    {
        if (ship == null)
            return new List<CrewSerializationData>();

        return SerializeAllCrews(ship.GetAllCrew());
    }

    /// <summary>
    /// 직렬화된 선원 데이터로 선원 객체 생성
    /// </summary>
    /// <param name="data">직렬화된 선원 데이터</param>
    /// <returns>생성된 선원 객체</returns>
    public static CrewBase DeserializeCrew(CrewSerializationData data)
    {
        if (data == null)
            return null;

        // 선원 매니저를 통해 선원 생성
        CrewBase crew =
            GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(data.race, data.crewName,
                data.isPlayerControlled);

        if (crew != null)
        {
            // 상태 정보 설정
            crew.health = data.health;
            crew.maxHealth = data.maxHealth;
            crew.attack = data.attack;
            crew.defense = data.defense;
            crew.learningSpeed = data.learningSpeed;
            crew.needsOxygen = data.needsOxygen;
            crew.status = data.status;
            crew.isAlive = data.isAlive;

            // 위치 정보 설정
            crew.position = data.position;
            crew.targetPosition = data.targetPosition;
            // 방 참조는 함선 로드 후에 별도로 설정해야 함

            // 스킬 정보 설정
            crew.skills = new Dictionary<SkillType, float>(data.skills);
            crew.equipAdditionalSkills = new Dictionary<SkillType, float>(data.equipAdditionalSkills);

            // 이동 관련 설정
            crew.isMoving = data.isMoving;
            crew.moveSpeed = data.moveSpeed;

            if (data.equippedAssistantId != 0)
            {
                EquipmentItem assistant =
                    EquipmentManager.Instance.GetEquipmentByTypeAndId(EquipmentType.AssistantEquipment,
                        data.equippedAssistantId.Value);
                if (assistant != null && assistant.eqType == EquipmentType.AssistantEquipment)
                {
                    crew.equippedAssistant = assistant;
                    crew.RecalculateEquipmentBonus(assistant, true);
                }
            }
        }

        return crew;
    }

    /// <summary>
    /// 함선에 모든 선원 복원
    /// </summary>
    /// <param name="crewDataList">직렬화된 선원 데이터 목록</param>
    /// <param name="ship">대상 함선</param>
    /// <returns>복원된 선원 수</returns>
    public static int DeserializeAllCrews(List<CrewSerializationData> crewDataList, Ship ship)
    {
        if (crewDataList == null || ship == null)
            return 0;

        int restoredCount = 0;

        // 기존 선원 제거
        ship.RemoveAllCrews();

        // 참조 매핑을 위한 딕셔너리
        Dictionary<string, Room> roomsById = new();
        foreach (Room room in ship.GetAllRooms()) roomsById[room.GetInstanceID().ToString()] = room;

        // 선원 복원
        foreach (CrewSerializationData data in crewDataList)
        {
            CrewBase crew = DeserializeCrew(data);
            if (crew != null)
            {
                // 함선에 선원 추가
                ship.AddCrew(crew);

                // 방 참조 복원
                if (!string.IsNullOrEmpty(data.currentRoomId) &&
                    roomsById.TryGetValue(data.currentRoomId, out Room room))
                {
                    crew.currentRoom = room;
                    room.OnCrewEnter(crew);
                }


                restoredCount++;
            }
        }

        return restoredCount;
    }

    /// <summary>
    /// 선원 데이터를 JSON 문자열로 변환
    /// </summary>
    /// <param name="data">직렬화할 선원 데이터</param>
    /// <returns>JSON 문자열</returns>
    public static string ToJson(CrewSerializationData data)
    {
        if (data == null)
            return "{}";

        // Vector2를 직렬화할 때 오류나는 것 때문에 추가함
        JsonSerializerSettings settings = new() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        return JsonConvert.SerializeObject(data, Formatting.Indented, settings);
    }

    /// <summary>
    /// 선원 데이터 목록을 JSON 문자열로 변환
    /// </summary>
    /// <param name="dataList">직렬화할 선원 데이터 목록</param>
    /// <returns>JSON 문자열</returns>
    public static string ToJson(List<CrewSerializationData> dataList)
    {
        if (dataList == null)
            return "[]";

        // Vector2를 직렬화할 때 오류나는 것 때문에 추가함
        JsonSerializerSettings settings = new() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        return JsonConvert.SerializeObject(dataList, Formatting.Indented, settings);
    }

    /// <summary>
    /// JSON 문자열에서 선원 데이터 복원
    /// </summary>
    /// <param name="json">JSON 문자열</param>
    /// <returns>복원된 선원 데이터</returns>
    public static CrewSerializationData FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonConvert.DeserializeObject<CrewSerializationData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"선원 데이터 역직렬화 오류: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// JSON 문자열에서 선원 데이터 목록 복원
    /// </summary>
    /// <param name="json">JSON 문자열</param>
    /// <returns>복원된 선원 데이터 목록</returns>
    public static List<CrewSerializationData> FromJsonList(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<CrewSerializationData>();

        try
        {
            return JsonConvert.DeserializeObject<List<CrewSerializationData>>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"선원 데이터 목록 역직렬화 오류: {e.Message}");
            return new List<CrewSerializationData>();
        }
    }
}
