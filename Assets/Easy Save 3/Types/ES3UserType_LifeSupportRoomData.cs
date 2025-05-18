using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("RoomLevels")]
	public class ES3UserType_LifeSupportRoomData : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_LifeSupportRoomData() : base(typeof(LifeSupportRoomData)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (LifeSupportRoomData)obj;
			
			writer.WriteProperty("RoomLevels", instance.RoomLevels, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<LifeSupportRoomData.LifeSupportRoomLevel>)));
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (LifeSupportRoomData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "RoomLevels":
						instance.RoomLevels = reader.Read<System.Collections.Generic.List<LifeSupportRoomData.LifeSupportRoomLevel>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_LifeSupportRoomDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_LifeSupportRoomDataArray() : base(typeof(LifeSupportRoomData[]), ES3UserType_LifeSupportRoomData.Instance)
		{
			Instance = this;
		}
	}
}