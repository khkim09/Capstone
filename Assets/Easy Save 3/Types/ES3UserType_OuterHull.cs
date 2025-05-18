using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("outerHullData", "currentLevel", "currentLevelData", "gridPosition", "direction", "parentShip")]
	public class ES3UserType_OuterHull : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_OuterHull() : base(typeof(OuterHull)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (OuterHull)obj;
			
			writer.WritePrivateFieldByRef("outerHullData", instance);
			writer.WritePrivateField("currentLevel", instance);
			writer.WritePrivateField("currentLevelData", instance);
			writer.WriteProperty("gridPosition", instance.gridPosition, ES3Type_Vector2Int.Instance);
			writer.WriteProperty("direction", instance.direction, ES3Type_int.Instance);
			writer.WritePrivateFieldByRef("parentShip", instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (OuterHull)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "outerHullData":
					instance = (OuterHull)reader.SetPrivateField("outerHullData", reader.Read<OuterHullData>(), instance);
					break;
					case "currentLevel":
					instance = (OuterHull)reader.SetPrivateField("currentLevel", reader.Read<System.Int32>(), instance);
					break;
					case "currentLevelData":
					instance = (OuterHull)reader.SetPrivateField("currentLevelData", reader.Read<OuterHullData.OuterHullLevel>(), instance);
					break;
					case "gridPosition":
						instance.gridPosition = reader.Read<UnityEngine.Vector2Int>(ES3Type_Vector2Int.Instance);
						break;
					case "direction":
						instance.direction = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "parentShip":
					instance = (OuterHull)reader.SetPrivateField("parentShip", reader.Read<Ship>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_OuterHullArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_OuterHullArray() : base(typeof(OuterHull[]), ES3UserType_OuterHull.Instance)
		{
			Instance = this;
		}
	}
}