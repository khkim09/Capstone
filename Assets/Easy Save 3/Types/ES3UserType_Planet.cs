using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("planetData", "planetSprites", "currentSprite", "planetButton", "tooltipPrefab")]
	public class ES3UserType_Planet : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Planet() : base(typeof(Planet)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (Planet)obj;
			
			writer.WritePrivateField("planetData", instance);
			writer.WritePrivateField("planetSprites", instance);
			writer.WritePrivateFieldByRef("currentSprite", instance);
			writer.WritePrivateFieldByRef("planetButton", instance);
			writer.WritePrivateFieldByRef("tooltipPrefab", instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (Planet)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "planetData":
					instance = (Planet)reader.SetPrivateField("planetData", reader.Read<PlanetData>(), instance);
					break;
					case "planetSprites":
					instance = (Planet)reader.SetPrivateField("planetSprites", reader.Read<System.Collections.Generic.List<UnityEngine.Sprite>>(), instance);
					break;
					case "currentSprite":
					instance = (Planet)reader.SetPrivateField("currentSprite", reader.Read<UnityEngine.Sprite>(), instance);
					break;
					case "planetButton":
					instance = (Planet)reader.SetPrivateField("planetButton", reader.Read<UnityEngine.UI.Button>(), instance);
					break;
					case "tooltipPrefab":
					instance = (Planet)reader.SetPrivateField("tooltipPrefab", reader.Read<UnityEngine.GameObject>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_PlanetArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_PlanetArray() : base(typeof(Planet[]), ES3UserType_Planet.Instance)
		{
			Instance = this;
		}
	}
}