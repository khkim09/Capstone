using System;
using UnityEngine;

namespace ES3Types
{
    [UnityEngine.Scripting.Preserve]
    [ES3PropertiesAttribute("itemState", "itemData", "amount", "frameOffsetY", "rotation", "boxSprites",
        "parentStorage", "gridPosition", "cachedPrice", "priceInitialized")]
    public class ES3UserType_TradingItem : ES3ComponentType
    {
        public static ES3Type Instance = null;

        public ES3UserType_TradingItem() : base(typeof(TradingItem))
        {
            Instance = this;
            priority = 1;
        }


        protected override void WriteComponent(object obj, ES3Writer writer)
        {
            TradingItem instance = (TradingItem)obj;

            writer.WritePrivateField("itemState", instance);
            writer.WritePrivateFieldByRef("itemData", instance);
            writer.WriteProperty("amount", instance.amount, ES3Type_int.Instance);
            writer.WritePrivateField("frameOffsetY", instance);
            writer.WriteProperty("rotation", instance.rotation,
                ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(Constants.Rotations.Rotation)));
            writer.WritePrivateField("boxSprites", instance);
            writer.WritePrivateFieldByRef("parentStorage", instance);
            writer.WriteProperty("gridPosition", instance.gridPosition, ES3Type_Vector2Int.Instance);
            writer.WritePrivateField("cachedPrice", instance);
            writer.WritePrivateField("priceInitialized", instance);
        }

        protected override void ReadComponent<T>(ES3Reader reader, object obj)
        {
            TradingItem instance = (TradingItem)obj;
            foreach (string propertyName in reader.Properties)
                switch (propertyName)
                {
                    case "itemState":
                        instance = (TradingItem)reader.SetPrivateField("itemState", reader.Read<ItemState>(), instance);
                        break;
                    case "itemData":
                        instance = (TradingItem)reader.SetPrivateField("itemData", reader.Read<TradingItemData>(),
                            instance);
                        break;
                    case "amount":
                        instance.amount = reader.Read<int>(ES3Type_int.Instance);
                        break;
                    case "frameOffsetY":
                        instance = (TradingItem)reader.SetPrivateField("frameOffsetY", reader.Read<float>(), instance);
                        break;
                    case "rotation":
                        instance.rotation = reader.Read<Constants.Rotations.Rotation>();
                        break;
                    case "boxSprites":
                        instance = (TradingItem)reader.SetPrivateField("boxSprites", reader.Read<Sprite[]>(), instance);
                        break;
                    case "parentStorage":
                        instance = (TradingItem)reader.SetPrivateField("parentStorage", reader.Read<StorageRoomBase>(),
                            instance);
                        break;
                    case "gridPosition":
                        instance.gridPosition = reader.Read<Vector2Int>(ES3Type_Vector2Int.Instance);
                        break;
                    case "cachedPrice":
                        instance = (TradingItem)reader.SetPrivateField("cachedPrice", reader.Read<Nullable<float>>(),
                            instance);
                        break;
                    case "priceInitialized":
                        instance = (TradingItem)reader.SetPrivateField("priceInitialized", reader.Read<bool>(),
                            instance);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
        }
    }


    public class ES3UserType_TradingItemArray : ES3ArrayType
    {
        public static ES3Type Instance;

        public ES3UserType_TradingItemArray() : base(typeof(TradingItem[]), ES3UserType_TradingItem.Instance)
        {
            Instance = this;
        }
    }
}
