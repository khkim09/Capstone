using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("currentRoomLevelData", "position", "currentLevel", "currentHitPoints", "roomType", "isDamageable", "connectedDoors", "currentRotation", "isActive", "isPowered", "isPowerRequested", "parentShip", "gridSize", "roomData")]
	public class ES3UserType_EngineRoom : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_EngineRoom() : base(typeof(EngineRoom)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (EngineRoom)obj;
			
			writer.WritePrivateField("currentRoomLevelData", instance);
			writer.WriteProperty("position", instance.position, ES3Type_Vector2Int.Instance);
			writer.WritePrivateField("currentLevel", instance);
			writer.WriteProperty("currentHitPoints", instance.currentHitPoints, ES3Type_float.Instance);
			writer.WriteProperty("roomType", instance.roomType, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(RoomType)));
			writer.WritePrivateField("isDamageable", instance);
			writer.WritePrivateField("connectedDoors", instance);
			writer.WriteProperty("currentRotation", instance.currentRotation, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(RotationConstants.Rotation)));
			writer.WritePrivateField("isActive", instance);
			writer.WritePrivateField("isPowered", instance);
			writer.WritePrivateField("isPowerRequested", instance);
			writer.WritePrivateFieldByRef("parentShip", instance);
			writer.WritePrivateField("gridSize", instance);
			writer.WritePropertyByRef("roomData", instance.roomData);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (EngineRoom)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "currentRoomLevelData":
					instance = (EngineRoom)reader.SetPrivateField("currentRoomLevelData", reader.Read<EngineRoomData.EngineRoomLevel>(), instance);
					break;
					case "position":
						instance.position = reader.Read<UnityEngine.Vector2Int>(ES3Type_Vector2Int.Instance);
						break;
					case "currentLevel":
					instance = (EngineRoom)reader.SetPrivateField("currentLevel", reader.Read<System.Int32>(), instance);
					break;
					case "currentHitPoints":
						instance.currentHitPoints = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "roomType":
						instance.roomType = reader.Read<RoomType>();
						break;
					case "isDamageable":
					instance = (EngineRoom)reader.SetPrivateField("isDamageable", reader.Read<System.Boolean>(), instance);
					break;
					case "connectedDoors":
					instance = (EngineRoom)reader.SetPrivateField("connectedDoors", reader.Read<System.Collections.Generic.List<Door>>(), instance);
					break;
					case "currentRotation":
						instance.currentRotation = reader.Read<RotationConstants.Rotation>();
						break;
					case "isActive":
					instance = (EngineRoom)reader.SetPrivateField("isActive", reader.Read<System.Boolean>(), instance);
					break;
					case "isPowered":
					instance = (EngineRoom)reader.SetPrivateField("isPowered", reader.Read<System.Boolean>(), instance);
					break;
					case "isPowerRequested":
					instance = (EngineRoom)reader.SetPrivateField("isPowerRequested", reader.Read<System.Boolean>(), instance);
					break;
					case "parentShip":
					instance = (EngineRoom)reader.SetPrivateField("parentShip", reader.Read<Ship>(), instance);
					break;
					case "gridSize":
					instance = (EngineRoom)reader.SetPrivateField("gridSize", reader.Read<UnityEngine.Vector2Int>(), instance);
					break;
					case "roomData":
						instance.roomData = reader.Read<EngineRoomData>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_EngineRoomArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_EngineRoomArray() : base(typeof(EngineRoom[]), ES3UserType_EngineRoom.Instance)
		{
			Instance = this;
		}
	}
}