using UnityEngine;

/*
 * link 1 : https://answers.unity.com/questions/1192912/hot-to-make-combo-attack-1-button-do-3-things.html
 * link 2 : https://samirgeorgy.wordpress.com/2021/07/22/lets-create-a-simple-melee-combo-system/
 */

public class PlayerAttackBehaviour : StateMachineBehaviour
{
    public int attackIndex;
    public bool isLastIndex;

    int numOfClick = 0;
    bool callNextState = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        numOfClick = 0;
        callNextState = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Input.GetMouseButtonDown(0))
        {
            numOfClick++;
        }

        if (0.95f < stateInfo.normalizedTime && callNextState == false)
        {
            callNextState = true;

            int attackValue = (isLastIndex || numOfClick == 0) ? 0 : attackIndex + 1;
            PlayerControl.Instance.SetPlayerAnimation(EPlayerState.Attack, attackValue);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{

    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}