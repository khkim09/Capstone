using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("RoomLevels")]
	public class ES3UserType_CrewQuartersRoomData : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_CrewQuartersRoomData() : base(typeof(CrewQuartersRoomData)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (CrewQuartersRoomData)obj;
			
			writer.WriteProperty("RoomLevels", instance.RoomLevels, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<CrewQuartersRoomData.CrewQuartersRoomLevel>)));
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (CrewQuartersRoomData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "RoomLevels":
						instance.RoomLevels = reader.Read<System.Collections.Generic.List<CrewQuartersRoomData.CrewQuartersRoomLevel>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_CrewQuartersRoomDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_CrewQuartersRoomDataArray() : base(typeof(CrewQuartersRoomData[]), ES3UserType_CrewQuartersRoomData.Instance)
		{
			Instance = this;
		}
	}
}