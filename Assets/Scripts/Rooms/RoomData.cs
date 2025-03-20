using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


/// <summary>
/// 모든 방 유형의 기본 데이터를 정의하는 추상 ScriptableObject
/// </summary>
public abstract class RoomData : ScriptableObject
{
    public class RoomLevel
    {
        public int level;
        public int hitPoint;
        public Vector2Int size;
        public int cost;
        public int crewRequirement;
        public float powerRequirement;
        public Dictionary<RoomDamageLevel, float> damageHitPointRate = new();
    }

    // 추상 메서드로 RoomLevel 데이터 가져오기
    public abstract RoomLevel GetRoomData(int level);
}

public enum RoomDamageLevel
{
    DamageLevelOne,
    DamageLevelTwo
}

// 제네릭 확장 버전
public abstract class RoomData<T> : RoomData where T : RoomData.RoomLevel
{
    [SerializeField] protected List<T> RoomLevels = new();

    public override RoomLevel GetRoomData(int level)
    {
        return GetTypedRoomData(level);
    }

    public T GetTypedRoomData(int level)
    {
        if (level <= 0 || level > RoomLevels.Count)
            return null;
        return RoomLevels[level - 1];
    }

    protected abstract void InitializeDefaultLevels();

    protected virtual void OnEnable()
    {
        InitializeDefaultLevels();
    }
}
