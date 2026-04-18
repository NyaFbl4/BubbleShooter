using Sirenix.OdinInspector;
using UnityEngine;
using System;

namespace Bubbles
{
    public class BubbleController : MonoBehaviour, IBubbleController
    {
        [SerializeField] private EBubbleType _bubbleType;
        // [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Animator _animator;
        [SerializeField] private LayerMask _wallMask;
        // private float _shotSpeed;
        [SerializeField] private float _defaultSpeed = 10f;

        private Camera _mainCamera;
        private SpriteRenderer _spriteRenderer;
        private Vector2 _shootDir;
        private float _speed;
        private bool _isFlying;

        public event Action<BubbleController, Collider2D> StoppedOnTrigger;
        public EBubbleType BubbleType => _bubbleType;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();           
        }

        [Button]
        public void Burst()
        {
            _animator.SetBool("IsBurst", true);
        }

        public void Shoot(Vector2 dir, float speed = -1f)
        {
            _shootDir = dir.normalized;
            _speed = speed > 0f ? speed : _defaultSpeed;
            _isFlying = true;
        }

        // private void OnCollisionEnter2D(Collision2D collision)
        public void StopFlying()
        {
            _isFlying = false;
        }

        private void FixedUpdate()
        {
            if (!_isFlying || _mainCamera == null || _spriteRenderer == null)   
                return;

            transform.position += (Vector3)(_shootDir * _speed * Time.fixedDeltaTime);
            var leftEdge = _mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            var rightEdge = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
            var halfWidth = _spriteRenderer.bounds.extents.x;

            var pos = transform.position;
            if (pos.x - halfWidth <= leftEdge)
            {
                pos.x = leftEdge + halfWidth;
                transform.position = pos;
                _shootDir.x = Mathf.Abs(_shootDir.x); // отскок от левой стены
            }
            else if (pos.x + halfWidth >= rightEdge)
            {
                pos.x = rightEdge - halfWidth;
                transform.position = pos;
                _shootDir.x = -Mathf.Abs(_shootDir.x); // отскок от правой стены
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isFlying)
                return;

            // как в ките: останавливаемся при касании другого шара/верхней границы
            if (other.GetComponent<BubbleController>() != null || other.CompareTag("Top"))
            {
                _isFlying = false;
                StoppedOnTrigger?.Invoke(this, other);
            }
         }
    }
}
