using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("isPersonalRoom", "currentRoomLevelData", "roomData", "position", "currentLevel", "currentHitPoints", "roomType", "isDamageable", "connectedDoors", "currentRotation", "crewInRoom", "isActive", "isPowered", "isPowerRequested", "parentShip", "gridSize", "damageCondition")]
	public class ES3UserType_CrewQuartersRoom : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_CrewQuartersRoom() : base(typeof(CrewQuartersRoom)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (CrewQuartersRoom)obj;
			
			writer.WriteProperty("isPersonalRoom", instance.isPersonalRoom, ES3Type_bool.Instance);
			writer.WritePrivateField("currentRoomLevelData", instance);
			writer.WritePrivateFieldByRef("roomData", instance);
			writer.WriteProperty("position", instance.position, ES3Type_Vector2Int.Instance);
			writer.WritePrivateField("currentLevel", instance);
			writer.WriteProperty("currentHitPoints", instance.currentHitPoints, ES3Type_float.Instance);
			writer.WriteProperty("roomType", instance.roomType, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(RoomType)));
			writer.WritePrivateField("isDamageable", instance);
			writer.WritePrivateField("connectedDoors", instance);
			writer.WriteProperty("currentRotation", instance.currentRotation, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(Constants.Rotations.Rotation)));
			writer.WriteProperty("crewInRoom", instance.crewInRoom, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<CrewMember>)));
			writer.WriteProperty("isActive", instance.isActive, ES3Type_bool.Instance);
			writer.WritePrivateField("isPowered", instance);
			writer.WritePrivateField("isPowerRequested", instance);
			writer.WritePropertyByRef("parentShip", instance.parentShip);
			writer.WritePrivateField("gridSize", instance);
			writer.WriteProperty("damageCondition", instance.damageCondition, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(DamageLevel)));
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (CrewQuartersRoom)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "isPersonalRoom":
						instance.isPersonalRoom = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "currentRoomLevelData":
					instance = (CrewQuartersRoom)reader.SetPrivateField("currentRoomLevelData", reader.Read<CrewQuartersRoomData.CrewQuartersRoomLevel>(), instance);
					break;
					case "roomData":
					instance = (CrewQuartersRoom)reader.SetPrivateField("roomData", reader.Read<RoomData>(), instance);
					break;
					case "position":
						instance.position = reader.Read<UnityEngine.Vector2Int>(ES3Type_Vector2Int.Instance);
						break;
					case "currentLevel":
					instance = (CrewQuartersRoom)reader.SetPrivateField("currentLevel", reader.Read<System.Int32>(), instance);
					break;
					case "currentHitPoints":
						instance.currentHitPoints = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "roomType":
						instance.roomType = reader.Read<RoomType>();
						break;
					case "isDamageable":
					instance = (CrewQuartersRoom)reader.SetPrivateField("isDamageable", reader.Read<System.Boolean>(), instance);
					break;
					case "connectedDoors":
					instance = (CrewQuartersRoom)reader.SetPrivateField("connectedDoors", reader.Read<System.Collections.Generic.List<Door>>(), instance);
					break;
					case "currentRotation":
						instance.currentRotation = reader.Read<Constants.Rotations.Rotation>();
						break;
					case "crewInRoom":
						instance.crewInRoom = reader.Read<System.Collections.Generic.List<CrewMember>>();
						break;
					case "isActive":
						instance.isActive = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "isPowered":
					instance = (CrewQuartersRoom)reader.SetPrivateField("isPowered", reader.Read<System.Boolean>(), instance);
					break;
					case "isPowerRequested":
					instance = (CrewQuartersRoom)reader.SetPrivateField("isPowerRequested", reader.Read<System.Boolean>(), instance);
					break;
					case "parentShip":
						instance.parentShip = reader.Read<Ship>(ES3UserType_Ship.Instance);
						break;
					case "gridSize":
					instance = (CrewQuartersRoom)reader.SetPrivateField("gridSize", reader.Read<UnityEngine.Vector2Int>(), instance);
					break;
					case "damageCondition":
						instance.damageCondition = reader.Read<DamageLevel>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_CrewQuartersRoomArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_CrewQuartersRoomArray() : base(typeof(CrewQuartersRoom[]), ES3UserType_CrewQuartersRoom.Instance)
		{
			Instance = this;
		}
	}
}