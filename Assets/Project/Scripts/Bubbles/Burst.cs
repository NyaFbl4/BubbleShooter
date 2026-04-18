using UnityEngine;

namespace Bubbles
{
    public class Burst : StateMachineBehaviour
    {
        private bool _destroyed;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _destroyed = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_destroyed)
                return;

            if (stateInfo.normalizedTime < 1f)
                return;

            _destroyed = true;
            Object.Destroy(animator.gameObject);
        }

    }
}
