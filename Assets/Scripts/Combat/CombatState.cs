using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine.UI;

public class CombatState : IGameState
{
    // 현재 단계
    private CombatPhase currentPhase;

    private CombatUIController uiController;

    public void Enter()
    {
        currentPhase = CombatPhase.Initialize;
    }

    public void Update()
    {
        switch (currentPhase)
        {
            case CombatPhase.Initialize:
                break;
            case CombatPhase.Combat:
                break;
            case CombatPhase.DestroyAnimation:
                break;
            case CombatPhase.Reward:
                break;
            case CombatPhase.End:
                break;
            default:
                break;
        }
    }

    public void Exit()
    {
    }
}
