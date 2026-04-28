using System;
using System.Collections.Generic;
using Bubbles;
using Random = UnityEngine.Random;

namespace BubbleGun
{
    public class BubbleQueueService
    {
        private readonly IBubbleShootPoolService _poolService;
        private readonly List<EBubbleType> _pool = new();

        private bool _hasCurrent;
        private bool _hasNext;

        public event Action QueueChanged;

        public EBubbleType CurrentType { get; private set; }
        public EBubbleType NextType { get; private set; }
        public bool IsPrimed { get; private set; }
        public bool HasCurrent => _hasCurrent;
        public bool HasNext => _hasNext;

        public BubbleQueueService(IBubbleShootPoolService poolService)
        {
            _poolService = poolService;
        }

        public void Prime()
        {
            RebuildPool();
            CurrentType = Roll();
            NextType = Roll();
            _hasCurrent = true;
            _hasNext = true;
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
            _hasCurrent = true;
            _hasNext = true;
            QueueChanged?.Invoke();
        }

        public void ClearCurrentAndNext()
        {
            _hasCurrent = false;
            _hasNext = false;
            CurrentType = default;
            NextType = default;
            QueueChanged?.Invoke();
        }

        public bool TrySwapCurrentNext()
        {
            if (!IsPrimed || !_hasCurrent || !_hasNext)
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
            if (!_pool.Contains(CurrentType))
            {
                CurrentType = Roll();
                changed = true;
            }

            if (!_pool.Contains(NextType))
            {
                NextType = Roll();
                changed = true;
            }

            if (changed)
                QueueChanged?.Invoke();
        }

        private void RebuildPool()
        {
            _poolService?.Rebuild(_pool);
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
