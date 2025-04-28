using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("currentRoomLevelData", "position", "currentLevel", "currentHitPoints", "roomType", "isDamageable", "connectedDoors", "currentRotation", "isActive", "isPowered", "isPowerRequested", "parentShip", "gridSize", "roomData")]
	public class ES3UserType_StorageRoomAnimal : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_StorageRoomAnimal() : base(typeof(StorageRoomAnimal)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (StorageRoomAnimal)obj;
			
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
			var instance = (StorageRoomAnimal)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "currentRoomLevelData":
					instance = (StorageRoomAnimal)reader.SetPrivateField("currentRoomLevelData", reader.Read<StorageRoomBaseData.StorageRoomBaseLevel>(), instance);
					break;
					case "position":
						instance.position = reader.Read<UnityEngine.Vector2Int>(ES3Type_Vector2Int.Instance);
						break;
					case "currentLevel":
					instance = (StorageRoomAnimal)reader.SetPrivateField("currentLevel", reader.Read<System.Int32>(), instance);
					break;
					case "currentHitPoints":
						instance.currentHitPoints = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "roomType":
						instance.roomType = reader.Read<RoomType>();
						break;
					case "isDamageable":
					instance = (StorageRoomAnimal)reader.SetPrivateField("isDamageable", reader.Read<System.Boolean>(), instance);
					break;
					case "connectedDoors":
					instance = (StorageRoomAnimal)reader.SetPrivateField("connectedDoors", reader.Read<System.Collections.Generic.List<Door>>(), instance);
					break;
					case "currentRotation":
						instance.currentRotation = reader.Read<RotationConstants.Rotation>();
						break;
					case "isActive":
					instance = (StorageRoomAnimal)reader.SetPrivateField("isActive", reader.Read<System.Boolean>(), instance);
					break;
					case "isPowered":
					instance = (StorageRoomAnimal)reader.SetPrivateField("isPowered", reader.Read<System.Boolean>(), instance);
					break;
					case "isPowerRequested":
					instance = (StorageRoomAnimal)reader.SetPrivateField("isPowerRequested", reader.Read<System.Boolean>(), instance);
					break;
					case "parentShip":
					instance = (StorageRoomAnimal)reader.SetPrivateField("parentShip", reader.Read<Ship>(), instance);
					break;
					case "gridSize":
					instance = (StorageRoomAnimal)reader.SetPrivateField("gridSize", reader.Read<UnityEngine.Vector2Int>(), instance);
					break;
					case "roomData":
						instance.roomData = reader.Read<StorageRoomBaseData>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_StorageRoomAnimalArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_StorageRoomAnimalArray() : base(typeof(StorageRoomAnimal[]), ES3UserType_StorageRoomAnimal.Instance)
		{
			Instance = this;
		}
	}
}