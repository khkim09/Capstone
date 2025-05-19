using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 엔딩 평가 항목 4개를 등급 이름 + 스프라이트로 출력하는 클래스.
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

    private void DisplayResults()
    {
        var data = GameManager.Instance.playerData;

        int comaIndex = GetCOMARankIndex(data.COMA);
        endingText1.text = GetCOMARankName(comaIndex);
        endingImage1.sprite = comaRankSprites[comaIndex];

        int pirateIndex = GetPirateRankIndex(data.pirateDefeated);
        endingText2.text = GetPirateRankName(pirateIndex);
        endingImage2.sprite = pirateRankSprites[pirateIndex];

        int questIndex = GetQuestRankIndex(data.questCleared);
        endingText3.text = GetQuestRankName(questIndex);
        endingImage3.sprite = questRankSprites[questIndex];

        int mysteryIndex = GetMysteryRankIndex(data.mysteryFound);
        endingText4.text = GetMysteryRankName(mysteryIndex);
        endingImage4.sprite = mysteryRankSprites[mysteryIndex];
    }

    // 등급 인덱스 (0~5)
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

    // 등급 이름 텍스트
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
