using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("crews", "parentShip")]
	public class ES3UserType_CrewSystem : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_CrewSystem() : base(typeof(CrewSystem)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (CrewSystem)obj;
			
			writer.WritePrivateField("crews", instance);
			writer.WritePrivateFieldByRef("parentShip", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (CrewSystem)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "crews":
					instance = (CrewSystem)reader.SetPrivateField("crews", reader.Read<System.Collections.Generic.List<CrewBase>>(), instance);
					break;
					case "parentShip":
					instance = (CrewSystem)reader.SetPrivateField("parentShip", reader.Read<Ship>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new CrewSystem();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_CrewSystemArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_CrewSystemArray() : base(typeof(CrewSystem[]), ES3UserType_CrewSystem.Instance)
		{
			Instance = this;
		}
	}
}