using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("questRewardType", "amount")]
	public class ES3UserType_QuestReward : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_QuestReward() : base(typeof(QuestReward)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (QuestReward)obj;
			
			writer.WriteProperty("questRewardType", instance.questRewardType, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(QuestRewardType)));
			writer.WriteProperty("amount", instance.amount, ES3Type_int.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (QuestReward)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "questRewardType":
						instance.questRewardType = reader.Read<QuestRewardType>();
						break;
					case "amount":
						instance.amount = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new QuestReward();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_QuestRewardArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_QuestRewardArray() : base(typeof(QuestReward[]), ES3UserType_QuestReward.Instance)
		{
			Instance = this;
		}
	}
}