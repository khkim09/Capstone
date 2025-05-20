using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("questId", "title", "description", "questAcceptedYear", "acceptedPlanetId", "status", "objectives", "rewards", "canCompleteQuest")]
	public class ES3UserType_RandomQuest : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_RandomQuest() : base(typeof(RandomQuest)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (RandomQuest)obj;
			
			writer.WriteProperty("questId", instance.questId, ES3Type_string.Instance);
			writer.WriteProperty("title", instance.title, ES3Type_string.Instance);
			writer.WriteProperty("description", instance.description, ES3Type_string.Instance);
			writer.WriteProperty("questAcceptedYear", instance.questAcceptedYear, ES3Type_int.Instance);
			writer.WriteProperty("acceptedPlanetId", instance.acceptedPlanetId, ES3Type_string.Instance);
			writer.WriteProperty("status", instance.status, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(QuestStatus)));
			writer.WriteProperty("objectives", instance.objectives, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<QuestObjective>)));
			writer.WriteProperty("rewards", instance.rewards, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<QuestReward>)));
			writer.WritePrivateField("canCompleteQuest", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (RandomQuest)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "questId":
						instance.questId = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "title":
						instance.title = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "description":
						instance.description = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "questAcceptedYear":
						instance.questAcceptedYear = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "acceptedPlanetId":
						instance.acceptedPlanetId = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "status":
						instance.status = reader.Read<QuestStatus>();
						break;
					case "objectives":
						instance.objectives = reader.Read<System.Collections.Generic.List<QuestObjective>>();
						break;
					case "rewards":
						instance.rewards = reader.Read<System.Collections.Generic.List<QuestReward>>();
						break;
					case "canCompleteQuest":
					instance = (RandomQuest)reader.SetPrivateField("canCompleteQuest", reader.Read<System.Boolean>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new RandomQuest();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_RandomQuestArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_RandomQuestArray() : base(typeof(RandomQuest[]), ES3UserType_RandomQuest.Instance)
		{
			Instance = this;
		}
	}
}