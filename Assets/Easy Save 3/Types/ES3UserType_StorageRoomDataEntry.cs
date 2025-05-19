using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("storageType", "size", "roomData")]
	public class ES3UserType_StorageRoomDataEntry : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_StorageRoomDataEntry() : base(typeof(RoomDatabase.StorageRoomDataEntry)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (RoomDatabase.StorageRoomDataEntry)obj;
			
			writer.WriteProperty("storageType", instance.storageType, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(StorageType)));
			writer.WriteProperty("size", instance.size, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(StorageSize)));
			writer.WritePropertyByRef("roomData", instance.roomData);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (RoomDatabase.StorageRoomDataEntry)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "storageType":
						instance.storageType = reader.Read<StorageType>();
						break;
					case "size":
						instance.size = reader.Read<StorageSize>();
						break;
					case "roomData":
						instance.roomData = reader.Read<StorageRoomBaseData>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new RoomDatabase.StorageRoomDataEntry();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_StorageRoomDataEntryArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_StorageRoomDataEntryArray() : base(typeof(RoomDatabase.StorageRoomDataEntry[]), ES3UserType_StorageRoomDataEntry.Instance)
		{
			Instance = this;
		}
	}
}