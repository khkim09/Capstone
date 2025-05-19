using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("RoomLevels")]
	public class ES3UserType_CorridorRoomData : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_CorridorRoomData() : base(typeof(CorridorRoomData)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (CorridorRoomData)obj;
			
			writer.WriteProperty("RoomLevels", instance.RoomLevels, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<CorridorRoomData.CorridorRoomLevel>)));
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (CorridorRoomData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "RoomLevels":
						instance.RoomLevels = reader.Read<System.Collections.Generic.List<CorridorRoomData.CorridorRoomLevel>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_CorridorRoomDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_CorridorRoomDataArray() : base(typeof(CorridorRoomData[]), ES3UserType_CorridorRoomData.Instance)
		{
			Instance = this;
		}
	}
}