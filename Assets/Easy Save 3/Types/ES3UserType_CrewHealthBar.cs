using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("healthBarFill", "healthBarCanvas", "offset", "target")]
	public class ES3UserType_CrewHealthBar : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_CrewHealthBar() : base(typeof(CrewHealthBar)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (CrewHealthBar)obj;
			
			writer.WritePrivateFieldByRef("healthBarFill", instance);
			writer.WritePrivateFieldByRef("healthBarCanvas", instance);
			writer.WritePrivateField("offset", instance);
			writer.WritePrivateFieldByRef("target", instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (CrewHealthBar)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "healthBarFill":
					instance = (CrewHealthBar)reader.SetPrivateField("healthBarFill", reader.Read<UnityEngine.UI.Image>(), instance);
					break;
					case "healthBarCanvas":
					instance = (CrewHealthBar)reader.SetPrivateField("healthBarCanvas", reader.Read<UnityEngine.Canvas>(), instance);
					break;
					case "offset":
					instance = (CrewHealthBar)reader.SetPrivateField("offset", reader.Read<UnityEngine.Vector3>(), instance);
					break;
					case "target":
					instance = (CrewHealthBar)reader.SetPrivateField("target", reader.Read<UnityEngine.Transform>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_CrewHealthBarArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_CrewHealthBarArray() : base(typeof(CrewHealthBar[]), ES3UserType_CrewHealthBar.Instance)
		{
			Instance = this;
		}
	}
}