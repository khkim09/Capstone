using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("RoomLevels")]
	public class ES3UserType_OxygenRoomData : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_OxygenRoomData() : base(typeof(OxygenRoomData)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (OxygenRoomData)obj;
			
			writer.WriteProperty("RoomLevels", instance.RoomLevels, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<OxygenRoomData.OxygenLevel>)));
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (OxygenRoomData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "RoomLevels":
						instance.RoomLevels = reader.Read<System.Collections.Generic.List<OxygenRoomData.OxygenLevel>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_OxygenRoomDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_OxygenRoomDataArray() : base(typeof(OxygenRoomData[]), ES3UserType_OxygenRoomData.Instance)
		{
			Instance = this;
		}
	}
}