using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("currentShield", "shieldRespawnTimer", "isShieldDestroyed", "parentShip")]
	public class ES3UserType_ShieldSystem : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_ShieldSystem() : base(typeof(ShieldSystem)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (ShieldSystem)obj;
			
			writer.WritePrivateField("currentShield", instance);
			writer.WritePrivateField("shieldRespawnTimer", instance);
			writer.WritePrivateField("isShieldDestroyed", instance);
			writer.WritePrivateFieldByRef("parentShip", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (ShieldSystem)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "currentShield":
					instance = (ShieldSystem)reader.SetPrivateField("currentShield", reader.Read<System.Single>(), instance);
					break;
					case "shieldRespawnTimer":
					instance = (ShieldSystem)reader.SetPrivateField("shieldRespawnTimer", reader.Read<System.Single>(), instance);
					break;
					case "isShieldDestroyed":
					instance = (ShieldSystem)reader.SetPrivateField("isShieldDestroyed", reader.Read<System.Boolean>(), instance);
					break;
					case "parentShip":
					instance = (ShieldSystem)reader.SetPrivateField("parentShip", reader.Read<Ship>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new ShieldSystem();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_ShieldSystemArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_ShieldSystemArray() : base(typeof(ShieldSystem[]), ES3UserType_ShieldSystem.Instance)
		{
			Instance = this;
		}
	}
}