using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("facilityType", "roomData")]
	public class ES3UserType_LifeSupportRoomDataEntry : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_LifeSupportRoomDataEntry() : base(typeof(RoomDatabase.LifeSupportRoomDataEntry)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (RoomDatabase.LifeSupportRoomDataEntry)obj;
			
			writer.WriteProperty("facilityType", instance.facilityType, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(LifeSupportRoomType)));
			writer.WritePropertyByRef("roomData", instance.roomData);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (RoomDatabase.LifeSupportRoomDataEntry)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "facilityType":
						instance.facilityType = reader.Read<LifeSupportRoomType>();
						break;
					case "roomData":
						instance.roomData = reader.Read<LifeSupportRoomData>(ES3UserType_LifeSupportRoomData.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new RoomDatabase.LifeSupportRoomDataEntry();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_LifeSupportRoomDataEntryArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_LifeSupportRoomDataEntryArray() : base(typeof(RoomDatabase.LifeSupportRoomDataEntry[]), ES3UserType_LifeSupportRoomDataEntry.Instance)
		{
			Instance = this;
		}
	}
}