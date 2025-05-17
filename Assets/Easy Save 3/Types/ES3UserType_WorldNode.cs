using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("worldNodeImage", "worldNodeData")]
	public class ES3UserType_WorldNode : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_WorldNode() : base(typeof(WorldNode)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (WorldNode)obj;
			
			writer.WritePrivateFieldByRef("worldNodeImage", instance);
			writer.WriteProperty("worldNodeData", instance.worldNodeData, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(WorldNodeData)));
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (WorldNode)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "worldNodeImage":
					instance = (WorldNode)reader.SetPrivateField("worldNodeImage", reader.Read<UnityEngine.UI.Image>(), instance);
					break;
					case "worldNodeData":
						instance.worldNodeData = reader.Read<WorldNodeData>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_WorldNodeArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_WorldNodeArray() : base(typeof(WorldNode[]), ES3UserType_WorldNode.Instance)
		{
			Instance = this;
		}
	}
}