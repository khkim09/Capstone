using System.Collections;
using UnityEditor.Animations;
using UnityEngine;

public class AnimationCleanser : MonoBehaviour
{
    public AnimatorController humanoidAnimator;
    public AnimatorController amorphousAnimator;
    public AnimatorController beastAnimator;
    public AnimatorController mechaTankAnimator;
    public AnimatorController mechaSupAnimator;
    public AnimatorController insectAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(AnimationCleans());
        StartCoroutine(KillAllCrews());
        StartCoroutine(RefreshRoom());
    }

    private IEnumerator AnimationCleans()
    {
        yield return null;
        yield return null;
        yield return null;
        foreach (CrewMember crew in GameManager.Instance.playerShip.allCrews)
        {
            switch (crew.race)
            {
                case CrewRace.Human:
                    crew.gameObject.GetComponent<Animator>().runtimeAnimatorController = humanoidAnimator;
                    break;
                case CrewRace.Amorphous:
                    crew.gameObject.GetComponent<Animator>().runtimeAnimatorController = amorphousAnimator;
                    break;
                case CrewRace.MechanicTank:
                    crew.gameObject.GetComponent<Animator>().runtimeAnimatorController = mechaTankAnimator;
                    break;
                case CrewRace.MechanicSup:
                    crew.gameObject.GetComponent<Animator>().runtimeAnimatorController = mechaSupAnimator;
                    break;
                case CrewRace.Beast:
                    crew.gameObject.GetComponent<Animator>().runtimeAnimatorController = beastAnimator;
                    break;
                case CrewRace.Insect:
                    crew.gameObject.GetComponent<Animator>().runtimeAnimatorController = insectAnimator;
                    break;
            }
            crew.PlayAnimation("idle");
        }
    }

    private IEnumerator KillAllCrews()
    {
        yield return null;
        yield return null;
        yield return null;
        foreach (CrewMember crew in GameManager.Instance.playerShip.allEnemies)
            crew.Die();
    }

    private IEnumerator RefreshRoom()
    {
        yield return null;
        yield return null;
        yield return null;

        GameManager.Instance.playerShip.RecalculateAllStats();
        GameManager.Instance.playerShip.RefreshAllSystems();
        GameManager.Instance.playerShip.ActionReconnect();
    }
}
