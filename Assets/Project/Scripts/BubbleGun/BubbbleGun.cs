using System;
using System.Collections.Generic;
using BubbleField;
using Bubbles;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace BubbleGun
{
    public class BubbbleGun : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private float _minAngle = 20f; // ограничение вниз-влево/вправо
        [SerializeField] private float _maxAngle = 160f;
        [SerializeField] private float _shotSpeed;
        [SerializeField] private BubbleSpawner _spawner;
        [SerializeField] private EBubbleType _currentType;
        [SerializeField] private EBubbleType _nextType;
        [SerializeField] private BubbleGameLogic _gameLogic;
        [SerializeField] private BubbleLevelData _levelData;
        [SerializeField] private BubbleCatalog _bubbleCatalog;
        [SerializeField] private SpriteRenderer _currentBubble;
        [SerializeField] private SpriteRenderer _nextBubble;

        private readonly List<EBubbleType> _shotPool = new();

        private void Start()
        {
            BuildShotPool();
            PrimeQueue();
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
            _gameLogic?.RegisterFlyingBubble(spawned);

            AdvanceQueue(); // после выстрела: current <- next, next <- random from level config
            RefreshPreviews();
        }

        private void BuildShotPool()
        {
            _shotPool.Clear();

            if (_levelData != null && _levelData.AvailableRandomTypes != null)
            {
                foreach (var type in _levelData.AvailableRandomTypes)
                {
                    if (_shotPool.Contains(type))
                        continue;
                    if (IsTypeConfigured(type))
                        _shotPool.Add(type);
                }
            }

            if (_shotPool.Count == 0 && _bubbleCatalog != null)
            {
                foreach (var def in _bubbleCatalog.Definitions)
                {
                    if (def == null || def.Prefab == null)
                        continue;
                    if (_shotPool.Contains(def.Type))
                        continue;
                    _shotPool.Add(def.Type);
                }
            }
        }
        
        private bool IsTypeConfigured(EBubbleType type)
        {
            if (_bubbleCatalog == null)
                return false;
            return _bubbleCatalog.TryGet(type, out var def)
                   && def != null
                   && def.Prefab != null;
        }

       private void PrimeQueue()
       {
           _currentType = RollType();
           _nextType = RollType();
       }

       private void AdvanceQueue()
       {
           _currentType = _nextType;
           _nextType = RollType();
       }

       private EBubbleType RollType()
       {
           if (_shotPool.Count == 0)
               BuildShotPool();

           if (_shotPool.Count == 0)
               return _currentType; // аварийный fallback

           int idx = Random.Range(0, _shotPool.Count);
           return _shotPool[idx];
       }

       private void RefreshPreviews()
       {
           ApplyPreview(_currentBubble, _currentType);
           ApplyPreview(_nextBubble, _nextType);
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