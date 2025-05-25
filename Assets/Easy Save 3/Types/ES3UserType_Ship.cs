using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("isPlayerShip", "shipName", "gridSize", "allRooms", "allWeapons", "allCrews", "unUsedItems", "doorData", "doorLevel", "outerHullData", "outerHullPrefab", "outerHulls", "currentStats", "systems", "outerHullSystem", "weaponSystem", "oxygenSystem", "crewSystem", "hitpointSystem", "moraleSystem", "powerSystem", "storageSystem", "shieldSystem", "backupRoomDatas", "outerHullList")]
	public class ES3UserType_Ship : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Ship() : base(typeof(Ship)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (Ship)obj;
			
			writer.WriteProperty("isPlayerShip", instance.isPlayerShip, ES3Type_bool.Instance);
			writer.WriteProperty("shipName", instance.shipName, ES3Type_string.Instance);
			writer.WritePrivateField("gridSize", instance);
			writer.WriteProperty("allRooms", instance.allRooms, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<Room>)));
			writer.WriteProperty("allWeapons", instance.allWeapons, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<ShipWeapon>)));
			writer.WriteProperty("allCrews", instance.allCrews, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<CrewMember>)));
			writer.WriteProperty("unUsedItems", instance.unUsedItems, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.HashSet<EquipmentItem>)));
			writer.WritePrivateFieldByRef("doorData", instance);
			writer.WritePrivateField("doorLevel", instance);
			writer.WritePropertyByRef("outerHullData", instance.outerHullData);
			writer.WritePropertyByRef("outerHullPrefab", instance.outerHullPrefab);
			writer.WritePropertyByRef("outerHulls", instance.outerHulls);
			writer.WriteProperty("currentStats", instance.currentStats, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.Dictionary<ShipStat, System.Single>)));
			writer.WritePrivateField("systems", instance);
			writer.WritePrivateField("outerHullSystem", instance);
			writer.WritePrivateField("weaponSystem", instance);
			writer.WritePrivateField("oxygenSystem", instance);
			writer.WritePrivateField("crewSystem", instance);
			writer.WritePrivateField("hitpointSystem", instance);
			writer.WritePrivateField("moraleSystem", instance);
			writer.WritePrivateField("powerSystem", instance);
			writer.WritePrivateField("storageSystem", instance);
			writer.WritePrivateField("shieldSystem", instance);
			writer.WriteProperty("backupRoomDatas", instance.backupRoomDatas, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<RoomBackupData>)));
			writer.WriteProperty("outerHullList", instance.outerHullList, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<OuterHull>)));
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (Ship)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "isPlayerShip":
						instance.isPlayerShip = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "shipName":
						instance.shipName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "gridSize":
					instance = (Ship)reader.SetPrivateField("gridSize", reader.Read<UnityEngine.Vector2Int>(), instance);
					break;
					case "allRooms":
						instance.allRooms = reader.Read<System.Collections.Generic.List<Room>>();
						break;
					case "allWeapons":
						instance.allWeapons = reader.Read<System.Collections.Generic.List<ShipWeapon>>();
						break;
					case "allCrews":
						instance.allCrews = reader.Read<System.Collections.Generic.List<CrewMember>>();
						break;
					case "unUsedItems":
						instance.unUsedItems = reader.Read<System.Collections.Generic.HashSet<EquipmentItem>>();
						break;
					case "doorData":
					instance = (Ship)reader.SetPrivateField("doorData", reader.Read<DoorData>(), instance);
					break;
					case "doorLevel":
					instance = (Ship)reader.SetPrivateField("doorLevel", reader.Read<System.Int32>(), instance);
					break;
					case "outerHullData":
						instance.outerHullData = reader.Read<OuterHullData>(ES3UserType_OuterHullData.Instance);
						break;
					case "outerHullPrefab":
						instance.outerHullPrefab = reader.Read<UnityEngine.GameObject>(ES3Type_GameObject.Instance);
						break;
					case "outerHulls":
						instance.outerHulls = reader.Read<UnityEngine.Transform>(ES3Type_Transform.Instance);
						break;
					case "currentStats":
						instance.currentStats = reader.Read<System.Collections.Generic.Dictionary<ShipStat, System.Single>>();
						break;
					case "systems":
					instance = (Ship)reader.SetPrivateField("systems", reader.Read<System.Collections.Generic.Dictionary<System.Type, ShipSystem>>(), instance);
					break;
					case "outerHullSystem":
					instance = (Ship)reader.SetPrivateField("outerHullSystem", reader.Read<OuterHullSystem>(), instance);
					break;
					case "weaponSystem":
					instance = (Ship)reader.SetPrivateField("weaponSystem", reader.Read<WeaponSystem>(), instance);
					break;
					case "oxygenSystem":
					instance = (Ship)reader.SetPrivateField("oxygenSystem", reader.Read<OxygenSystem>(), instance);
					break;
					case "crewSystem":
					instance = (Ship)reader.SetPrivateField("crewSystem", reader.Read<CrewSystem>(), instance);
					break;
					case "hitpointSystem":
					instance = (Ship)reader.SetPrivateField("hitpointSystem", reader.Read<HitPointSystem>(), instance);
					break;
					case "moraleSystem":
					instance = (Ship)reader.SetPrivateField("moraleSystem", reader.Read<MoraleSystem>(), instance);
					break;
					case "powerSystem":
					instance = (Ship)reader.SetPrivateField("powerSystem", reader.Read<PowerSystem>(), instance);
					break;
					case "storageSystem":
					instance = (Ship)reader.SetPrivateField("storageSystem", reader.Read<StorageSystem>(), instance);
					break;
					case "shieldSystem":
					instance = (Ship)reader.SetPrivateField("shieldSystem", reader.Read<ShieldSystem>(), instance);
					break;
					case "backupRoomDatas":
						instance.backupRoomDatas = reader.Read<System.Collections.Generic.List<RoomBackupData>>();
						break;
					case "outerHullList":
						instance.outerHullList = reader.Read<System.Collections.Generic.List<OuterHull>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_ShipArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_ShipArray() : base(typeof(Ship[]), ES3UserType_Ship.Instance)
		{
			Instance = this;
		}
	}
}