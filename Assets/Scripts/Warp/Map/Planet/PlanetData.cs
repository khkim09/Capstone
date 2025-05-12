using System.Collections.Generic;
using UnityEngine;

public class PlanetData
{
    public string planetName;
    public float distance;

    public List<TradingItem> itemsTier1 = new();
    public List<TradingItem> itemsTier2 = new();
    public List<TradingItem> itemsTier3 = new();

    public int tier2Requirement = 0;
    public int tier3Requirement = 0;

    public int currentTradingAmount = 0;
    public int currentTier = 0;

    [Range(25f, 75f)] public float currentFuelPrice = 0f;

    public RandomQuest currentQuest;
    public RandomEvent currentEvent;
}
