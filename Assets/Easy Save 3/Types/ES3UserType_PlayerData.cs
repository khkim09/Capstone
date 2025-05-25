using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("questCleared", "mysteryFound", "pirateDefeated")]
	public class ES3UserType_PlayerData : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_PlayerData() : base(typeof(PlayerData)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (PlayerData)obj;
			
			writer.WriteProperty("questCleared", instance.questCleared, ES3Type_int.Instance);
			writer.WriteProperty("mysteryFound", instance.mysteryFound, ES3Type_int.Instance);
			writer.WriteProperty("pirateDefeated", instance.pirateDefeated, ES3Type_int.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (PlayerData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "questCleared":
						instance.questCleared = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "mysteryFound":
						instance.mysteryFound = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "pirateDefeated":
						instance.pirateDefeated = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new PlayerData();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_PlayerDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_PlayerDataArray() : base(typeof(PlayerData[]), ES3UserType_PlayerData.Instance)
		{
			Instance = this;
		}
	}
}