using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("outerHullName", "level", "costPerSurface", "damageReduction")]
	public class ES3UserType_OuterHullLevel : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_OuterHullLevel() : base(typeof(OuterHullData.OuterHullLevel)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (OuterHullData.OuterHullLevel)obj;
			
			writer.WriteProperty("outerHullName", instance.outerHullName, ES3Type_string.Instance);
			writer.WriteProperty("level", instance.level, ES3Type_int.Instance);
			writer.WriteProperty("costPerSurface", instance.costPerSurface, ES3Type_int.Instance);
			writer.WriteProperty("damageReduction", instance.damageReduction, ES3Type_float.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (OuterHullData.OuterHullLevel)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "outerHullName":
						instance.outerHullName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "level":
						instance.level = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "costPerSurface":
						instance.costPerSurface = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "damageReduction":
						instance.damageReduction = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new OuterHullData.OuterHullLevel();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_OuterHullLevelArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_OuterHullLevelArray() : base(typeof(OuterHullData.OuterHullLevel[]), ES3UserType_OuterHullLevel.Instance)
		{
			Instance = this;
		}
	}
}