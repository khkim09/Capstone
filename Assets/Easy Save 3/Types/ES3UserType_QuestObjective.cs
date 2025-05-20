using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("objectiveType", "description", "targetId", "amount", "currentAmount", "targetPlanetDataId", "canComplete")]
	public class ES3UserType_QuestObjective : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_QuestObjective() : base(typeof(QuestObjective)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (QuestObjective)obj;
			
			writer.WriteProperty("objectiveType", instance.objectiveType, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(QuestObjectiveType)));
			writer.WriteProperty("description", instance.description, ES3Type_string.Instance);
			writer.WriteProperty("targetId", instance.targetId, ES3Type_int.Instance);
			writer.WriteProperty("amount", instance.amount, ES3Type_int.Instance);
			writer.WriteProperty("currentAmount", instance.currentAmount, ES3Type_int.Instance);
			writer.WriteProperty("targetPlanetDataId", instance.targetPlanetDataId, ES3Type_int.Instance);
			writer.WriteProperty("canComplete", instance.canComplete, ES3Type_bool.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (QuestObjective)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "objectiveType":
						instance.objectiveType = reader.Read<QuestObjectiveType>();
						break;
					case "description":
						instance.description = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "targetId":
						instance.targetId = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "amount":
						instance.amount = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "currentAmount":
						instance.currentAmount = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "targetPlanetDataId":
						instance.targetPlanetDataId = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "canComplete":
						instance.canComplete = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new QuestObjective();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_QuestObjectiveArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_QuestObjectiveArray() : base(typeof(QuestObjective[]), ES3UserType_QuestObjective.Instance)
		{
			Instance = this;
		}
	}
}