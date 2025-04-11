using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Trading Item Data", menuName = "Item/ItemData")]
public class TradingItemData : ScriptableObject
{
    public int id;
    public ItemPlanet planet;
    public ItemTierLevel tier;
    public string itemName;
    public string debugName;
    public ItemCategory type;
    public float temperatureMin;
    public float temperatureMax;
    public int shape;
    public int costBase;
    public int capacity;
    public int costMin;
    public float costChangerate;
    public int costMax;
    public string description;
    public Sprite itemSprite;
}
