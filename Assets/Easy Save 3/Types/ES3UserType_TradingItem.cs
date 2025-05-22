using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("itemData", "boxRenderer", "itemRenderer", "frameRenderer", "frameOffsetY", "rotation", "boxSprites", "boxGrid", "parentStorage", "gridPosition", "cachedPrice", "priceInitialized")]
	public class ES3UserType_TradingItem : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_TradingItem() : base(typeof(TradingItem)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (TradingItem)obj;
			
			writer.WritePrivateFieldByRef("itemData", instance);
			writer.WritePrivateFieldByRef("boxRenderer", instance);
			writer.WritePrivateFieldByRef("itemRenderer", instance);
			writer.WritePrivateFieldByRef("frameRenderer", instance);
			writer.WritePrivateField("frameOffsetY", instance);
			writer.WriteProperty("rotation", instance.rotation, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(Constants.Rotations.Rotation)));
			writer.WritePrivateField("boxSprites", instance);
			writer.WriteProperty("boxGrid", instance.boxGrid, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Boolean[][])));
			writer.WritePropertyByRef("parentStorage", instance.parentStorage);
			writer.WriteProperty("gridPosition", instance.gridPosition, ES3Type_Vector2Int.Instance);
			writer.WritePrivateField("cachedPrice", instance);
			writer.WritePrivateField("priceInitialized", instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (TradingItem)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "itemData":
					instance = (TradingItem)reader.SetPrivateField("itemData", reader.Read<TradingItemData>(), instance);
					break;
					case "boxRenderer":
					instance = (TradingItem)reader.SetPrivateField("boxRenderer", reader.Read<UnityEngine.SpriteRenderer>(), instance);
					break;
					case "itemRenderer":
					instance = (TradingItem)reader.SetPrivateField("itemRenderer", reader.Read<UnityEngine.SpriteRenderer>(), instance);
					break;
					case "frameRenderer":
					instance = (TradingItem)reader.SetPrivateField("frameRenderer", reader.Read<UnityEngine.SpriteRenderer>(), instance);
					break;
					case "frameOffsetY":
					instance = (TradingItem)reader.SetPrivateField("frameOffsetY", reader.Read<System.Single>(), instance);
					break;
					case "rotation":
						instance.rotation = reader.Read<Constants.Rotations.Rotation>();
						break;
					case "boxSprites":
					instance = (TradingItem)reader.SetPrivateField("boxSprites", reader.Read<UnityEngine.Sprite[]>(), instance);
					break;
					case "boxGrid":
						instance.boxGrid = reader.Read<System.Boolean[][]>();
						break;
					case "parentStorage":
						instance.parentStorage = reader.Read<StorageRoomBase>();
						break;
					case "gridPosition":
						instance.gridPosition = reader.Read<UnityEngine.Vector2Int>(ES3Type_Vector2Int.Instance);
						break;
					case "cachedPrice":
					instance = (TradingItem)reader.SetPrivateField("cachedPrice", reader.Read<System.Nullable<System.Single>>(), instance);
					break;
					case "priceInitialized":
					instance = (TradingItem)reader.SetPrivateField("priceInitialized", reader.Read<System.Boolean>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_TradingItemArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_TradingItemArray() : base(typeof(TradingItem[]), ES3UserType_TradingItem.Instance)
		{
			Instance = this;
		}
	}
}