using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// TradingItemList는 TradingItemData 배열을 포함하는 컨테이너 클래스입니다.
/// JSON 데이터 파싱에 사용됩니다.
/// </summary>
[System.Serializable]
public class TradingItemList
{
    /// <summary>
    /// 로드된 TradingItemData 배열입니다.
    /// </summary>
    public TradingItemData[] items;
}

/// <summary>
/// TradeDataLoader는 JSON 파일에서 거래 가능한 아이템 데이터를 로드하는 기능을 제공합니다.
/// 로드된 데이터는 tradableItems 리스트에 저장됩니다.
/// </summary>
public class TradeDataLoader : MonoBehaviour
{
    /// <summary>
    /// Resources 폴더 내의 JSON 파일 이름 (확장자 제외).
    /// 현재 사용되지 않지만, 추후 파일 이름 지정에 활용할 수 있습니다.
    /// </summary>
    [SerializeField] private string jsonFileName = "PlanetTradeTest";

    /// <summary>
    /// 로드된 TradingItemData 데이터를 저장하는 리스트입니다.
    /// </summary>
    public List<TradingItemData> tradingItems = new List<TradingItemData>();

    /// <summary>
    /// 컴포넌트가 활성화될 때 JSON 데이터를 로드합니다.
    /// </summary>
    private void Awake()
    {
        LoadTradeData();
    }

    /// <summary>
    /// JSON 파일에서 거래 가능한 아이템 데이터를 로드합니다.
    /// 파일 경로: Assets/Data/PlanetTradeTest.json
    /// JSON 파일의 형식이 딕셔너리 형태일 경우, 불필요한 키를 제거하여 배열 형태로 변환 후 파싱합니다.
    /// </summary>
    private void LoadTradeData()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "Data", "PlanetTradeTest.json");

        if (!System.IO.File.Exists(path))
        {
            Debug.LogError("TradeDataLoader: JSON 파일을 찾을 수 없습니다. 경로: " + path);
            return;
        }

        string jsonText = System.IO.File.ReadAllText(path);
        string rawJson = jsonText.Trim();

        if (rawJson.StartsWith("{") && rawJson.EndsWith("}"))
        {
            rawJson = rawJson.Substring(1, rawJson.Length - 2);
        }

        string pattern = "\"\\d+\"\\s*:\\s*";
        string replaced = Regex.Replace(rawJson, pattern, "");

        string arrayJson = "{\"items\":[" + replaced + "]}";

        TradingItemList itemList = JsonUtility.FromJson<TradingItemList>(arrayJson);
        tradingItems = new List<TradingItemData>(itemList.items);

        Debug.Log("TradeDataLoader: " + tradingItems.Count + "개의 무역 아이템 데이터를 로드했습니다.");
    }
}
