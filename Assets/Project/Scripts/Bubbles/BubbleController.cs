using Sirenix.OdinInspector;
using UnityEngine;

namespace Assets.Project.Scripts.Bubbles
{
    public class BubbleController : MonoBehaviour
    {
        [SerializeField] private EBubbleType _bubbleType;
        [SerializeField] private Animator _animator;
        public EBubbleType BubbleType => _bubbleType;

        [Button]
        public void Burst()
        {
            _animator.SetBool("IsBurst", true);
        }
    }
}
