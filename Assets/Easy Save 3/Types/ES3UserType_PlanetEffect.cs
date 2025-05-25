using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("categoryType", "changeAmount", "startYear", "parentEventName")]
	public class ES3UserType_PlanetEffect : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_PlanetEffect() : base(typeof(PlanetEffect)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (PlanetEffect)obj;
			
			writer.WriteProperty("categoryType", instance.categoryType, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(ItemCategory)));
			writer.WriteProperty("changeAmount", instance.changeAmount, ES3Type_float.Instance);
			writer.WriteProperty("startYear", instance.startYear, ES3Type_int.Instance);
			writer.WriteProperty("parentEventName", instance.parentEventName, ES3Type_string.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (PlanetEffect)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "categoryType":
						instance.categoryType = reader.Read<ItemCategory>(ES3Type_enum.Instance);
						break;
					case "changeAmount":
						instance.changeAmount = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "startYear":
						instance.startYear = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "parentEventName":
						instance.parentEventName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new PlanetEffect();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_PlanetEffectArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_PlanetEffectArray() : base(typeof(PlanetEffect[]), ES3UserType_PlanetEffect.Instance)
		{
			Instance = this;
		}
	}
}