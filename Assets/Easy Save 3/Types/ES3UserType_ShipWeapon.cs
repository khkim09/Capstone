using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("weaponData", "firePoint", "gridPosition", "gridSize", "attachedDirection", "spriteRenderer", "hits", "totalDamageDealt", "isEnabled", "ownerShip")]
	public class ES3UserType_ShipWeapon : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_ShipWeapon() : base(typeof(ShipWeapon)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (ShipWeapon)obj;
			
			writer.WritePropertyByRef("weaponData", instance.weaponData);
			writer.WritePrivateFieldByRef("firePoint", instance);
			writer.WritePrivateField("gridPosition", instance);
			writer.WriteProperty("gridSize", instance.gridSize, ES3Type_Vector2Int.Instance);
			writer.WritePrivateField("attachedDirection", instance);
			writer.WritePrivateFieldByRef("spriteRenderer", instance);
			writer.WritePrivateField("hits", instance);
			writer.WritePrivateField("totalDamageDealt", instance);
			writer.WritePrivateField("isEnabled", instance);
			writer.WritePrivateFieldByRef("ownerShip", instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (ShipWeapon)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "weaponData":
						instance.weaponData = reader.Read<ShipWeaponData>();
						break;
					case "firePoint":
					instance = (ShipWeapon)reader.SetPrivateField("firePoint", reader.Read<UnityEngine.Transform>(), instance);
					break;
					case "gridPosition":
					instance = (ShipWeapon)reader.SetPrivateField("gridPosition", reader.Read<UnityEngine.Vector2Int>(), instance);
					break;
					case "gridSize":
						instance.gridSize = reader.Read<UnityEngine.Vector2Int>(ES3Type_Vector2Int.Instance);
						break;
					case "attachedDirection":
					instance = (ShipWeapon)reader.SetPrivateField("attachedDirection", reader.Read<ShipWeaponAttachedDirection>(), instance);
					break;
					case "spriteRenderer":
					instance = (ShipWeapon)reader.SetPrivateField("spriteRenderer", reader.Read<UnityEngine.SpriteRenderer>(), instance);
					break;
					case "hits":
					instance = (ShipWeapon)reader.SetPrivateField("hits", reader.Read<System.Int32>(), instance);
					break;
					case "totalDamageDealt":
					instance = (ShipWeapon)reader.SetPrivateField("totalDamageDealt", reader.Read<System.Single>(), instance);
					break;
					case "isEnabled":
					instance = (ShipWeapon)reader.SetPrivateField("isEnabled", reader.Read<System.Boolean>(), instance);
					break;
					case "ownerShip":
					instance = (ShipWeapon)reader.SetPrivateField("ownerShip", reader.Read<Ship>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_ShipWeaponArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_ShipWeaponArray() : base(typeof(ShipWeapon[]), ES3UserType_ShipWeapon.Instance)
		{
			Instance = this;
		}
	}
}