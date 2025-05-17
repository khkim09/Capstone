using System;
using UnityEngine;

namespace ES3Types
{
    [UnityEngine.Scripting.Preserve]
    [ES3PropertiesAttribute("currentRoomLevelData", "position", "currentLevel", "currentHitPoints", "roomType",
        "isDamageable", "connectedDoors", "currentRotation", "roomParticles", "roomSound", "crewInRoom", "isActive",
        "isPowered", "isPowerRequested", "parentShip", "gridSize", "damageCondition", "roomData")]
    public class ES3UserType_MedBayRoom : ES3ComponentType
    {
        public static ES3Type Instance = null;

        public ES3UserType_MedBayRoom() : base(typeof(MedBayRoom))
        {
            Instance = this;
            priority = 1;
        }


        protected override void WriteComponent(object obj, ES3Writer writer)
        {
            MedBayRoom instance = (MedBayRoom)obj;

            writer.WritePrivateField("currentRoomLevelData", instance);
            writer.WriteProperty("position", instance.position, ES3Type_Vector2Int.Instance);
            writer.WritePrivateField("currentLevel", instance);
            writer.WriteProperty("currentHitPoints", instance.currentHitPoints, ES3Type_float.Instance);
            writer.WriteProperty("roomType", instance.roomType,
                ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(RoomType)));
            writer.WritePrivateField("isDamageable", instance);
            writer.WritePrivateField("connectedDoors", instance);
            writer.WriteProperty("currentRotation", instance.currentRotation,
                ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(Constants.Rotations.Rotation)));
            writer.WritePrivateFieldByRef("roomParticles", instance);
            writer.WritePrivateFieldByRef("roomSound", instance);
            writer.WriteProperty("crewInRoom", instance.crewInRoom,
                ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.List<CrewMember>)));
            writer.WriteProperty("isActive", instance.isActive, ES3Type_bool.Instance);
            writer.WritePrivateField("isPowered", instance);
            writer.WritePrivateField("isPowerRequested", instance);
            writer.WritePropertyByRef("parentShip", instance.parentShip);
            writer.WritePrivateField("gridSize", instance);
            writer.WriteProperty("damageCondition", instance.damageCondition,
                ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(DamageLevel)));
            writer.WritePropertyByRef("roomData", instance.roomData);
        }

        protected override void ReadComponent<T>(ES3Reader reader, object obj)
        {
            MedBayRoom instance = (MedBayRoom)obj;
            foreach (string propertyName in reader.Properties)
                switch (propertyName)
                {
                    case "currentRoomLevelData":
                        instance = (MedBayRoom)reader.SetPrivateField("currentRoomLevelData",
                            reader.Read<MedBayRoomData.MedBayRoomLevel>(), instance);
                        break;
                    case "position":
                        instance.position = reader.Read<Vector2Int>(ES3Type_Vector2Int.Instance);
                        break;
                    case "currentLevel":
                        instance = (MedBayRoom)reader.SetPrivateField("currentLevel", reader.Read<int>(), instance);
                        break;
                    case "currentHitPoints":
                        instance.currentHitPoints = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "roomType":
                        instance.roomType = reader.Read<RoomType>();
                        break;
                    case "isDamageable":
                        instance = (MedBayRoom)reader.SetPrivateField("isDamageable", reader.Read<bool>(), instance);
                        break;
                    case "connectedDoors":
                        instance = (MedBayRoom)reader.SetPrivateField("connectedDoors",
                            reader.Read<System.Collections.Generic.List<Door>>(), instance);
                        break;
                    case "currentRotation":
                        instance.currentRotation = reader.Read<Constants.Rotations.Rotation>();
                        break;
                    case "roomParticles":
                        instance = (MedBayRoom)reader.SetPrivateField("roomParticles", reader.Read<ParticleSystem>(),
                            instance);
                        break;
                    case "roomSound":
                        instance = (MedBayRoom)reader.SetPrivateField("roomSound", reader.Read<AudioSource>(),
                            instance);
                        break;
                    case "crewInRoom":
                        instance.crewInRoom = reader.Read<System.Collections.Generic.List<CrewMember>>();
                        break;
                    case "isActive":
                        instance.isActive = reader.Read<bool>(ES3Type_bool.Instance);
                        break;
                    case "isPowered":
                        instance = (MedBayRoom)reader.SetPrivateField("isPowered", reader.Read<bool>(), instance);
                        break;
                    case "isPowerRequested":
                        instance = (MedBayRoom)reader.SetPrivateField("isPowerRequested", reader.Read<bool>(),
                            instance);
                        break;
                    case "parentShip":
                        instance.parentShip = reader.Read<Ship>(ES3UserType_Ship.Instance);
                        break;
                    case "gridSize":
                        instance = (MedBayRoom)reader.SetPrivateField("gridSize", reader.Read<Vector2Int>(), instance);
                        break;
                    case "damageCondition":
                        instance.damageCondition = reader.Read<DamageLevel>();
                        break;
                    case "roomData":
                        instance.roomData = reader.Read<MedBayRoomData>();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
        }
    }


    public class ES3UserType_MedBayRoomArray : ES3ArrayType
    {
        public static ES3Type Instance;

        public ES3UserType_MedBayRoomArray() : base(typeof(MedBayRoom[]), ES3UserType_MedBayRoom.Instance)
        {
            Instance = this;
        }
    }
}
