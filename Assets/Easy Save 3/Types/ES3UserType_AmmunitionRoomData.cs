using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("RoomLevels")]
	public class ES3UserType_AmmunitionRoomData : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_AmmunitionRoomData() : base(typeof(AmmunitionRoomData)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (AmmunitionRoomData)obj;
			
			writer.WriteProperty("RoomLevels", instance.RoomLevels, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<AmmunitionRoomData.AmmunitionRoomLevel>)));
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (AmmunitionRoomData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "RoomLevels":
						instance.RoomLevels = reader.Read<System.Collections.Generic.List<AmmunitionRoomData.AmmunitionRoomLevel>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_AmmunitionRoomDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_AmmunitionRoomDataArray() : base(typeof(AmmunitionRoomData[]), ES3UserType_AmmunitionRoomData.Instance)
		{
			Instance = this;
		}
	}
}