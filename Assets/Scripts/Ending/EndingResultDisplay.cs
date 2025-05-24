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
        "ui.ending.comadescription.0".Localize(),
        "ui.ending.comadescription.1".Localize(),
        "ui.ending.comadescription.2".Localize(),
        "ui.ending.comadescription.3".Localize(),
        "ui.ending.comadescription.4".Localize(),
        "ui.ending.comadescription.5".Localize()
    };

    private readonly string[] pirateDescriptions =
    {
        "ui.ending.piratedescription.0".Localize(),
        "ui.ending.piratedescription.1".Localize(),
        "ui.ending.piratedescription.2".Localize(),
        "ui.ending.piratedescription.3".Localize(),
        "ui.ending.piratedescription.4".Localize(),
        "ui.ending.piratedescription.5".Localize()
    };

    private readonly string[] questDescriptions =
    {
        "ui.ending.questdescription.0".Localize(),
        "ui.ending.questdescription.1".Localize(),
        "ui.ending.questdescription.2".Localize(),
        "ui.ending.questdescription.3".Localize(),
        "ui.ending.questdescription.4".Localize(),
        "ui.ending.questdescription.5".Localize()
    };

    private readonly string[] mysteryDescriptions =
    {
        "ui.ending.mysterydescription.0".Localize(),
        "ui.ending.mysterydescription.1".Localize(),
        "ui.ending.mysterydescription.2".Localize(),
        "ui.ending.mysterydescription.3".Localize(),
        "ui.ending.mysterydescription.4".Localize(),
        "ui.ending.mysterydescription.5".Localize()
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
            5 => "ui.ending.comatitle.5".Localize(),
            4 => "ui.ending.comatitle.4".Localize(),
            3 => "ui.ending.comatitle.3".Localize(),
            2 => "ui.ending.comatitle.2".Localize(),
            1 => "ui.ending.comatitle.1".Localize(),
            _ => "ui.ending.comatitle.0".Localize()
        };
    }

    private string GetPirateRankName(int index)
    {
        return index switch
        {
            5 => "ui.ending.piratetitle.5".Localize(),
            4 => "ui.ending.piratetitle.4".Localize(),
            3 => "ui.ending.piratetitle.3".Localize(),
            2 => "ui.ending.piratetitle.2".Localize(),
            1 => "ui.ending.piratetitle.1".Localize(),
            _ => "ui.ending.piratetitle.0".Localize()
        };
    }

    private string GetQuestRankName(int index)
    {
        return index switch
        {
            5 => "ui.ending.questtitle.5".Localize(),
            4 => "ui.ending.questtitle.4".Localize(),
            3 => "ui.ending.questtitle.3".Localize(),
            2 => "ui.ending.questtitle.2".Localize(),
            1 => "ui.ending.questtitle.1".Localize(),
            _ => "ui.ending.questtitle.0".Localize()
        };
    }

    private string GetMysteryRankName(int index)
    {
        return index switch
        {
            5 => "ui.ending.mysterytitle.5".Localize(),
            4 => "ui.ending.mysterytitle.4".Localize(),
            3 => "ui.ending.mysterytitle.3".Localize(),
            2 => "ui.ending.mysterytitle.2".Localize(),
            1 => "ui.ending.mysterytitle.1".Localize(),
            _ => "ui.ending.mysterytitle.0".Localize()
        };
    }
}
