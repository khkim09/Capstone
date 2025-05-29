using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TeleportEffectHandler : ISpecialEffectHandler
{
    public void HandleEffect(EventOutcome outcome)
    {
        int index = Random.Range(0, GameManager.Instance.PlanetDataList.Count);
        PlanetData targetPlanet = GameManager.Instance.PlanetDataList[index];

        GameManager.Instance.ClearCurrentWarpMap();
        GameManager.Instance.SetCurrentWarpTargetPlanetId(targetPlanet.planetId);

        GameManager.Instance.WorldNodeDataList.Clear();
        // 함선 위치 행성으로 텔레포트
        Vector2 planetPos = targetPlanet.normalizedPosition;
        Vector2 newPosition = new(
            Mathf.Clamp01(planetPos.x),
            Mathf.Clamp01(planetPos.y)
        );
        GameManager.Instance.normalizedPlayerPosition = newPosition;

        GameManager.Instance.LandOnPlanet();
    }
}
