using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("currentRoomLevelData", "roomData", "position", "currentLevel", "currentHitPoints", "roomType", "isDamageable", "connectedDoors", "currentRotation", "crewInRoom", "isActive", "isPowered", "isPowerRequested", "roomRenderer", "parentShip", "icon", "gridSize", "damageCondition", "workDirection")]
	public class ES3UserType_PowerRoom : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_PowerRoom() : base(typeof(PowerRoom)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (PowerRoom)obj;
			
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
			writer.WritePrivateFieldByRef("roomRenderer", instance);
			writer.WritePropertyByRef("parentShip", instance.parentShip);
			writer.WritePrivateFieldByRef("icon", instance);
			writer.WritePrivateField("gridSize", instance);
			writer.WriteProperty("damageCondition", instance.damageCondition, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(DamageLevel)));
			writer.WriteProperty("workDirection", instance.workDirection, ES3Type_Vector2Int.Instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (PowerRoom)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "currentRoomLevelData":
					instance = (PowerRoom)reader.SetPrivateField("currentRoomLevelData", reader.Read<PowerRoomData.PowerRoomLevel>(), instance);
					break;
					case "roomData":
					instance = (PowerRoom)reader.SetPrivateField("roomData", reader.Read<RoomData>(), instance);
					break;
					case "position":
						instance.position = reader.Read<UnityEngine.Vector2Int>(ES3Type_Vector2Int.Instance);
						break;
					case "currentLevel":
					instance = (PowerRoom)reader.SetPrivateField("currentLevel", reader.Read<System.Int32>(), instance);
					break;
					case "currentHitPoints":
						instance.currentHitPoints = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "roomType":
						instance.roomType = reader.Read<RoomType>();
						break;
					case "isDamageable":
					instance = (PowerRoom)reader.SetPrivateField("isDamageable", reader.Read<System.Boolean>(), instance);
					break;
					case "connectedDoors":
					instance = (PowerRoom)reader.SetPrivateField("connectedDoors", reader.Read<System.Collections.Generic.List<Door>>(), instance);
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
					instance = (PowerRoom)reader.SetPrivateField("isPowered", reader.Read<System.Boolean>(), instance);
					break;
					case "isPowerRequested":
					instance = (PowerRoom)reader.SetPrivateField("isPowerRequested", reader.Read<System.Boolean>(), instance);
					break;
					case "roomRenderer":
					instance = (PowerRoom)reader.SetPrivateField("roomRenderer", reader.Read<UnityEngine.SpriteRenderer>(), instance);
					break;
					case "parentShip":
						instance.parentShip = reader.Read<Ship>(ES3UserType_Ship.Instance);
						break;
					case "icon":
					instance = (PowerRoom)reader.SetPrivateField("icon", reader.Read<UnityEngine.SpriteRenderer>(), instance);
					break;
					case "gridSize":
					instance = (PowerRoom)reader.SetPrivateField("gridSize", reader.Read<UnityEngine.Vector2Int>(), instance);
					break;
					case "damageCondition":
						instance.damageCondition = reader.Read<DamageLevel>();
						break;
					case "workDirection":
						instance.workDirection = reader.Read<UnityEngine.Vector2Int>(ES3Type_Vector2Int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_PowerRoomArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_PowerRoomArray() : base(typeof(PowerRoom[]), ES3UserType_PowerRoom.Instance)
		{
			Instance = this;
		}
	}
}