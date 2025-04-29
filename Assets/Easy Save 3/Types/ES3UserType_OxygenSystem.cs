using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("currentOxygenRate", "currentOxygenLevel", "parentShip")]
	public class ES3UserType_OxygenSystem : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_OxygenSystem() : base(typeof(OxygenSystem)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (OxygenSystem)obj;
			
			writer.WritePrivateField("currentOxygenRate", instance);
			writer.WritePrivateField("currentOxygenLevel", instance);
			writer.WritePrivateFieldByRef("parentShip", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (OxygenSystem)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "currentOxygenRate":
					instance = (OxygenSystem)reader.SetPrivateField("currentOxygenRate", reader.Read<System.Single>(), instance);
					break;
					case "currentOxygenLevel":
					instance = (OxygenSystem)reader.SetPrivateField("currentOxygenLevel", reader.Read<OxygenLevel>(), instance);
					break;
					case "parentShip":
					instance = (OxygenSystem)reader.SetPrivateField("parentShip", reader.Read<Ship>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new OxygenSystem();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_OxygenSystemArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_OxygenSystemArray() : base(typeof(OxygenSystem[]), ES3UserType_OxygenSystem.Instance)
		{
			Instance = this;
		}
	}
}