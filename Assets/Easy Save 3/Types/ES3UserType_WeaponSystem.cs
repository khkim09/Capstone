using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("parentShip")]
	public class ES3UserType_WeaponSystem : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_WeaponSystem() : base(typeof(WeaponSystem)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (WeaponSystem)obj;
			
			writer.WritePrivateFieldByRef("parentShip", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (WeaponSystem)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "parentShip":
					instance = (WeaponSystem)reader.SetPrivateField("parentShip", reader.Read<Ship>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new WeaponSystem();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_WeaponSystemArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_WeaponSystemArray() : base(typeof(WeaponSystem[]), ES3UserType_WeaponSystem.Instance)
		{
			Instance = this;
		}
	}
}