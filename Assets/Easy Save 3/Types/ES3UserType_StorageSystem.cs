using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("parentShip")]
	public class ES3UserType_StorageSystem : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_StorageSystem() : base(typeof(StorageSystem)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (StorageSystem)obj;
			
			writer.WritePrivateFieldByRef("parentShip", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (StorageSystem)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "parentShip":
					instance = (StorageSystem)reader.SetPrivateField("parentShip", reader.Read<Ship>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new StorageSystem();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_StorageSystemArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_StorageSystemArray() : base(typeof(StorageSystem[]), ES3UserType_StorageSystem.Instance)
		{
			Instance = this;
		}
	}
}