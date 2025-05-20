using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("position", "direction")]
	public class ES3UserType_DoorPosition : ES3Type
	{
		public static ES3Type Instance = null;

		public ES3UserType_DoorPosition() : base(typeof(DoorPosition)){ Instance = this; priority = 1;}


		public override void Write(object obj, ES3Writer writer)
		{
			var instance = (DoorPosition)obj;
			
			writer.WriteProperty("position", instance.position, ES3Type_Vector2Int.Instance);
			writer.WriteProperty("direction", instance.direction, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(DoorDirection)));
		}

		public override object Read<T>(ES3Reader reader)
		{
			var instance = new DoorPosition();
			string propertyName;
			while((propertyName = reader.ReadPropertyName()) != null)
			{
				switch(propertyName)
				{
					
					case "position":
						instance.position = reader.Read<UnityEngine.Vector2Int>(ES3Type_Vector2Int.Instance);
						break;
					case "direction":
						instance.direction = reader.Read<DoorDirection>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
			return instance;
		}
	}


	public class ES3UserType_DoorPositionArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_DoorPositionArray() : base(typeof(DoorPosition[]), ES3UserType_DoorPosition.Instance)
		{
			Instance = this;
		}
	}
}