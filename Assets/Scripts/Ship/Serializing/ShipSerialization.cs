// using System;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;
//
// /// <summary>
// /// 함선 데이터 직렬화 및 역직렬화를 담당하는 클래스
// /// </summary>
// public static class ShipSerialization
// {
//     /// <summary>
//     /// 함선 데이터를 직렬화하기 위한 구조체
//     /// </summary>
//     [Serializable]
//     public class ShipSerializationData
//     {
//         // 함선 기본 정보
//         public string shipName;
//         public Vector2Int gridSize;
//
//         // 방 정보 목록
//         public List<RoomSerializationData> rooms = new();
//
//         // 문 정보는 방과 함께 저장됩니다.
//
//         // 무기 정보 목록
//         public List<WeaponSerializationData> weapons = new();
//
//         // 선원 정보 목록
//         public List<CrewSerializationData> crews = new();
//
//         // 외갑판 레벨
//         public int outerHullLevel;
//
//         // 문 레벨
//         public int doorLevel;
//     }
//
//     /// <summary>
//     /// 방 데이터를 직렬화하기 위한 구조체
//     /// </summary>
//     [Serializable]
//     public class RoomSerializationData
//     {
//         // 방 기본 정보
//         public RoomType roomType;
//         public string roomDataName; // ScriptableObject 이름
//         public int level;
//         public Vector2Int position;
//         public float currentHitPoints;
//         public RotationConstants.Rotation rotation;
//
//         // StorageRoom 특수 데이터
//         public StorageType? storageType; // null이면 일반 방
//
//         // 방에 연결된 문 정보
//         public List<DoorSerializationData> doors = new();
//     }
//
//     /// <summary>
//     /// 문 데이터를 직렬화하기 위한 구조체
//     /// </summary>
//     [Serializable]
//     public class DoorSerializationData
//     {
//         public Vector2Int originalPosition;
//         public DoorDirection originalDirection;
//         public DoorDirection currentDirection;
//     }
//
//     /// <summary>
//     /// 무기 데이터를 직렬화하기 위한 구조체
//     /// </summary>
//     [Serializable]
//     public class WeaponSerializationData
//     {
//         public string weaponDataName; // ScriptableObject 이름
//         public Vector2Int gridPosition;
//         public ShipWeaponAttachedDirection attachedDirection;
//     }
//
//     /// <summary>
//     /// 선원 데이터를 직렬화하기 위한 구조체
//     /// </summary>
//     [Serializable]
//     public class CrewSerializationData
//     {
//         public string crewName;
//         public CrewRace race;
//         public Vector2Int position;
//         public float currentHealth;
//         public float currentMorale;
//         public Dictionary<string, float> roomSkills; // 키: RoomType 이름, 값: 숙련도
//         public string equippedWeaponName; // 장착한 무기 (ScriptableObject 이름)
//         public string equippedArmorName; // 장착한 방어구 (ScriptableObject 이름)
//         public string equippedAccessoryName; // 장착한 액세서리 (ScriptableObject 이름)
//     }
//
//     /// <summary>
//     /// 함선 객체를 JSON 문자열로 직렬화
//     /// </summary>
//     /// <param name="ship">직렬화할 함선</param>
//     /// <returns>직렬화된 JSON 문자열</returns>
//     public static string SerializeShip(Ship ship)
//     {
//         ShipSerializationData data = new();
//
//         // 함선 기본 정보 복사
//         data.shipName = ship.shipName;
//         data.gridSize = ship.GetGridSize();
//         data.doorLevel = ship.GetDoorLevel();
//
//         // 방 정보 수집
//         foreach (Room room in ship.GetAllRooms())
//         {
//             RoomSerializationData roomData = new()
//             {
//                 roomType = room.GetRoomType(),
//                 roomDataName = room.GetRoomData().name,
//                 level = room.GetCurrentLevel(),
//                 position = room.position,
//                 currentHitPoints = room.currentHitPoints,
//                 rotation = room.currentRotation
//             };
//
//             // StorageRoom인 경우 특수 유형 저장
//             if (room is StorageRoomBase storageRoom) roomData.storageType = storageRoom.GetStorageType();
//
//             // 방에 연결된 문 정보 수집
//             foreach (Door door in room.GetConnectedDoors())
//             {
//                 DoorSerializationData doorData = new()
//                 {
//                     originalPosition = door.OriginalGridPosition,
//                     originalDirection = door.OriginalDirection,
//                     currentDirection = door.Direction
//                 };
//
//                 roomData.doors.Add(doorData);
//             }
//
//             data.rooms.Add(roomData);
//         }
//
//         // 무기 정보 수집
//         foreach (ShipWeapon weapon in ship.GetAllWeapons())
//         {
//             WeaponSerializationData weaponData = new()
//             {
//                 weaponDataName = weapon.weaponData.name,
//                 gridPosition = weapon.GetGridPosition(),
//                 attachedDirection = weapon.GetAttachedDirection()
//             };
//
//             data.weapons.Add(weaponData);
//         }
//
//         // 선원 정보 수집
//         foreach (CrewBase crew in ship.GetAllCrew())
//         {
//             CrewSerializationData crewData = new()
//             {
//                 crewName = crew.GetCrewName(),
//                 race = crew.GetRace(),
//                 position = crew.position,
//                 currentHealth = crew.GetCurrentHealth(),
//                 currentMorale = crew.GetMorale(),
//                 roomSkills = new Dictionary<string, float>()
//             };
//
//             // 각 방 타입별 숙련도 저장
//             foreach (RoomType roomType in Enum.GetValues(typeof(RoomType)))
//             {
//                 string roomTypeName = roomType.ToString();
//                 float skill = crew.GetRoomSkill(roomType);
//                 crewData.roomSkills[roomTypeName] = skill;
//             }
//
//             // 장비 정보 저장
//             if (crew.GetEquippedWeapon() != null)
//                 crewData.equippedWeaponName = crew.GetEquippedWeapon().name;
//
//             if (crew.GetEquippedArmor() != null)
//                 crewData.equippedArmorName = crew.GetEquippedArmor().name;
//
//             if (crew.GetEquippedAccessory() != null)
//                 crewData.equippedAccessoryName = crew.GetEquippedAccessory().name;
//
//             data.crews.Add(crewData);
//         }
//
//         // JSON으로 직렬화
//         string json = JsonConvert.SerializeObject(data, Formatting.Indented);
//         return json;
//     }
//
//     /// <summary>
//     /// JSON 문자열에서 함선 객체를 역직렬화
//     /// </summary>
//     /// <param name="json">역직렬화할 JSON 문자열</param>
//     /// <param name="ship">대상 함선 객체 (비어있어야 함)</param>
//     /// <returns>성공 여부</returns>
//     public static bool DeserializeShip(string json, Ship ship)
//     {
//         try
//         {
//             ShipSerializationData data = JsonConvert.DeserializeObject<ShipSerializationData>(json);
//
//             // 함선 기본 정보 복원
//             ship.shipName = data.shipName;
//             ship.gridSize = data.gridSize;
//
//             // 기존 방, 무기, 선원 제거
//             ClearShip(ship);
//
//             // 방 복원
//             foreach (RoomSerializationData roomData in data.rooms)
//             {
//                 // ScriptableObject 검색
//                 RoomData roomDataObj = Resources.Load<RoomData>("RoomData/" + roomData.roomDataName);
//                 if (roomDataObj == null)
//                 {
//                     Debug.LogError($"방 데이터를 찾을 수 없음: {roomData.roomDataName}");
//                     continue;
//                 }
//
//                 // 방 추가
//                 Room room = ship.AddRoom(roomData.level, roomDataObj, roomData.position);
//                 if (room == null)
//                 {
//                     Debug.LogError($"방 추가 실패: {roomData.roomType} at {roomData.position}");
//                     continue;
//                 }
//
//                 // 상태 복원
//                 room.currentHitPoints = roomData.currentHitPoints;
//                 room.currentRotation = roomData.rotation;
//
//                 // 특수 방 설정
//                 if (roomData.storageType.HasValue && room is StorageRoomBase storageRoom)
//                     storageRoom.SetStorageType(roomData.storageType.Value);
//
//                 // 문 복원은 방이 모두 추가된 후에 진행
//             }
//
//             // 문 복원
//             for (int i = 0; i < data.rooms.Count; i++)
//             {
//                 RoomSerializationData roomData = data.rooms[i];
//                 Room room = ship.GetRoomAtPosition(roomData.position);
//
//                 if (room == null) continue;
//
//                 foreach (DoorSerializationData doorData in roomData.doors)
//                 {
//                     // 문 추가
//                     DoorPosition doorPos = new(doorData.originalPosition, doorData.originalDirection);
//                     Door door = room.AddDoor(doorPos, ship.GetDoorData(), data.doorLevel);
//
//                     if (door != null)
//                     {
//                         // 문 상태 복원
//                         door.SetDirection(doorData.currentDirection);
//                         if (doorData.isOpen)
//                             door.TryOpenDoor(true);
//                     }
//                 }
//             }
//
//             // 무기 복원
//             foreach (WeaponSerializationData weaponData in data.weapons)
//             {
//                 // ScriptableObject 검색
//                 ShipWeaponData weaponDataObj = Resources.Load<ShipWeaponData>("Weapons/" + weaponData.weaponDataName);
//                 if (weaponDataObj == null)
//                 {
//                     Debug.LogError($"무기 데이터를 찾을 수 없음: {weaponData.weaponDataName}");
//                     continue;
//                 }
//
//                 // 무기 생성 및 배치
//                 ShipWeapon weapon = ship.AddWeapon(weaponDataObj, weaponData.gridPosition);
//                 if (weapon != null)
//                 {
//                     weapon.SetAttachedDirection(weaponData.attachedDirection);
//                     weapon.SetRemainingCooldown(weaponData.cooldownRemaining);
//                 }
//             }
//
//             // 선원 복원
//             foreach (CrewSerializationData crewData in data.crews)
//             {
//                 // 선원 생성
//                 CrewBase crew = CrewFactory.CreateCrew(crewData.race, crewData.crewName);
//                 if (crew == null)
//                 {
//                     Debug.LogError($"선원 생성 실패: {crewData.crewName}");
//                     continue;
//                 }
//
//                 // 선원 상태 복원
//                 crew.position = crewData.position;
//                 crew.SetCurrentHealth(crewData.currentHealth);
//                 crew.SetMorale(crewData.currentMorale);
//
//                 // 숙련도 복원
//                 foreach (KeyValuePair<string, float> skill in crewData.roomSkills)
//                     if (Enum.TryParse(skill.Key, out RoomType roomType))
//                         crew.SetRoomSkill(roomType, skill.Value);
//
//                 // 장비 복원
//                 if (!string.IsNullOrEmpty(crewData.equippedWeaponName))
//                 {
//                     WeaponItemData weapon =
//                         Resources.Load<WeaponItemData>("Items/Weapons/" + crewData.equippedWeaponName);
//                     if (weapon != null)
//                         crew.EquipWeapon(weapon);
//                 }
//
//                 if (!string.IsNullOrEmpty(crewData.equippedArmorName))
//                 {
//                     ArmorItemData armor = Resources.Load<ArmorItemData>("Items/Armors/" + crewData.equippedArmorName);
//                     if (armor != null)
//                         crew.EquipArmor(armor);
//                 }
//
//                 if (!string.IsNullOrEmpty(crewData.equippedAccessoryName))
//                 {
//                     AccessoryItemData accessory =
//                         Resources.Load<AccessoryItemData>("Items/Accessories/" + crewData.equippedAccessoryName);
//                     if (accessory != null)
//                         crew.EquipAccessory(accessory);
//                 }
//
//                 // 함선에 선원 추가
//                 ship.AddCrew(crew);
//             }
//
//             // 스탯 재계산
//             ship.RecalculateAllStats();
//
//             return true;
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"함선 역직렬화 오류: {e.Message}\n{e.StackTrace}");
//             return false;
//         }
//     }
//
//     /// <summary>
//     /// 함선을 파일로 저장
//     /// </summary>
//     /// <param name="ship">저장할 함선</param>
//     /// <param name="filePath">저장 경로</param>
//     /// <returns>성공 여부</returns>
//     public static bool SaveShipToFile(Ship ship, string filePath)
//     {
//         try
//         {
//             string json = SerializeShip(ship);
//             File.WriteAllText(filePath, json);
//             return true;
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"함선 저장 오류: {e.Message}");
//             return false;
//         }
//     }
//
//     /// <summary>
//     /// 파일에서 함선 불러오기
//     /// </summary>
//     /// <param name="filePath">파일 경로</param>
//     /// <param name="ship">대상 함선 객체</param>
//     /// <returns>성공 여부</returns>
//     public static bool LoadShipFromFile(string filePath, Ship ship)
//     {
//         try
//         {
//             if (!File.Exists(filePath))
//             {
//                 Debug.LogError($"함선 파일을 찾을 수 없음: {filePath}");
//                 return false;
//             }
//
//             string json = File.ReadAllText(filePath);
//             return DeserializeShip(json, ship);
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"함선 로드 오류: {e.Message}");
//             return false;
//         }
//     }
//
//     /// <summary>
//     /// 함선의 모든 콘텐츠 제거 (초기화)
//     /// </summary>
//     /// <param name="ship">초기화할 함선</param>
//     private static void ClearShip(Ship ship)
//     {
//         // 모든 방 제거
//         List<Room> rooms = new(ship.GetAllRooms());
//         foreach (Room room in rooms) ship.RemoveRoom(room);
//
//         // 모든 무기 제거
//         int weaponCount = ship.GetWeaponCount();
//         for (int i = weaponCount - 1; i >= 0; i--) ship.RemoveWeapon(i);
//
//         // 모든 선원 제거
//         List<CrewBase> crews = new(ship.GetAllCrew());
//         foreach (CrewBase crew in crews) ship.RemoveCrew(crew);
//     }
// }


