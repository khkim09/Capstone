using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("RoomLevels")]
	public class ES3UserType_MedBayRoomData : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_MedBayRoomData() : base(typeof(MedBayRoomData)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (MedBayRoomData)obj;
			
			writer.WriteProperty("RoomLevels", instance.RoomLevels, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<MedBayRoomData.MedBayRoomLevel>)));
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (MedBayRoomData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "RoomLevels":
						instance.RoomLevels = reader.Read<System.Collections.Generic.List<MedBayRoomData.MedBayRoomLevel>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_MedBayRoomDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_MedBayRoomDataArray() : base(typeof(MedBayRoomData[]), ES3UserType_MedBayRoomData.Instance)
		{
			Instance = this;
		}
	}
}