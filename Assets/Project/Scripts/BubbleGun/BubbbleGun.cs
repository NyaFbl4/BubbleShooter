using System;
using Bubbles;
using GameLogic;
using MessagePipe;
using Project.Scripts.GameManager;
using Project.Scripts.Systems.UI.Dtos;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace BubbleGun
{
    public class BubbleGunController : MonoBehaviour, IGameStartListener, IGameUpdateListener, IGameFinishListener
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private BubbleSpawner _spawner;
        [SerializeField] private BubbleGameLogic _gameLogic;
        [SerializeField] private SpriteRenderer _currentBubbleOnGun;

        private GunConfig _gunConfig;
        private BubbleCatalog _bubbleCatalog;
        private BubbleQueueService _queue;
        private BubbleGunService _service;
        private BubbleShotsService _shots;
        private ISubscriber<SwapBubbleCommandDto> _swapSubscriber;
        private bool _isAiming;
        private bool _isShotInFlight;
        private BubbleController _activeShotBubble;

        private IDisposable _swapSubscription;

        [Inject]
        public void Construct(
            GunConfig gunConfig,
            BubbleCatalog bubbleCatalog,
            BubbleQueueService queue,
            BubbleGunService service,
            BubbleShotsService shots,
            ISubscriber<SwapBubbleCommandDto> swapSubscriber)
        {
            _gunConfig = gunConfig;
            _bubbleCatalog = bubbleCatalog;
            _queue = queue;
            _service = service;
            _shots = shots;
            _swapSubscriber = swapSubscriber;

            _swapSubscription = _swapSubscriber.Subscribe(_ => TrySwapFromUi());

            IGameListener.Register(this);
        }

        public void OnStartGame()
        {
            _shots?.ResetFromLevel();
            _queue?.Prime();
            if (_shots != null && !_shots.HasShots)
                _queue?.ClearCurrentAndNext();
            _isShotInFlight = false;
            UnbindActiveShotBubble();

            if (_queue != null)
                _queue.QueueChanged += RefreshGunCurrentBubble;
            RefreshGunCurrentBubble();
        }

        private void OnDestroy()
        {
            if (_queue != null)
                _queue.QueueChanged -= RefreshGunCurrentBubble;
            UnbindActiveShotBubble();
            _swapSubscription?.Dispose();
            IGameListener.Unregister(this);
        }

        public void OnUpdate(float deltaTime)
        {
            if (TryHandleSwapInput())
                return;

            if (IsShootPointerPressedThisFrame())
                _isAiming = true;

            if (_isAiming)
                AimToPointer();

            if (IsShootPointerReleasedThisFrame())
            {
                if (_isAiming)
                    TryShoot();

                _isAiming = false;
            }
        }

        public void OnFinishGame()
        {
            if (_queue != null)
                _queue.QueueChanged -= RefreshGunCurrentBubble;
            UnbindActiveShotBubble();
            _swapSubscription?.Dispose();
            _swapSubscription = null;
        }

        private void TrySwapFromUi()
        {
            if (_queue == null || _gunConfig == null)
                return;
            if (!_gunConfig.AllowSwap)
                return;
            if (_shots != null && !_shots.HasShots)
                return;

            if (_queue.TrySwapCurrentNext())
                RefreshGunCurrentBubble();
        }

        private bool TryHandleSwapInput()
        {
            if (_queue == null || _gunConfig == null || !_gunConfig.AllowSwap)
                return false;

            bool keySwap = Keyboard.current != null && Keyboard.current[_gunConfig.SwapKey].wasPressedThisFrame;
            bool rightClickSwap = _gunConfig.AllowRightClickSwap &&
                                  Mouse.current != null &&
                                  Mouse.current.rightButton.wasPressedThisFrame;

            if (!keySwap && !rightClickSwap)
                return false;

            var swapped = _queue.TrySwapCurrentNext();
            if (swapped)
                RefreshGunCurrentBubble();
            return swapped;
        }

        private void AimToPointer()
        {
            if (_camera == null || _pivot == null)
                return;

            if (!TryGetPointerScreenPosition(out var pointerScreen))
                return;

            Vector3 pointerWorld = _camera.ScreenToWorldPoint(
                new Vector3(pointerScreen.x, pointerScreen.y, Mathf.Abs(_camera.transform.position.z))
            );
            pointerWorld.z = _pivot.position.z;

            if (_service.TryCalculateAim(_pivot.position, pointerWorld, _gunConfig.MinAngle, _gunConfig.MaxAngle, out float z))
                _pivot.rotation = Quaternion.Euler(0f, 0f, z);
        }

        private void TryShoot()
        {
            if (_queue == null || _shots == null)
                return;
            if (_isShotInFlight)
                return;
            if (!_shots.HasShots)
                return;
            if (!_queue.HasCurrent)
                return;
            if (!IsPointerAtOrAboveGun())
                return;

            BubbleController spawned = _service.Shoot(_spawner, _shootPoint, _queue.CurrentType, _gunConfig.ShotSpeed, _gameLogic);
            if (spawned == null)
                return;

            BindActiveShotBubble(spawned);
            _isShotInFlight = true;

            _shots.TryConsumeOne();
            if (_shots.HasShots)
                _queue.Advance();
            else
                _queue.ClearCurrentAndNext();

            RefreshGunCurrentBubble();
        }

        private void RefreshGunCurrentBubble()
        {
            if (_currentBubbleOnGun == null)
                return;

            if (_queue == null || !_queue.HasCurrent)
            {
                _currentBubbleOnGun.sprite = null;
                _currentBubbleOnGun.enabled = false;
                return;
            }

            var sprite = ResolveSprite(_queue.CurrentType);
            _currentBubbleOnGun.sprite = sprite;
            _currentBubbleOnGun.enabled = sprite != null;
        }

        private Sprite ResolveSprite(EBubbleType type)
        {
            if (_bubbleCatalog == null)
                return null;

            if (!_bubbleCatalog.TryGet(type, out var def) || def == null)
                return null;

            if (def.Sprite != null)
                return def.Sprite;

            if (def.Prefab == null)
                return null;

            var sr = def.Prefab.GetComponentInChildren<SpriteRenderer>();
            return sr != null ? sr.sprite : null;
        }

        private bool IsPointerAtOrAboveGun()
        {
            if (_camera == null || _pivot == null)
                return false;

            if (!TryGetPointerScreenPosition(out var pointerScreen))
                return false;

            Vector3 pointerWorld = _camera.ScreenToWorldPoint(
                new Vector3(pointerScreen.x, pointerScreen.y, Mathf.Abs(_camera.transform.position.z))
            );
            return pointerWorld.y >= _pivot.position.y;
        }

        private bool IsShootPointerPressedThisFrame()
        {
            bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool touchPressed = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
            return mousePressed || touchPressed;
        }

        private bool IsShootPointerReleasedThisFrame()
        {
            bool mouseReleased = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
            bool touchReleased = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
            return mouseReleased || touchReleased;
        }

        private bool TryGetPointerScreenPosition(out Vector2 pointerScreen)
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.isPressed || touch.press.wasPressedThisFrame || touch.press.wasReleasedThisFrame)
                {
                    pointerScreen = touch.position.ReadValue();
                    return true;
                }
            }

            if (Mouse.current != null)
            {
                pointerScreen = Mouse.current.position.ReadValue();
                return true;
            }

            pointerScreen = default;
            return false;
        }

        private void BindActiveShotBubble(BubbleController bubble)
        {
            UnbindActiveShotBubble();
            _activeShotBubble = bubble;
            if (_activeShotBubble != null)
                _activeShotBubble.StoppedOnTrigger += OnActiveShotStopped;
        }

        private void UnbindActiveShotBubble()
        {
            if (_activeShotBubble != null)
                _activeShotBubble.StoppedOnTrigger -= OnActiveShotStopped;
            _activeShotBubble = null;
        }

        private void OnActiveShotStopped(BubbleController bubble, Collider2D other)
        {
            if (_activeShotBubble != null)
                _activeShotBubble.StoppedOnTrigger -= OnActiveShotStopped;
            _activeShotBubble = null;
            _isShotInFlight = false;
        }
    }
}
