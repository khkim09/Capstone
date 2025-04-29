using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("parentShip")]
	public class ES3UserType_MoraleSystem : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_MoraleSystem() : base(typeof(MoraleSystem)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (MoraleSystem)obj;
			
			writer.WritePrivateFieldByRef("parentShip", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (MoraleSystem)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "parentShip":
					instance = (MoraleSystem)reader.SetPrivateField("parentShip", reader.Read<Ship>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new MoraleSystem();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_MoraleSystemArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_MoraleSystemArray() : base(typeof(MoraleSystem[]), ES3UserType_MoraleSystem.Instance)
		{
			Instance = this;
		}
	}
}