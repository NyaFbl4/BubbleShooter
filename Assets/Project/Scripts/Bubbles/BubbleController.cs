using Sirenix.OdinInspector;
using UnityEngine;
using System;
using BubbleField;

namespace Bubbles
{
    public class BubbleController : MonoBehaviour, IBubbleController
    {
        [SerializeField] private EBubbleType _bubbleType;
        [SerializeField] private Animator _animator;
        // [SerializeField] private LayerMask _wallMask;
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private float _defaultSpeed = 10f;

        private Camera _mainCamera;
        private SpriteRenderer _spriteRenderer;
        private Vector2 _shootDir;
        private float _speed;
        private bool _isFlying;
        private bool _collidingWithBubbleThisFlight;

        public event Action<BubbleController, Collider2D> StoppedOnTrigger;
        public EBubbleType BubbleType => _bubbleType;
        public int Row { get; private set; } = -1;
        public int Col { get; private set; } = -1;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();      

            if (_rb == null)
                _rb = GetComponent<Rigidbody2D>();
            if (_rb == null)
                _rb = gameObject.AddComponent<Rigidbody2D>();

            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.simulated = true;
            _rb.gravityScale = 0f;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;     
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
            _collidingWithBubbleThisFlight = false;
            _isFlying = true;
        }

        public void StopFlying()
        {
            _isFlying = false;
        }

        public void SetGridCoords(int row, int col)
        {
            Row = row;
            Col = col;
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
                _shootDir.x = Mathf.Abs(_shootDir.x);
            }
            else if (pos.x + halfWidth >= rightEdge)
            {
                pos.x = rightEdge - halfWidth;
                transform.position = pos;
                _shootDir.x = -Mathf.Abs(_shootDir.x);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isFlying)
                return;

            var touchedBubble = other.GetComponent<BubbleController>() != null;
            var touchedTop = other.GetComponent<TopBound>() != null;

            if (touchedBubble)
            {
                _collidingWithBubbleThisFlight = true;
                _isFlying = false;
                StoppedOnTrigger?.Invoke(this, other);
                return;
            }

            // как в ките: потолок обрабатываем только если не было коллизии с другим шаром
            if (touchedTop && !_collidingWithBubbleThisFlight)
            {
                _isFlying = false;
                StoppedOnTrigger?.Invoke(this, other);
            }
         }
    }
}
