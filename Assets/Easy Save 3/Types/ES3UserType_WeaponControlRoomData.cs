using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("RoomLevels")]
	public class ES3UserType_WeaponControlRoomData : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_WeaponControlRoomData() : base(typeof(WeaponControlRoomData)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (WeaponControlRoomData)obj;
			
			writer.WriteProperty("RoomLevels", instance.RoomLevels, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<WeaponControlRoomData.WeaponControlRoomLevel>)));
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (WeaponControlRoomData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "RoomLevels":
						instance.RoomLevels = reader.Read<System.Collections.Generic.List<WeaponControlRoomData.WeaponControlRoomLevel>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_WeaponControlRoomDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_WeaponControlRoomDataArray() : base(typeof(WeaponControlRoomData[]), ES3UserType_WeaponControlRoomData.Instance)
		{
			Instance = this;
		}
	}
}