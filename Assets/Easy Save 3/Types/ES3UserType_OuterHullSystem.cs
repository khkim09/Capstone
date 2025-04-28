using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("parentShip")]
	public class ES3UserType_OuterHullSystem : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_OuterHullSystem() : base(typeof(OuterHullSystem)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (OuterHullSystem)obj;
			
			writer.WritePrivateFieldByRef("parentShip", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (OuterHullSystem)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "parentShip":
					instance = (OuterHullSystem)reader.SetPrivateField("parentShip", reader.Read<Ship>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new OuterHullSystem();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_OuterHullSystemArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_OuterHullSystemArray() : base(typeof(OuterHullSystem[]), ES3UserType_OuterHullSystem.Instance)
		{
			Instance = this;
		}
	}
}