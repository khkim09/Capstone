using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("type", "valueType", "floatAmount", "intAmount", "maxFloatAmount", "maxIntAmount")]
	public class ES3UserType_ResourcesData : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_ResourcesData() : base(typeof(ResourcesData)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (ResourcesData)obj;
			
			writer.WriteProperty("type", instance.type, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(ResourceType)));
			writer.WriteProperty("valueType", instance.valueType, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(ResourceValueType)));
			writer.WriteProperty("floatAmount", instance.floatAmount, ES3Type_float.Instance);
			writer.WriteProperty("intAmount", instance.intAmount, ES3Type_int.Instance);
			writer.WriteProperty("maxFloatAmount", instance.maxFloatAmount, ES3Type_float.Instance);
			writer.WriteProperty("maxIntAmount", instance.maxIntAmount, ES3Type_int.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (ResourcesData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "type":
						instance.type = reader.Read<ResourceType>();
						break;
					case "valueType":
						instance.valueType = reader.Read<ResourceValueType>();
						break;
					case "floatAmount":
						instance.floatAmount = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "intAmount":
						instance.intAmount = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "maxFloatAmount":
						instance.maxFloatAmount = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "maxIntAmount":
						instance.maxIntAmount = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new ResourcesData();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_ResourcesDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_ResourcesDataArray() : base(typeof(ResourcesData[]), ES3UserType_ResourcesData.Instance)
		{
			Instance = this;
		}
	}
}