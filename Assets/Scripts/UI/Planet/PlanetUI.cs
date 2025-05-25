using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetUI : MonoBehaviour
{
    [SerializeField] private Image portrait;

    [SerializeField] private Image planetIllust;

    [SerializeField] private List<PlanetSpriteInfo> spritesInfo;

    private Dictionary<PlanetRace, PlanetSpriteInfo> spriteDict;


    private void Awake()
    {
        spriteDict = new Dictionary<PlanetRace, PlanetSpriteInfo>();

        foreach (PlanetSpriteInfo info in spritesInfo)
            if (!spriteDict.ContainsKey(info.key))
                spriteDict.Add(info.key, info);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        PlanetRace curPlanetRace = GameManager.Instance.WhereIAm().planetRace;

        if (spriteDict.TryGetValue(curPlanetRace, out PlanetSpriteInfo info))
        {
            portrait.sprite = info.portraitValue;
            planetIllust.sprite = info.planetIllustValue;
        }
        else
        {
            Debug.LogWarning($"PlanetRace {curPlanetRace}에 대한 스프라이트 정보가 없습니다.");
        }
    }

    public void OnExitButtonClicked()
    {
        // 1. 행성 떠날 때 WorldNodeDataList clear
        GameManager.Instance.WorldNodeDataList.Clear();

        // 2. WarpNodeDataList clear
        GameManager.Instance.WarpNodeDataList.Clear();

        // 3. currentWorldNodePosition 현재 행성 위치로
        // currentWorldNodePosition = GameManager.Instance.PlanetDataList[GameManager.Instance.CurrentWarpTargetPlanetId].normalizedPosition;


        SceneChanger.Instance.LoadScene("Idle");
    }
}

[Serializable]
public class PlanetSpriteInfo
{
    public PlanetRace key;
    public Sprite portraitValue;
    public Sprite planetIllustValue;
}
