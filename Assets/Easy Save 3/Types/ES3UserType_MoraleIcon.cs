using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("race", "totalValue", "iconImage", "effectDataList", "tooltipPrefab", "canvasComponent")]
	public class ES3UserType_MoraleIcon : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_MoraleIcon() : base(typeof(MoraleIcon)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (MoraleIcon)obj;
			
			writer.WriteProperty("race", instance.race, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(CrewRace)));
			writer.WriteProperty("totalValue", instance.totalValue, ES3Type_int.Instance);
			writer.WritePropertyByRef("iconImage", instance.iconImage);
			writer.WritePrivateField("effectDataList", instance);
			writer.WritePrivateFieldByRef("tooltipPrefab", instance);
			writer.WritePrivateFieldByRef("canvasComponent", instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (MoraleIcon)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "race":
						instance.race = reader.Read<CrewRace>();
						break;
					case "totalValue":
						instance.totalValue = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "iconImage":
						instance.iconImage = reader.Read<UnityEngine.UI.Image>(ES3Type_Image.Instance);
						break;
					case "effectDataList":
					instance = (MoraleIcon)reader.SetPrivateField("effectDataList", reader.Read<System.Collections.Generic.List<MoraleEffectData>>(), instance);
					break;
					case "tooltipPrefab":
					instance = (MoraleIcon)reader.SetPrivateField("tooltipPrefab", reader.Read<UnityEngine.GameObject>(), instance);
					break;
					case "canvasComponent":
					instance = (MoraleIcon)reader.SetPrivateField("canvasComponent", reader.Read<UnityEngine.Canvas>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_MoraleIconArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_MoraleIconArray() : base(typeof(MoraleIcon[]), ES3UserType_MoraleIcon.Instance)
		{
			Instance = this;
		}
	}
}