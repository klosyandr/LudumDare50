using UnityEngine;

namespace DefaultNamespace
{
    public class ClearCauldronSMB : TutorialEntrySMB
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Cauldron.instance.Clear();
        }
    }
}