using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("id", "planet", "tier", "itemName", "debugName", "type", "temperatureMin", "temperatureMax", "shape", "costBase", "capacity", "costMin", "costChangerate", "costMax", "description", "itemSprite", "amount", "boughtCost")]
	public class ES3UserType_TradingItemData : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_TradingItemData() : base(typeof(TradingItemData)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (TradingItemData)obj;
			
			writer.WriteProperty("id", instance.id, ES3Type_int.Instance);
			writer.WriteProperty("planet", instance.planet, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(ItemPlanet)));
			writer.WriteProperty("tier", instance.tier, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(ItemTierLevel)));
			writer.WriteProperty("itemName", instance.itemName, ES3Type_string.Instance);
			writer.WriteProperty("debugName", instance.debugName, ES3Type_string.Instance);
			writer.WriteProperty("type", instance.type, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(ItemCategory)));
			writer.WriteProperty("temperatureMin", instance.temperatureMin, ES3Type_float.Instance);
			writer.WriteProperty("temperatureMax", instance.temperatureMax, ES3Type_float.Instance);
			writer.WriteProperty("shape", instance.shape, ES3Type_int.Instance);
			writer.WriteProperty("costBase", instance.costBase, ES3Type_int.Instance);
			writer.WriteProperty("capacity", instance.capacity, ES3Type_int.Instance);
			writer.WriteProperty("costMin", instance.costMin, ES3Type_int.Instance);
			writer.WriteProperty("costChangerate", instance.costChangerate, ES3Type_float.Instance);
			writer.WriteProperty("costMax", instance.costMax, ES3Type_int.Instance);
			writer.WriteProperty("description", instance.description, ES3Type_string.Instance);
			writer.WritePropertyByRef("itemSprite", instance.itemSprite);
			writer.WriteProperty("amount", instance.amount, ES3Type_int.Instance);
			writer.WriteProperty("boughtCost", instance.boughtCost, ES3Type_int.Instance);
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (TradingItemData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "id":
						instance.id = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "planet":
						instance.planet = reader.Read<ItemPlanet>();
						break;
					case "tier":
						instance.tier = reader.Read<ItemTierLevel>();
						break;
					case "itemName":
						instance.itemName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "debugName":
						instance.debugName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "type":
						instance.type = reader.Read<ItemCategory>();
						break;
					case "temperatureMin":
						instance.temperatureMin = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "temperatureMax":
						instance.temperatureMax = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "shape":
						instance.shape = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "costBase":
						instance.costBase = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "capacity":
						instance.capacity = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "costMin":
						instance.costMin = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "costChangerate":
						instance.costChangerate = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "costMax":
						instance.costMax = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "description":
						instance.description = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "itemSprite":
						instance.itemSprite = reader.Read<UnityEngine.Sprite>(ES3Type_Sprite.Instance);
						break;
					case "amount":
						instance.amount = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "boughtCost":
						instance.boughtCost = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_TradingItemDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_TradingItemDataArray() : base(typeof(TradingItemData[]), ES3UserType_TradingItemData.Instance)
		{
			Instance = this;
		}
	}
}