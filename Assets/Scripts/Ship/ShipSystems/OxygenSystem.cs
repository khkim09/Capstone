using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 산소 수치를 관리하는 시스템.
/// 산소 생성량과 소비량을 바탕으로 현재 산소 수치를 계산합니다.
/// </summary>
public class OxygenSystem : ShipSystem
{
    /// <summary>
    /// 현재 산소 수치 (% 단위, 0 ~ 100).
    /// </summary>
    private float currentOxygenRate;

    /// <summary>
    /// 현재 산소 레벨 (총 5단계)
    /// </summary>
    private OxygenLevel currentOxygenLevel;


    /// <summary>
    /// 시스템을 초기화하고 산소 수치를 100%로 설정합니다.
    /// </summary>
    /// <param name="ship">초기화할 대상 함선 객체.</param>
    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);
        Refresh();
    }

    public override void Refresh()
    {
        //currentOxygenRate = 100.0f;
        //currentOxygenLevel = OxygenLevel.Normal;
        if (currentOxygenRate > 0)
            currentOxygenLevel = OxygenLevel.Normal;
        else if (currentOxygenRate == 0)
            currentOxygenLevel = OxygenLevel.None;
    }

    public float damageCheckInterval = 0;

    /// <summary>
    /// 매 프레임마다 호출되어 산소 수치를 갱신합니다.
    /// 산소 생성량과 소비량을 계산하여 변화량을 적용합니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
    public override void Update(float deltaTime)
    {
        currentOxygenRate += CalculateOxygenChange() * deltaTime;

        currentOxygenRate = Mathf.Clamp(currentOxygenRate, 0, 100);

        if (currentOxygenRate > 0)
            currentOxygenLevel = OxygenLevel.Normal;
        else if (currentOxygenRate == 0)
            currentOxygenLevel = OxygenLevel.None;

        if (currentOxygenLevel == OxygenLevel.None)
        {
            damageCheckInterval += deltaTime;
            if (damageCheckInterval > 1f)
            {
                damageCheckInterval = 0;
                List<CrewMember> crews = parentShip.GetAllCrew();
                for (int i = crews.Count - 1; i >= 0; i--)
                {
                    crews[i].TakeOxygenDamage();
                }
            }
        }
        else
            damageCheckInterval = 0;
    }

    /// <summary>
    /// 현재 산소 변화량을 계산합니다.
    /// 산소 생성량에서 소비량을 뺀 값을 반환합니다.
    /// </summary>
    /// <returns>1초당 산소 변화량.</returns>
    public float CalculateOxygenChange()
    {
        float oxygenGeneratePerSecond = GetShipStat(ShipStat.OxygenGeneratePerSecond);
        float oxygenUsingPerSecond = GetShipStat(ShipStat.OxygenUsingPerSecond);

        return oxygenGeneratePerSecond - oxygenUsingPerSecond;
    }

    /// <summary>
    /// 현재 산소량을 반환합니다.
    /// </summary>
    /// <returns>현재 산소량.</returns>
    public float GetOxygenRate()
    {
        return currentOxygenRate;
    }

    /// <summary>
    /// 현재 산소 레벨을 반환합니다.
    /// </summary>
    /// <returns>현재 산소 레벨.</returns>
    public OxygenLevel GetOxygenLevel()
    {
        return currentOxygenLevel;
    }
}
