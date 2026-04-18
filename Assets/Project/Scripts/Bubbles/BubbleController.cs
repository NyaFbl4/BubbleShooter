using Sirenix.OdinInspector;
using UnityEngine;

namespace Bubbles
{
    public class BubbleController : MonoBehaviour, IBubbleController
    {
        [SerializeField] private EBubbleType _bubbleType;
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Animator _animator;
        public EBubbleType BubbleType => _bubbleType;

        [Button]
        public void Burst()
        {
            _animator.SetBool("IsBurst", true);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.CompareTag("Wall"))
                return;
            Vector2 velocity = _rb.linearVelocity;
            float speed = velocity.magnitude;

            Vector2 normal = collision.GetContact(0).normal;
            Vector2 reflected = Vector2.Reflect(velocity.normalized, normal);

            _rb.linearVelocity = reflected * speed; // сохраняем ту же скорость
        }
    }
}
