using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 엔딩 평가 항목 4개를 등급 이름 + 설명 + 스프라이트로 출력하는 클래스.
/// Sprite 리스트는 인덱스 0 = 최하, 5 = 최상 으로 정렬되어 있어야 함.
/// </summary>
public class EndingResultDisplay : MonoBehaviour
{
    [Header("이미지 오브젝트")]
    [SerializeField] private Image endingImage1;
    [SerializeField] private Image endingImage2;
    [SerializeField] private Image endingImage3;
    [SerializeField] private Image endingImage4;

    [Header("텍스트 오브젝트")]
    [SerializeField] private TextMeshProUGUI endingText1;
    [SerializeField] private TextMeshProUGUI endingText2;
    [SerializeField] private TextMeshProUGUI endingText3;
    [SerializeField] private TextMeshProUGUI endingText4;

    [Header("등급별 스프라이트 리스트")]
    [SerializeField] private List<Sprite> comaRankSprites;
    [SerializeField] private List<Sprite> pirateRankSprites;
    [SerializeField] private List<Sprite> questRankSprites;
    [SerializeField] private List<Sprite> mysteryRankSprites;

    private void Start()
    {
        DisplayResults();
    }

    /// <summary>
    /// 외부 호출용 함수입니다.
    /// </summary>
    public void ForceDisplay()
    {
        DisplayResults();
    }

    private void DisplayResults()
    {
        var data = GameManager.Instance.playerData;

        int comaIndex = GetCOMARankIndex(data.COMA);
        string comaTitle = GetCOMARankName(comaIndex);
        string comaDesc = comaDescriptions[comaIndex];
        endingText1.text = $"{comaTitle}\n{comaDesc}";
        endingImage1.sprite = comaRankSprites[comaIndex];

        int pirateIndex = GetPirateRankIndex(data.pirateDefeated);
        string pirateTitle = GetPirateRankName(pirateIndex);
        string pirateDesc = pirateDescriptions[pirateIndex];
        endingText2.text = $"{pirateTitle}\n{pirateDesc}";
        endingImage2.sprite = pirateRankSprites[pirateIndex];

        int questIndex = GetQuestRankIndex(data.questCleared);
        string questTitle = GetQuestRankName(questIndex);
        string questDesc = questDescriptions[questIndex];
        endingText3.text = $"{questTitle}\n{questDesc}";
        endingImage3.sprite = questRankSprites[questIndex];

        int mysteryIndex = GetMysteryRankIndex(data.mysteryFound);
        string mysteryTitle = GetMysteryRankName(mysteryIndex);
        string mysteryDesc = mysteryDescriptions[mysteryIndex];
        endingText4.text = $"{mysteryTitle}\n{mysteryDesc}";
        endingImage4.sprite = mysteryRankSprites[mysteryIndex];
    }

    // 설명 문자열
    private readonly string[] comaDescriptions =
    {
        "물질에 종속되지 않는 삶을 추구합니다.",
        "부족하지는 않습니다.",
        "나름 불편하지는 않은 삶입니다.",
        "부지런히 움직이는 당신에게서 돈냄새가 나는 것 같습니다.",
        "망해도 당신의 손주까지는 걱정없을 정도입니다.",
        "온 우주에서 가장 부유한 사람을 묻는다면 당신을 뽑을 것입니다."
    };

    private readonly string[] pirateDescriptions =
    {
        "당신의 자비로움을 그들이 알아준다면 참 좋을 것 같습니다.",
        "걸어오는 싸움을 피하지는 않습니다.",
        "안전한 항해를 위해 미리 주변을 정리합니다.",
        "악한 자들을 처벌하자 내면의 폭력성이 고개를 듭니다.",
        "당신과 마주친 해적이 부리나케 도망갑니다.",
        "당신의 잔혹함과 명성이 온 우주에 퍼져나갑니다."
    };

    private readonly string[] questDescriptions =
    {
        "누구의 말도 듣지 않는 자신만의 길을 갑니다.",
        "겸사겸사 해봤습니다.",
        "부탁 정도라면 들어줄 수 있습니다.",
        "맡은 일은 나름 열심히 합니다.",
        "어디든 찾아갑니다. 보상이 있다면요.",
        "누구든 곤란한 일이 있다면 자연스럽게 당신을 떠올립니다."
    };

    private readonly string[] mysteryDescriptions =
    {
        "평범한 우주 여행을 경험해왔습니다.",
        "세상에는 이해를 벗어난 일이 일어나기도 합니다.",
        "이해할 수 없는 일들을 연달아 경험했습니다.",
        "운이 좋은건지, 나쁜건지 모르겠습니다.",
        "보이지 않는 손이 당신을 위험으로 밀어넣습니다.",
        "우주의 불확실성을 이해하기 시작합니다."
    };

    // 등급 인덱스
    private int GetCOMARankIndex(int amount)
    {
        if (amount >= 1000000) return 5;
        if (amount >= 500000) return 4;
        if (amount >= 250000) return 3;
        if (amount >= 100000) return 2;
        if (amount > 0) return 1;
        return 0;
    }

    private int GetPirateRankIndex(int count)
    {
        if (count >= 500) return 5;
        if (count >= 400) return 4;
        if (count >= 300) return 3;
        if (count >= 150) return 2;
        if (count >= 1) return 1;
        return 0;
    }

    private int GetQuestRankIndex(int count)
    {
        if (count >= 300) return 5;
        if (count >= 200) return 4;
        if (count >= 100) return 3;
        if (count >= 50) return 2;
        if (count >= 1) return 1;
        return 0;
    }

    private int GetMysteryRankIndex(int count)
    {
        if (count >= 5) return 5;
        if (count == 4) return 4;
        if (count == 3) return 3;
        if (count == 2) return 2;
        if (count == 1) return 1;
        return 0;
    }

    private string GetCOMARankName(int index)
    {
        return index switch
        {
            5 => "거상",
            4 => "수집가",
            3 => "부지런한 중산층",
            2 => "소상인",
            1 => "미니멀리스트",
            _ => "무소유"
        };
    }

    private string GetPirateRankName(int index)
    {
        return index switch
        {
            5 => "학살자",
            4 => "추적자",
            3 => "영웅",
            2 => "자경대원",
            1 => "정당방위",
            _ => "비폭력주의자"
        };
    }

    private string GetQuestRankName(int index)
    {
        return index switch
        {
            5 => "해결사",
            4 => "용병",
            3 => "훌륭한 일꾼",
            2 => "친절한 이웃",
            1 => "겸사겸사",
            _ => "독불장군"
        };
    }

    private string GetMysteryRankName(int index)
    {
        return index switch
        {
            5 => "전설의 탐험가",
            4 => "이상한 우주의 표류자",
            3 => "미지의 조우자",
            2 => "우연의 일치",
            1 => "어리둥절",
            _ => "천운"
        };
    }
}
