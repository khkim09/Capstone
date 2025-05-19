using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("roomType", "roomData")]
	public class ES3UserType_RoomDataEntry : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_RoomDataEntry() : base(typeof(RoomDatabase.RoomDataEntry)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (RoomDatabase.RoomDataEntry)obj;
			
			writer.WriteProperty("roomType", instance.roomType, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(RoomType)));
			writer.WritePropertyByRef("roomData", instance.roomData);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (RoomDatabase.RoomDataEntry)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "roomType":
						instance.roomType = reader.Read<RoomType>();
						break;
					case "roomData":
						instance.roomData = reader.Read<RoomData>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new RoomDatabase.RoomDataEntry();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_RoomDataEntryArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_RoomDataEntryArray() : base(typeof(RoomDatabase.RoomDataEntry[]), ES3UserType_RoomDataEntry.Instance)
		{
			Instance = this;
		}
	}
}