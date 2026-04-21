using System;
using System.Collections.Generic;
using BubbleField;
using Bubbles;
using GameLogic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace BubbleGun
{
    public class BubbleGunController  : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private BubbleSpawner _spawner;
        [SerializeField] private BubbleGameLogic _gameLogic;
        [SerializeField] private BubbleCatalog _bubbleCatalog;
        [SerializeField] private SpriteRenderer _currentBubble;
        [SerializeField] private SpriteRenderer _nextBubble;

        private GunConfig _gunConfig;
        private BubbleQueueService _queue;
        private BubbleGunService _service;

        [Inject]
        public void Construct(GunConfig gunConfig, 
            BubbleQueueService queue,  BubbleGunService service)
        {
            _gunConfig = gunConfig;
            _queue = queue;
            _service = service;
        }
        
        private void Start()
        {
            _service = new BubbleGunService();
            _queue?.Prime();
            RefreshPreviews();
        }

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

            if (_service.TryCalculateAim(_pivot.position, mouseWorld, _gunConfig.MinAngle, _gunConfig.MaxAngle, out float z))
                _pivot.rotation = Quaternion.Euler(0f, 0f, z);
        }
        
        private void TryShoot()
        {
            if (_queue == null)
                return;
            
            BubbleController spawned = _service.Shoot(_spawner, _shootPoint, _queue.CurrentType, _gunConfig.ShotSpeed, _gameLogic);
            if (spawned == null)
                return;

            _queue.Advance();
            RefreshPreviews();
        }

       private void RefreshPreviews()
       {
           if (_queue == null)
           {
               ApplyPreview(_currentBubble, default);
               ApplyPreview(_nextBubble, default);
               return;
           }
           
           ApplyPreview(_currentBubble, _queue.CurrentType);
           ApplyPreview(_nextBubble, _queue.NextType);
       } 
       
       private void ApplyPreview(SpriteRenderer target, EBubbleType type)
       {
           if (target == null)
               return;

           if (TryGetPreviewSprite(type, out var sprite))
           {
               target.sprite = sprite;
               target.enabled = true;
           }
           else
           {
               target.sprite = null;
               target.enabled = false;
           }
       }

       private bool TryGetPreviewSprite(EBubbleType type, out Sprite sprite)
       {
           sprite = null;

           if (_bubbleCatalog == null)
               return false;

           if (!_bubbleCatalog.TryGet(type, out var def) || def == null)
               return false;

           if (def.Sprite != null)
           {
               sprite = def.Sprite;
               return true;
           }

           if (def.Prefab != null)
           {
               var sr = def.Prefab.GetComponentInChildren<SpriteRenderer>();
               if (sr != null)
               {
                   sprite = sr.sprite;
                   return true;
               }
           }

           return false;
       }
    }
}