using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomDamageRates : ISerializationCallbackReceiver
{
    // 인스펙터에 직접 노출되는 값
    [SerializeField] public float damageLevelOne = 50f;
    [SerializeField] public float damageLevelTwo = 10f;

    // 내부 리스트 – 실제 저장용
    [SerializeField] private List<DamageHitPointPair> pairs = new();

    // 인덱서 – 딕셔너리처럼 사용
    public float this[RoomDamageLevel level]
    {
        get
        {
            return level switch
            {
                RoomDamageLevel.DamageLevelOne => damageLevelOne,
                RoomDamageLevel.DamageLevelTwo => damageLevelTwo,
                _ => GetFromList(level)
            };
        }
        set
        {
            switch (level)
            {
                case RoomDamageLevel.DamageLevelOne:
                    damageLevelOne = value;
                    break;
                case RoomDamageLevel.DamageLevelTwo:
                    damageLevelTwo = value;
                    break;
                default:
                    SetToList(level, value);
                    break;
            }
        }
    }

    // 리스트에서 읽기
    private float GetFromList(RoomDamageLevel level)
    {
        foreach (DamageHitPointPair pair in pairs)
            if (pair.damageLevel == level)
                return pair.hitPointRate;
        return 0f;
    }

    // 리스트에 저장
    private void SetToList(RoomDamageLevel level, float value)
    {
        foreach (DamageHitPointPair pair in pairs)
            if (pair.damageLevel == level)
            {
                pair.hitPointRate = value;
                return;
            }

        pairs.Add(new DamageHitPointPair(level, value));
    }

    // Unity 직렬화 전에 호출됨 → 리스트 갱신
    public void OnBeforeSerialize()
    {
        SetToList(RoomDamageLevel.DamageLevelOne, damageLevelOne);
        SetToList(RoomDamageLevel.DamageLevelTwo, damageLevelTwo);
    }

    // Unity 직렬화 후에 호출됨 → 필드 갱신
    public void OnAfterDeserialize()
    {
        damageLevelOne = GetFromList(RoomDamageLevel.DamageLevelOne);
        damageLevelTwo = GetFromList(RoomDamageLevel.DamageLevelTwo);
    }

    // 딕셔너리 변환 (선택)
    public Dictionary<RoomDamageLevel, float> ToDictionary()
    {
        Dictionary<RoomDamageLevel, float> dict = new();
        dict[RoomDamageLevel.DamageLevelOne] = damageLevelOne;
        dict[RoomDamageLevel.DamageLevelTwo] = damageLevelTwo;
        foreach (DamageHitPointPair pair in pairs)
            if (!dict.ContainsKey(pair.damageLevel))
                dict[pair.damageLevel] = pair.hitPointRate;
        return dict;
    }

    public static RoomDamageRates Create(float level1Rate, float level2Rate)
    {
        RoomDamageRates result = new();
        result[RoomDamageLevel.DamageLevelOne] = level1Rate;
        result[RoomDamageLevel.DamageLevelTwo] = level2Rate;
        return result;
    }
}

[Serializable]
public class DamageHitPointPair
{
    public RoomDamageLevel damageLevel;
    public float hitPointRate;

    public DamageHitPointPair(RoomDamageLevel damageLevel, float hitPointRate)
    {
        this.damageLevel = damageLevel;
        this.hitPointRate = hitPointRate;
    }
}
