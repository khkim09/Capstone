using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("normalizedPosition", "isVisited", "isCurrentNode")]
	public class ES3UserType_WorldNodeData : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_WorldNodeData() : base(typeof(WorldNodeData)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (WorldNodeData)obj;
			
			writer.WriteProperty("normalizedPosition", instance.normalizedPosition, ES3Type_Vector2.Instance);
			writer.WriteProperty("isVisited", instance.isVisited, ES3Type_bool.Instance);
			writer.WriteProperty("isCurrentNode", instance.isCurrentNode, ES3Type_bool.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (WorldNodeData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "normalizedPosition":
						instance.normalizedPosition = reader.Read<UnityEngine.Vector2>(ES3Type_Vector2.Instance);
						break;
					case "isVisited":
						instance.isVisited = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "isCurrentNode":
						instance.isCurrentNode = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new WorldNodeData();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_WorldNodeDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_WorldNodeDataArray() : base(typeof(WorldNodeData[]), ES3UserType_WorldNodeData.Instance)
		{
			Instance = this;
		}
	}
}