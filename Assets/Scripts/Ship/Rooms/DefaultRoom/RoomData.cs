using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


/// <summary>
/// 모든 방 유형의 기본 데이터를 정의하는 추상 ScriptableObject.
/// 방의 레벨별 속성, 체력, 크기, 요구 자원 등을 설정합니다.
/// </summary>
public abstract class RoomData : ScriptableObject
{
    public class RoomLevel
    {
        /// <summary>레벨 이름 또는 표시용 이름.</summary>
        public string roomName;

        /// <summary>해당 레벨 번호.</summary>
        public int level;

        /// <summary>최대 체력.</summary>
        public int hitPoint;

        /// <summary>방 크기 (격자 단위, 가로x세로).</summary>
        public Vector2Int size;

        /// <summary>업그레이드 비용.</summary>
        public int cost;

        /// <summary> 방 회전 상태 </summary>
        public int rotation;

        /// <summary>작동에 필요한 최소 선원 수.</summary>
        public int crewRequirement;

        /// <summary>요구 전력량.</summary>
        public float powerRequirement;

        /// <summary>피해 단계별 체력 비율 (예: 연기, 화재 등).</summary>
        public Dictionary<RoomDamageLevel, float> damageHitPointRate = new();

        /// <summary>해당 레벨의 방 스프라이트.</summary>
        public Sprite roomSprite;
        // TODO: 스프라이트 완성되면 각 Scriptable Object 에 스프라이트 추가할 것, rotation, roomPrefab(실제 배치될 방 prefab), previewPrefab(roomPrefab에서 alpha값만 0.5)
    }

    /// <summary>
    /// 주어진 레벨에 해당하는 RoomLevel 데이터를 반환합니다.
    /// 각 파생 클래스에서 구현해야 합니다.
    /// </summary>
    /// <param name="level">조회할 레벨 번호.</param>
    /// <returns>RoomLevel 데이터.</returns>
    public abstract RoomLevel GetRoomData(int level);

    /// <summary>
    /// 기본 레벨 데이터를 초기화합니다.
    /// 파생 클래스에서 구현합니다.
    /// </summary>
    protected abstract void InitializeDefaultLevels();

    /// <summary>
    /// ScriptableObject가 활성화될 때 호출되며, 필요한 경우 기본 데이터를 초기화합니다.
    /// </summary>
    protected virtual void OnEnable()
    {
        bool needsInitialization = CheckIfNeedsInitialization();
        if (needsInitialization) InitializeDefaultLevels();
    }

    /// <summary>
    /// 데이터 초기화가 필요한지 판단하는 메서드입니다.
    /// 파생 클래스에서 구현합니다.
    /// </summary>
    /// <returns>초기화 필요 여부.</returns>
    protected abstract bool CheckIfNeedsInitialization();
}

/// <summary>
/// 방의 피해 단계 수준을 나타내는 열거형입니다.
/// </summary>
public enum RoomDamageLevel
{
    /// <summary>1단계 피해 (경미한 손상 등).</summary>
    DamageLevelOne,

    /// <summary>2단계 피해 (심각한 손상 등).</summary>
    DamageLevelTwo
}

/// <summary>
/// RoomData의 제네릭 확장 버전.
/// 각 방 타입에 특화된 RoomLevel 데이터를 제네릭으로 정의할 수 있습니다.
/// </summary>
/// <typeparam name="T">RoomLevel을 상속한 특화된 타입.</typeparam>
public abstract class RoomData<T> : RoomData where T : RoomData.RoomLevel
{
    /// <summary>레벨별 방 데이터 리스트.</summary>
    [SerializeField] protected List<T> RoomLevels = new();

    /// <summary>
    /// 주어진 레벨에 해당하는 RoomLevel 데이터를 반환합니다.
    /// </summary>
    /// <param name="level">조회할 레벨 번호.</param>
    /// <returns>RoomLevel 데이터.</returns>
    public override RoomLevel GetRoomData(int level)
    {
        return GetTypedRoomData(level);
    }

    /// <summary>
    /// 제네릭 타입으로 명확하게 캐스팅된 RoomLevel 데이터를 반환합니다.
    /// </summary>
    /// <param name="level">조회할 레벨 번호.</param>
    /// <returns>T 타입의 RoomLevel 데이터.</returns>
    public T GetTypedRoomData(int level)
    {
        if (level <= 0 || level > RoomLevels.Count)
            return null;
        return RoomLevels[level - 1];
    }

    /// <summary>
    /// RoomLevels 리스트가 비어 있거나 null인지 확인하여 초기화가 필요한지 판단합니다.
    /// </summary>
    /// <returns>초기화 필요 여부.</returns>
    protected override bool CheckIfNeedsInitialization()
    {
        // RoomLevels가 비어있거나 null이면 초기화 필요
        return RoomLevels == null || RoomLevels.Count == 0;
    }
}
