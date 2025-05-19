using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("RoomLevels")]
	public class ES3UserType_ShieldRoomData : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_ShieldRoomData() : base(typeof(ShieldRoomData)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (ShieldRoomData)obj;
			
			writer.WriteProperty("RoomLevels", instance.RoomLevels, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<ShieldRoomData.ShieldRoomLevel>)));
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (ShieldRoomData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "RoomLevels":
						instance.RoomLevels = reader.Read<System.Collections.Generic.List<ShieldRoomData.ShieldRoomLevel>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_ShieldRoomDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_ShieldRoomDataArray() : base(typeof(ShieldRoomData[]), ES3UserType_ShieldRoomData.Instance)
		{
			Instance = this;
		}
	}
}