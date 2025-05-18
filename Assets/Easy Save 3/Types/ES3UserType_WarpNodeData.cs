using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("normalizedPosition", "isStartNode", "isEndNode", "layer", "indexInLayer", "connectionIds", "nodeId")]
	public class ES3UserType_WarpNodeData : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_WarpNodeData() : base(typeof(WarpNodeData)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (WarpNodeData)obj;
			
			writer.WriteProperty("normalizedPosition", instance.normalizedPosition, ES3Type_Vector2.Instance);
			writer.WriteProperty("isStartNode", instance.isStartNode, ES3Type_bool.Instance);
			writer.WriteProperty("isEndNode", instance.isEndNode, ES3Type_bool.Instance);
			writer.WriteProperty("layer", instance.layer, ES3Type_int.Instance);
			writer.WriteProperty("indexInLayer", instance.indexInLayer, ES3Type_int.Instance);
			writer.WriteProperty("connectionIds", instance.connectionIds, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<System.Int32>)));
			writer.WriteProperty("nodeId", instance.nodeId, ES3Type_int.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (WarpNodeData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "normalizedPosition":
						instance.normalizedPosition = reader.Read<UnityEngine.Vector2>(ES3Type_Vector2.Instance);
						break;
					case "isStartNode":
						instance.isStartNode = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "isEndNode":
						instance.isEndNode = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "layer":
						instance.layer = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "indexInLayer":
						instance.indexInLayer = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "connectionIds":
						instance.connectionIds = reader.Read<System.Collections.Generic.List<System.Int32>>();
						break;
					case "nodeId":
						instance.nodeId = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new WarpNodeData();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_WarpNodeDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_WarpNodeDataArray() : base(typeof(WarpNodeData[]), ES3UserType_WarpNodeData.Instance)
		{
			Instance = this;
		}
	}
}