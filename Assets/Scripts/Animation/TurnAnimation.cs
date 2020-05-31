using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnAnimation : StateMachineBehaviour
{
    public int Side;

    /*
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int side = animator.GetInteger("Side");
        if (side != Side)
        {
            Debug.Log(stateInfo.normalizedTime);
            animator.Play((side > 0) ? "Right Turn" : "Left Turn", layerIndex, (1 - Mathf.Clamp01(stateInfo.normalizedTime)));
        }
    }
    */

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animator.transform.parent.eulerAngles = new Vector3(0, (animator.GetInteger("Side") > 0) ? 180 : 0, 0);
    }
}
