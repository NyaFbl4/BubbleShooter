using System.Collections.Generic;
using BubbleField;
using Bubbles;
using UnityEngine;

namespace BubbleGun
{
    public class BubbleQueueService
    {
        private readonly BubbleLevelData _levelData;
        private readonly BubbleCatalog _bubbleCatalog;
        private readonly List<EBubbleType> _pool = new();

        public EBubbleType CurrentType { get; private set; }
        public EBubbleType NextType { get; private set; }
        public bool IsPrimed { get; private set; }
        
        public BubbleQueueService(BubbleLevelData levelData, BubbleCatalog bubbleCatalog)
        {
            _levelData = levelData;
            _bubbleCatalog = bubbleCatalog;
        }

        public void Prime()
        {
            RebuildPool();
            CurrentType = Roll();
            NextType = Roll();
            IsPrimed = true;
        }

        public void Advance()
        {
            if (!IsPrimed)
            {
                Prime();
                return;
            }

            CurrentType = NextType;
            NextType = Roll();
        }

        private void RebuildPool()
        {
            _pool.Clear();

            if (_levelData != null && _levelData.AvailableRandomTypes != null)
            {
                foreach (var type in _levelData.AvailableRandomTypes)
                {
                    if (_pool.Contains(type))
                        continue;
                    if (IsSpawnable(type))
                        _pool.Add(type);
                }
            }

            if (_pool.Count == 0 && _bubbleCatalog != null)
            {
                foreach (var def in _bubbleCatalog.Definitions)
                {
                    if (def == null || def.Prefab == null)
                        continue;
                    if (_pool.Contains(def.Type))
                        continue;
                    _pool.Add(def.Type);
                }
            }
        }

        private bool IsSpawnable(EBubbleType type)
        {
            if (_bubbleCatalog == null)
                return false;
            
            return _bubbleCatalog.TryGet(type, out var def) &&
                def != null &&
                def.Prefab != null;
        }
        private EBubbleType Roll()
        {
            if (_pool.Count == 0)
                RebuildPool();

            if (_pool.Count == 0)
                return default;
            
            int idx = Random.Range(0, _pool.Count);
            return _pool[idx];
        }
    }
}