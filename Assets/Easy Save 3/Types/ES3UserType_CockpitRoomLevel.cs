using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("fuelEfficiency", "avoidEfficiency", "roomName", "level", "roomType", "category", "hitPoint", "size", "cost", "crewRequirement", "powerRequirement", "damageHitPointRate", "roomSprite", "roomIconPrefab", "toolTipPrefab", "possibleDoorPositions", "crewEntryGridPriority")]
	public class ES3UserType_CockpitRoomLevel : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_CockpitRoomLevel() : base(typeof(CockpitRoomData.CockpitRoomLevel)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (CockpitRoomData.CockpitRoomLevel)obj;
			
			writer.WriteProperty("fuelEfficiency", instance.fuelEfficiency, ES3Type_float.Instance);
			writer.WriteProperty("avoidEfficiency", instance.avoidEfficiency, ES3Type_float.Instance);
			writer.WriteProperty("roomName", instance.roomName, ES3Type_string.Instance);
			writer.WriteProperty("level", instance.level, ES3Type_int.Instance);
			writer.WriteProperty("roomType", instance.roomType, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(RoomType)));
			writer.WriteProperty("category", instance.category, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(RoomCategory)));
			writer.WriteProperty("hitPoint", instance.hitPoint, ES3Type_int.Instance);
			writer.WriteProperty("size", instance.size, ES3Type_Vector2Int.Instance);
			writer.WriteProperty("cost", instance.cost, ES3Type_int.Instance);
			writer.WriteProperty("crewRequirement", instance.crewRequirement, ES3Type_int.Instance);
			writer.WriteProperty("powerRequirement", instance.powerRequirement, ES3Type_float.Instance);
			writer.WriteProperty("damageHitPointRate", instance.damageHitPointRate, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(RoomDamageRates)));
			writer.WritePropertyByRef("roomSprite", instance.roomSprite);
			writer.WritePropertyByRef("roomIconPrefab", instance.roomIconPrefab);
			writer.WritePropertyByRef("toolTipPrefab", instance.toolTipPrefab);
			writer.WriteProperty("possibleDoorPositions", instance.possibleDoorPositions, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<DoorPosition>)));
			writer.WriteProperty("crewEntryGridPriority", instance.crewEntryGridPriority, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<UnityEngine.Vector2Int>)));
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (CockpitRoomData.CockpitRoomLevel)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "fuelEfficiency":
						instance.fuelEfficiency = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "avoidEfficiency":
						instance.avoidEfficiency = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "roomName":
						instance.roomName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "level":
						instance.level = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "roomType":
						instance.roomType = reader.Read<RoomType>();
						break;
					case "category":
						instance.category = reader.Read<RoomCategory>();
						break;
					case "hitPoint":
						instance.hitPoint = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "size":
						instance.size = reader.Read<UnityEngine.Vector2Int>(ES3Type_Vector2Int.Instance);
						break;
					case "cost":
						instance.cost = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "crewRequirement":
						instance.crewRequirement = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "powerRequirement":
						instance.powerRequirement = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "damageHitPointRate":
						instance.damageHitPointRate = reader.Read<RoomDamageRates>();
						break;
					case "roomSprite":
						instance.roomSprite = reader.Read<UnityEngine.Sprite>(ES3Type_Sprite.Instance);
						break;
					case "roomIconPrefab":
						instance.roomIconPrefab = reader.Read<UnityEngine.GameObject>(ES3Type_GameObject.Instance);
						break;
					case "toolTipPrefab":
						instance.toolTipPrefab = reader.Read<UnityEngine.GameObject>(ES3Type_GameObject.Instance);
						break;
					case "possibleDoorPositions":
						instance.possibleDoorPositions = reader.Read<System.Collections.Generic.List<DoorPosition>>();
						break;
					case "crewEntryGridPriority":
						instance.crewEntryGridPriority = reader.Read<System.Collections.Generic.List<UnityEngine.Vector2Int>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new CockpitRoomData.CockpitRoomLevel();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_CockpitRoomLevelArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_CockpitRoomLevelArray() : base(typeof(CockpitRoomData.CockpitRoomLevel[]), ES3UserType_CockpitRoomLevel.Instance)
		{
			Instance = this;
		}
	}
}