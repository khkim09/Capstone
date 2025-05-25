using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute()]
	public class ES3UserType_EquipmentItem : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_EquipmentItem() : base(typeof(EquipmentItem)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (EquipmentItem)obj;
			
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (EquipmentItem)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_EquipmentItemArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_EquipmentItemArray() : base(typeof(EquipmentItem[]), ES3UserType_EquipmentItem.Instance)
		{
			Instance = this;
		}
	}
}