using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class TradableItemList
{
    public TradableItem[] items;
}

public class TradeDataLoader : MonoBehaviour
{
    // Resources 폴더 내의 JSON 파일 이름 (확장자 제외)
    [SerializeField] private string jsonFileName = "행성";

    // 로드된 TradableItem 데이터를 리스트로 보관
    public List<TradableItem> tradableItems;

    private void Awake()
    {
        LoadTradeData();
    }

    private void LoadTradeData()
    {
        // Assets/Data 폴더 내의 "PlanetTrade.json" 파일 경로 설정
        string path = System.IO.Path.Combine(Application.dataPath, "Data", "PlanetTrade.json");

        if (!System.IO.File.Exists(path))
        {
            Debug.LogError("TradeDataLoader: JSON 파일을 찾을 수 없습니다. 경로: " + path);
            return;
        }

        // 파일 내용을 읽어옵니다.
        string jsonText = System.IO.File.ReadAllText(path);
        string rawJson = jsonText.Trim();

        // JSON이 딕셔너리 형태라면, 양 끝의 중괄호를 제거합니다.
        if (rawJson.StartsWith("{") && rawJson.EndsWith("}"))
        {
            rawJson = rawJson.Substring(1, rawJson.Length - 2);
        }

        // 정규표현식을 이용해 각 항목의 키 (예: "0":, "1": 등)를 제거합니다.
        string pattern = "\"\\d+\"\\s*:\\s*";
        string replaced = System.Text.RegularExpressions.Regex.Replace(rawJson, pattern, "");

        // 배열 형태의 JSON 문자열로 감쌉니다.
        string arrayJson = "{\"items\":[" + replaced + "]}";

        // 변환된 JSON 문자열을 파싱합니다.
        TradableItemList itemList = JsonUtility.FromJson<TradableItemList>(arrayJson);
        tradableItems = new List<TradableItem>(itemList.items);

        Debug.Log("TradeDataLoader: " + tradableItems.Count + "개의 무역 아이템 데이터를 로드했습니다.");
    }

}
