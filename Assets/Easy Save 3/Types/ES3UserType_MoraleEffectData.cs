using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("targetRace", "value", "duration", "startYear", "source")]
	public class ES3UserType_MoraleEffectData : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_MoraleEffectData() : base(typeof(MoraleEffectData)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (MoraleEffectData)obj;
			
			writer.WriteProperty("targetRace", instance.targetRace, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(CrewRace)));
			writer.WriteProperty("value", instance.value, ES3Type_int.Instance);
			writer.WriteProperty("duration", instance.duration, ES3Type_int.Instance);
			writer.WriteProperty("startYear", instance.startYear, ES3Type_int.Instance);
			writer.WriteProperty("source", instance.source, ES3Type_string.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (MoraleEffectData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "targetRace":
						instance.targetRace = reader.Read<CrewRace>();
						break;
					case "value":
						instance.value = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "duration":
						instance.duration = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "startYear":
						instance.startYear = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "source":
						instance.source = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new MoraleEffectData();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_MoraleEffectDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_MoraleEffectDataArray() : base(typeof(MoraleEffectData[]), ES3UserType_MoraleEffectData.Instance)
		{
			Instance = this;
		}
	}
}