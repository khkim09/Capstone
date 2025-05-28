using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("planetId", "planetName", "planetRace", "itemPlanet", "normalizedPosition", "itemsTier1", "itemsTier2", "itemsTier3", "currentRevenue", "currentFuelPrice", "currentMissilePrice", "currentHypersonicPrice", "questList", "activeEffects", "categoryPriceModifiers", "itemPriceDictionary", "currentRandomEquipmentItem", "isHome")]
	public class ES3UserType_PlanetData : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_PlanetData() : base(typeof(PlanetData)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (PlanetData)obj;
			
			writer.WriteProperty("planetId", instance.planetId, ES3Type_int.Instance);
			writer.WriteProperty("planetName", instance.planetName, ES3Type_string.Instance);
			writer.WriteProperty("planetRace", instance.planetRace, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(PlanetRace)));
			writer.WriteProperty("itemPlanet", instance.itemPlanet, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(ItemPlanet)));
			writer.WriteProperty("normalizedPosition", instance.normalizedPosition, ES3Type_Vector2.Instance);
			writer.WriteProperty("itemsTier1", instance.itemsTier1, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<TradingItemData>)));
			writer.WriteProperty("itemsTier2", instance.itemsTier2, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<TradingItemData>)));
			writer.WriteProperty("itemsTier3", instance.itemsTier3, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<TradingItemData>)));
			writer.WriteProperty("currentRevenue", instance.currentRevenue, ES3Type_int.Instance);
			writer.WriteProperty("currentFuelPrice", instance.currentFuelPrice, ES3Type_int.Instance);
			writer.WriteProperty("currentMissilePrice", instance.currentMissilePrice, ES3Type_int.Instance);
			writer.WriteProperty("currentHypersonicPrice", instance.currentHypersonicPrice, ES3Type_int.Instance);
			writer.WriteProperty("questList", instance.questList, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<RandomQuest>)));
			writer.WriteProperty("activeEffects", instance.activeEffects, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<PlanetEffect>)));
			writer.WriteProperty("categoryPriceModifiers", instance.categoryPriceModifiers, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.Dictionary<ItemCategory, System.Single>)));
			writer.WriteProperty("itemPriceDictionary", instance.itemPriceDictionary, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.Dictionary<System.Int32, System.Int32>)));
			writer.WritePropertyByRef("currentRandomEquipmentItem", instance.currentRandomEquipmentItem);
			writer.WriteProperty("isHome", instance.isHome, ES3Type_bool.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (PlanetData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "planetId":
						instance.planetId = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "planetName":
						instance.planetName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "planetRace":
						instance.planetRace = reader.Read<PlanetRace>();
						break;
					case "itemPlanet":
						instance.itemPlanet = reader.Read<ItemPlanet>();
						break;
					case "normalizedPosition":
						instance.normalizedPosition = reader.Read<UnityEngine.Vector2>(ES3Type_Vector2.Instance);
						break;
					case "itemsTier1":
						instance.itemsTier1 = reader.Read<System.Collections.Generic.List<TradingItemData>>();
						break;
					case "itemsTier2":
						instance.itemsTier2 = reader.Read<System.Collections.Generic.List<TradingItemData>>();
						break;
					case "itemsTier3":
						instance.itemsTier3 = reader.Read<System.Collections.Generic.List<TradingItemData>>();
						break;
					case "currentRevenue":
						instance.currentRevenue = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "currentFuelPrice":
						instance.currentFuelPrice = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "currentMissilePrice":
						instance.currentMissilePrice = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "currentHypersonicPrice":
						instance.currentHypersonicPrice = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "questList":
						instance.questList = reader.Read<System.Collections.Generic.List<RandomQuest>>();
						break;
					case "activeEffects":
						instance.activeEffects = reader.Read<System.Collections.Generic.List<PlanetEffect>>();
						break;
					case "categoryPriceModifiers":
						instance.categoryPriceModifiers = reader.Read<System.Collections.Generic.Dictionary<ItemCategory, System.Single>>();
						break;
					case "itemPriceDictionary":
						instance.itemPriceDictionary = reader.Read<System.Collections.Generic.Dictionary<System.Int32, System.Int32>>();
						break;
					case "currentRandomEquipmentItem":
						instance.currentRandomEquipmentItem = reader.Read<EquipmentItem>(ES3UserType_EquipmentItem.Instance);
						break;
					case "isHome":
						instance.isHome = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new PlanetData();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_PlanetDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_PlanetDataArray() : base(typeof(PlanetData[]), ES3UserType_PlanetData.Instance)
		{
			Instance = this;
		}
	}
}