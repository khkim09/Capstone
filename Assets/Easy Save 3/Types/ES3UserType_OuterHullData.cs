using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("OuterHullLevels", "level1Sprites", "level2Sprites", "level3Sprites")]
	public class ES3UserType_OuterHullData : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_OuterHullData() : base(typeof(OuterHullData)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (OuterHullData)obj;
			
			writer.WritePrivateField("OuterHullLevels", instance);
			writer.WriteProperty("level1Sprites", instance.level1Sprites, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(UnityEngine.Sprite[])));
			writer.WriteProperty("level2Sprites", instance.level2Sprites, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(UnityEngine.Sprite[])));
			writer.WriteProperty("level3Sprites", instance.level3Sprites, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(UnityEngine.Sprite[])));
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (OuterHullData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "OuterHullLevels":
					instance = (OuterHullData)reader.SetPrivateField("OuterHullLevels", reader.Read<System.Collections.Generic.List<OuterHullData.OuterHullLevel>>(), instance);
					break;
					case "level1Sprites":
						instance.level1Sprites = reader.Read<UnityEngine.Sprite[]>();
						break;
					case "level2Sprites":
						instance.level2Sprites = reader.Read<UnityEngine.Sprite[]>();
						break;
					case "level3Sprites":
						instance.level3Sprites = reader.Read<UnityEngine.Sprite[]>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_OuterHullDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_OuterHullDataArray() : base(typeof(OuterHullData[]), ES3UserType_OuterHullData.Instance)
		{
			Instance = this;
		}
	}
}