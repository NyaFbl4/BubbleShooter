using Bubbles;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BubbleGun
{
    public class BubbbleGun : MonoBehaviour
    {
        // [SerializeField] private BubbleController _bubble;
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private float _minAngle = 20f;   // ограничение вниз-влево/вправо
        [SerializeField] private float _maxAngle = 160f;
        [SerializeField] private float _shotSpeed;
        [SerializeField] private BubbleSpawner _spawner;
        [SerializeField] private EBubbleType _currentType;
        // [SerializeField] private BubbleController _bubblePrefab;

        private void Update()
        {
            AimToMouse();

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryShoot();
            }
        }

        private void AimToMouse()
        {
            if (Mouse.current == null || _camera == null || _pivot == null)
                return;

                Vector2 mouseScreen = Mouse.current.position.ReadValue();
                Vector3 mouseWorld = _camera.ScreenToWorldPoint(
                    new Vector3(mouseScreen.x, mouseScreen.y, Mathf.Abs(_camera.transform.position.z))
            );
            mouseWorld.z = _pivot.position.z;

            Vector2 dir = mouseWorld - _pivot.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            angle = Mathf.Clamp(angle, _minAngle, _maxAngle);
            _pivot.rotation = Quaternion.Euler(0f, 0f, angle - 90f); //добавить огриничение на вращение

            // Если ствол смотрит вверх (+Y), замени на:
            // _pivot.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        [Button]
        private void TryShoot()
        {
            if (_spawner == null || _shootPoint == null)
                return;

            BubbleController spawned = _spawner.Spawn(_currentType, _shootPoint.position);
            if (spawned == null)
                return;

            spawned.transform.up = _shootPoint.up;
            spawned.Shoot(_shootPoint.up, _shotSpeed);
        }
    }
}