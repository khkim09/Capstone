using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("skillType", "maxValue")]
	public class ES3UserType_SkillValue : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_SkillValue() : base(typeof(CrewRaceStat.SkillValue)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (CrewRaceStat.SkillValue)obj;
			
			writer.WriteProperty("skillType", instance.skillType, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(SkillType)));
			writer.WriteProperty("maxValue", instance.maxValue, ES3Type_float.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (CrewRaceStat.SkillValue)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "skillType":
						instance.skillType = reader.Read<SkillType>(ES3Type_enum.Instance);
						break;
					case "maxValue":
						instance.maxValue = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new CrewRaceStat.SkillValue();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_SkillValueArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_SkillValueArray() : base(typeof(CrewRaceStat.SkillValue[]), ES3UserType_SkillValue.Instance)
		{
			Instance = this;
		}
	}
}