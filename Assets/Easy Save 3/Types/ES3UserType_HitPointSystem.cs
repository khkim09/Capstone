using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("currentHitPoint", "parentShip")]
	public class ES3UserType_HitPointSystem : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_HitPointSystem() : base(typeof(HitPointSystem)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (HitPointSystem)obj;
			
			writer.WritePrivateField("currentHitPoint", instance);
			writer.WritePrivateFieldByRef("parentShip", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (HitPointSystem)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "currentHitPoint":
					instance = (HitPointSystem)reader.SetPrivateField("currentHitPoint", reader.Read<System.Single>(), instance);
					break;
					case "parentShip":
					instance = (HitPointSystem)reader.SetPrivateField("parentShip", reader.Read<Ship>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new HitPointSystem();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_HitPointSystemArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_HitPointSystemArray() : base(typeof(HitPointSystem[]), ES3UserType_HitPointSystem.Instance)
		{
			Instance = this;
		}
	}
}