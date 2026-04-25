using System;
using System.Collections.Generic;
using BubbleField;
using Bubbles;
using Random = UnityEngine.Random;

namespace BubbleGun
{
    public class BubbleQueueService
    {
        private readonly BubbleLevelData _levelData;
        private readonly BubbleCatalog _bubbleCatalog;
        private readonly IBubbleShootPoolService _poolService;
        private readonly List<EBubbleType> _pool = new();

        public event Action QueueChanged;
        
        public EBubbleType CurrentType { get; private set; }
        public EBubbleType NextType { get; private set; }
        public bool IsPrimed { get; private set; }
        
        public BubbleQueueService(IBubbleShootPoolService poolService)
        {
            _poolService = poolService;
        }

        public void Prime()
        {
            RebuildPool();
            CurrentType = Roll();
            NextType = Roll();
            IsPrimed = true;
            QueueChanged?.Invoke();
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
            QueueChanged?.Invoke();
        }

        public bool TrySwapCurrentNext()
        {
            if (!IsPrimed)
                return false;
            
            (CurrentType, NextType) = (NextType, CurrentType);
            QueueChanged?.Invoke();
            return true;
        }

        public void SyncAfterBoardChanged()
        {
            RebuildPool();
            if (!IsPrimed)
            {
                Prime();
                return;
            }

            var changed = false;
            if (!_pool.Contains(CurrentType)) { CurrentType = Roll(); changed = true; }
            if (!_pool.Contains(NextType)) { NextType = Roll(); changed = true; }
            if (changed) QueueChanged?.Invoke();
        }
        
        private void RebuildPool()
        {
            _poolService?.Rebuild(_pool);
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