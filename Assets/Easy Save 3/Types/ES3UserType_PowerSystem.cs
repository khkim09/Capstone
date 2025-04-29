using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("parentShip")]
	public class ES3UserType_PowerSystem : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_PowerSystem() : base(typeof(PowerSystem)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (PowerSystem)obj;
			
			writer.WritePrivateFieldByRef("parentShip", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (PowerSystem)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "parentShip":
					instance = (PowerSystem)reader.SetPrivateField("parentShip", reader.Read<Ship>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new PowerSystem();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_PowerSystemArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_PowerSystemArray() : base(typeof(PowerSystem[]), ES3UserType_PowerSystem.Instance)
		{
			Instance = this;
		}
	}
}