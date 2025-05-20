using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("damageLevelOne", "damageLevelTwo", "pairs")]
	public class ES3UserType_RoomDamageRates : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_RoomDamageRates() : base(typeof(RoomDamageRates)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (RoomDamageRates)obj;
			
			writer.WriteProperty("damageLevelOne", instance.damageLevelOne, ES3Type_float.Instance);
			writer.WriteProperty("damageLevelTwo", instance.damageLevelTwo, ES3Type_float.Instance);
			writer.WritePrivateField("pairs", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (RoomDamageRates)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "damageLevelOne":
						instance.damageLevelOne = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "damageLevelTwo":
						instance.damageLevelTwo = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "pairs":
					instance = (RoomDamageRates)reader.SetPrivateField("pairs", reader.Read<System.Collections.Generic.List<DamageHitPointPair>>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new RoomDamageRates();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_RoomDamageRatesArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_RoomDamageRatesArray() : base(typeof(RoomDamageRates[]), ES3UserType_RoomDamageRates.Instance)
		{
			Instance = this;
		}
	}
}