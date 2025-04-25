using System;
using UnityEngine;

namespace ES3Types
{
    [UnityEngine.Scripting.Preserve]
    [ES3PropertiesAttribute("shipName", "gridSize", "allRooms", "doorData", "doorLevel", "systems", "testRoomPrefab1",
        "testRoomPrefab2", "testRoomPrefab3", "backupRooms", "backupRoomDatas")]
    public class ES3UserType_Ship : ES3ComponentType
    {
        public static ES3Type Instance = null;

        public ES3UserType_Ship() : base(typeof(Ship))
        {
            Instance = this;
            priority = 1;
        }


        protected override void WriteComponent(object obj, ES3Writer writer)
        {
            Ship instance = (Ship)obj;

            writer.WriteProperty("shipName", instance.shipName, ES3Type_string.Instance);
            writer.WritePrivateField("gridSize", instance);
            writer.WriteProperty("allRooms", instance.allRooms,
                ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<Room>)));
            writer.WritePrivateFieldByRef("doorData", instance);
            writer.WritePrivateField("doorLevel", instance);
            writer.WritePrivateField("systems", instance);
            writer.WritePropertyByRef("testRoomPrefab1", instance.testRoomPrefab1);
            writer.WritePropertyByRef("testRoomPrefab2", instance.testRoomPrefab2);
            writer.WritePropertyByRef("testRoomPrefab3", instance.testRoomPrefab3);
            writer.WriteProperty("backupRoomDatas", instance.backupRoomDatas,
                ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<RoomBackupData>)));
        }

        protected override void ReadComponent<T>(ES3Reader reader, object obj)
        {
            Ship instance = (Ship)obj;
            foreach (string propertyName in reader.Properties)
                switch (propertyName)
                {
                    case "shipName":
                        instance.shipName = reader.Read<string>(ES3Type_string.Instance);
                        break;
                    case "gridSize":
                        instance = (Ship)reader.SetPrivateField("gridSize", reader.Read<Vector2Int>(), instance);
                        break;
                    case "allRooms":
                        instance.allRooms = reader.Read<System.Collections.Generic.List<Room>>();
                        break;
                    case "doorData":
                        instance = (Ship)reader.SetPrivateField("doorData", reader.Read<DoorData>(), instance);
                        break;
                    case "doorLevel":
                        instance = (Ship)reader.SetPrivateField("doorLevel", reader.Read<int>(), instance);
                        break;
                    case "systems":
                        instance = (Ship)reader.SetPrivateField("systems",
                            reader.Read<System.Collections.Generic.Dictionary<Type, ShipSystem>>(), instance);
                        break;
                    case "testRoomPrefab1":
                        instance.testRoomPrefab1 = reader.Read<GameObject>(ES3Type_GameObject.Instance);
                        break;
                    case "testRoomPrefab2":
                        instance.testRoomPrefab2 = reader.Read<GameObject>(ES3Type_GameObject.Instance);
                        break;
                    case "testRoomPrefab3":
                        instance.testRoomPrefab3 = reader.Read<GameObject>(ES3Type_GameObject.Instance);
                        break;
                    case "backupRoomDatas":
                        instance.backupRoomDatas = reader.Read<System.Collections.Generic.List<RoomBackupData>>();
                        break;
                    default:
                        reader.Skip();
                        break;
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
