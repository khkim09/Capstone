using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("id", "weaponName", "description", "damage", "cooldownPerSecond", "weaponIcon", "blueprintSprites", "flattenedSprites", "weaponType", "effectData", "effectPower", "cost", "warheadType", "projectileId")]
	public class ES3UserType_ShipWeaponData : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_ShipWeaponData() : base(typeof(ShipWeaponData)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (ShipWeaponData)obj;
			
			writer.WriteProperty("id", instance.id, ES3Type_int.Instance);
			writer.WriteProperty("weaponName", instance.weaponName, ES3Type_string.Instance);
			writer.WriteProperty("description", instance.description, ES3Type_string.Instance);
			writer.WriteProperty("damage", instance.damage, ES3Type_float.Instance);
			writer.WriteProperty("cooldownPerSecond", instance.cooldownPerSecond, ES3Type_float.Instance);
			writer.WritePropertyByRef("weaponIcon", instance.weaponIcon);
			writer.WriteProperty("blueprintSprites", instance.blueprintSprites, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(UnityEngine.Sprite[,])));
			writer.WriteProperty("flattenedSprites", instance.flattenedSprites, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(UnityEngine.Sprite[])));
			writer.WritePropertyByRef("weaponType", instance.weaponType);
			writer.WritePropertyByRef("effectData", instance.effectData);
			writer.WriteProperty("effectPower", instance.effectPower, ES3Type_float.Instance);
			writer.WriteProperty("cost", instance.cost, ES3Type_int.Instance);
			writer.WritePropertyByRef("warheadType", instance.warheadType);
			writer.WriteProperty("projectileId", instance.projectileId, ES3Type_int.Instance);
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (ShipWeaponData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "id":
						instance.id = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "weaponName":
						instance.weaponName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "description":
						instance.description = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "damage":
						instance.damage = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "cooldownPerSecond":
						instance.cooldownPerSecond = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "weaponIcon":
						instance.weaponIcon = reader.Read<UnityEngine.Sprite>(ES3Type_Sprite.Instance);
						break;
					case "blueprintSprites":
						instance.blueprintSprites = reader.Read<UnityEngine.Sprite[,]>();
						break;
					case "flattenedSprites":
						instance.flattenedSprites = reader.Read<UnityEngine.Sprite[]>();
						break;
					case "weaponType":
						instance.weaponType = reader.Read<ShipWeaponTypeData>();
						break;
					case "effectData":
						instance.effectData = reader.Read<WeaponEffectData>();
						break;
					case "effectPower":
						instance.effectPower = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "cost":
						instance.cost = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "warheadType":
						instance.warheadType = reader.Read<WarheadTypeData>();
						break;
					case "projectileId":
						instance.projectileId = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_ShipWeaponDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_ShipWeaponDataArray() : base(typeof(ShipWeaponData[]), ES3UserType_ShipWeaponData.Instance)
		{
			Instance = this;
		}
	}
}