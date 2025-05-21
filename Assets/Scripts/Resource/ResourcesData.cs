using System;
using UnityEngine;

[Serializable]
public class ResourcesData
{
    public ResourceType type;
    public ResourceValueType valueType;

    public float floatAmount;
    public int intAmount;

    public float maxFloatAmount;
    public int maxIntAmount;

    public float GetAmount()
    {
        return valueType == ResourceValueType.Float ? floatAmount : intAmount;
    }

    public void SetAmount(float value)
    {
        if (valueType == ResourceValueType.Float)
            floatAmount = Mathf.Clamp(value, 0, maxFloatAmount);
        else
            intAmount = Mathf.Clamp((int)value, 0, maxIntAmount);
    }
}
