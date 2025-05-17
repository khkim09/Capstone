using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선 생성 및 관리를 담당하는 팩토리 클래스
/// 함선 인스턴스 생성 및 템플릿 관리 기능을 제공합니다.
/// </summary>
public class ShipFactory : MonoBehaviour
{
    [SerializeField] private GameObject shipPrefab; // 기본 함선 프리팹

    private void Awake()
    {
    }


    /// <summary>
    /// 빈 함선 인스턴스 생성
    /// </summary>
    /// <param name="shipName">함선 이름 (옵션)</param>
    /// <returns>생성된 함선 객체</returns>
    public Ship CreateEmptyShipInstance(string shipName = "Ship")
    {
        GameObject shipObject = Instantiate(shipPrefab);
        shipObject.name = shipName;

        Ship ship = shipObject.GetComponent<Ship>();
        if (ship == null)
            ship = shipObject.AddComponent<Ship>();

        ship.shipName = shipName;
        return ship;
    }

    /// <summary>
    /// 템플릿을 기반으로 함선 생성
    /// </summary>
    /// <param name="template">함선 템플릿</param>
    /// <returns>생성된 함선 객체</returns>
    public Ship CreateShipFromTemplate(ShipTemplate template)
    {
        Ship ship = CreateEmptyShipInstance(template.shipName);

        // 템플릿에서 기본 룸 추가
        foreach (ShipTemplate.RoomTemplate roomTemplate in template.startingRooms)
        {
            Room room = GameObjectFactory.Instance.RoomFactory.CreateRoomInstance(
                roomTemplate.roomType,
                roomTemplate.level);

            ship.AddRoom(
                room,
                roomTemplate.position,
                roomTemplate.rotation);
        }

        // 템플릿에서 기본 무기 추가
        foreach (ShipTemplate.WeaponTemplate weaponTemplate in template.startingWeapons)
            ship.AddWeapon(
                weaponTemplate.weaponId,
                weaponTemplate.position,
                weaponTemplate.direction);

        // 템플릿에서 기본 승무원 추가
        foreach (ShipTemplate.CrewTemplate crewTemplate in template.startingCrew)
        {
            CrewBase crew = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(
                crewTemplate.crewRace,
                crewTemplate.crewName);

            if (crew is CrewMember crewMember)
                ship.AddCrew(crewMember);
        }

        return ship;
    }

    /// <summary>
    /// 기존 함선을 복제하여 새 인스턴스 생성
    /// </summary>
    /// <param name="originalShip">복제할 원본 함선</param>
    /// <returns>복제된 함선 인스턴스</returns>
    public Ship CloneShip(Ship originalShip)
    {
        if (originalShip == null)
            return null;

        Ship newShip = CreateEmptyShipInstance(originalShip.shipName);

        // 룸 복제
        foreach (Room room in originalShip.GetAllRooms())
        {
            Room clonedRoom = GameObjectFactory.Instance.RoomFactory.CreateRoomObject(room);
            newShip.AddRoom(clonedRoom);
        }

        // 무기 복제
        foreach (ShipWeapon weapon in originalShip.GetAllWeapons())
        {
            ShipWeapon clonedWeapon = GameObjectFactory.Instance.CreateWeaponObject(weapon);
            newShip.AddWeapon(clonedWeapon);
        }

        // 승무원 복제
        foreach (CrewBase crew in originalShip.GetAllCrew())
            if (crew is CrewMember crewMember)
            {
                CrewBase clonedCrew = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(
                    crewMember.race,
                    crewMember.crewName);

                if (clonedCrew is CrewMember clonedCrewMember)
                    newShip.AddCrew(clonedCrewMember);
            }

        return newShip;
    }

    /// <summary>
    /// 저장된 함선 데이터에서 함선 복원
    /// </summary>
    /// <param name="savePath">저장 파일 경로</param>
    /// <returns>복원된 함선 또는 null</returns>
    public Ship LoadShipFromSave(string savePath)
    {
        Ship ship = CreateEmptyShipInstance("Loaded Ship");

        bool success = ShipSerialization.RestoreShip(savePath, ship);

        if (!success)
        {
            Destroy(ship.gameObject);
            return null;
        }

        return ship;
    }
}

/// <summary>
/// 함선 템플릿 정의 클래스
/// 사전 정의된 함선 구성을 설정합니다.
/// </summary>
[System.Serializable]
public class ShipTemplate
{
    public int templateId;
    public string shipName = "Template Ship";
    public string description;
    public int baseHitPoints = 100;

    [Header("Starting Rooms")] public List<RoomTemplate> startingRooms = new();

    [Header("Starting Weapons")] public List<WeaponTemplate> startingWeapons = new();

    [Header("Starting Crew")] public List<CrewTemplate> startingCrew = new();

    [System.Serializable]
    public class RoomTemplate
    {
        public RoomType roomType;
        public int level = 1;
        public Vector2Int position;
        public Constants.Rotations.Rotation rotation;
    }

    [System.Serializable]
    public class WeaponTemplate
    {
        public int weaponId;
        public Vector2Int position;
        public ShipWeaponAttachedDirection direction = ShipWeaponAttachedDirection.North;
    }

    [System.Serializable]
    public class CrewTemplate
    {
        public CrewRace crewRace;
        public string crewName = "";
    }
}
