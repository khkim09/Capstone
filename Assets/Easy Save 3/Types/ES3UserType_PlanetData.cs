using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("planetName", "planetRace", "itemPlanet", "normalizedPosition", "itemsTier1", "itemsTier2", "itemsTier3", "currentRevenue", "currentTier", "currentFuelPrice", "currentQuest", "currentEvent")]
	public class ES3UserType_PlanetData : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_PlanetData() : base(typeof(PlanetData)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (PlanetData)obj;
			
			writer.WriteProperty("planetName", instance.planetName, ES3Type_string.Instance);
			writer.WritePrivateField("planetRace", instance);
			writer.WritePrivateField("itemPlanet", instance);
			writer.WriteProperty("normalizedPosition", instance.normalizedPosition, ES3Type_Vector2.Instance);
			writer.WriteProperty("itemsTier1", instance.itemsTier1, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<TradingItemData>)));
			writer.WriteProperty("itemsTier2", instance.itemsTier2, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<TradingItemData>)));
			writer.WriteProperty("itemsTier3", instance.itemsTier3, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<TradingItemData>)));
			writer.WriteProperty("currentRevenue", instance.currentRevenue, ES3Type_int.Instance);
			writer.WriteProperty("currentTier", instance.currentTier, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(ItemTierLevel)));
			writer.WriteProperty("currentFuelPrice", instance.currentFuelPrice, ES3Type_int.Instance);
			writer.WritePropertyByRef("currentQuest", instance.currentQuest);
			writer.WritePropertyByRef("currentEvent", instance.currentEvent);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (PlanetData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "planetName":
						instance.planetName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "planetRace":
					instance = (PlanetData)reader.SetPrivateField("planetRace", reader.Read<PlanetRace>(), instance);
					break;
					case "itemPlanet":
					instance = (PlanetData)reader.SetPrivateField("itemPlanet", reader.Read<ItemPlanet>(), instance);
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
					case "currentTier":
						instance.currentTier = reader.Read<ItemTierLevel>();
						break;
					case "currentFuelPrice":
						instance.currentFuelPrice = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "currentQuest":
						instance.currentQuest = reader.Read<RandomQuest>();
						break;
					case "currentEvent":
						instance.currentEvent = reader.Read<RandomEvent>();
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