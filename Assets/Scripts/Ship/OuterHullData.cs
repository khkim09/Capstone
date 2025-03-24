using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


/// <summary>
/// 외갑판의 데이터를 담는 ScriptableObject
/// </summary>
public class OuterHullData : ScriptableObject
{
    public class OuterHullLevel
    {
        public string outerHullName;
        public int level;
        public int costPerSurface;
        public float damageReduction;
    }

    protected List<OuterHullLevel> OuterHullLevels = new();

    public OuterHullLevel GetOuterHullData(int level)
    {
        return OuterHullLevels[level];
    }

    private void InitializeDefaultLevels()
    {
        OuterHullLevels = new List<OuterHullLevel>
        {
            new() { outerHullName = "outerhull.level1", level = 1, costPerSurface = 5, damageReduction = 0 },
            new() { outerHullName = "outerhull.level2", level = 1, costPerSurface = 10, damageReduction = 5 },
            new() { outerHullName = "outerhull.level3", level = 1, costPerSurface = 20, damageReduction = 10 }
        };
    }

    protected virtual void OnEnable()
    {
        InitializeDefaultLevels();
    }
}
