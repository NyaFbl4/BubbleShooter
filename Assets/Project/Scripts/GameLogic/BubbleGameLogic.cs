using System.Collections.Generic;
using BubbleField;
using BubbleGun;
using Bubbles;
using UnityEngine;
using VContainer;

namespace GameLogic
{
    public class BubbleGameLogic : MonoBehaviour
    {
        [SerializeField] private BubbleFieldGrid _grid;
        [SerializeField] private int _minMatchCount = 3;
        
        private IBubbleResolveService _resolveService;
        private IBubbleFieldScrollService _scrollService;
        private BubbleQueueService _queue;

        [Inject] 
        public void Construct(IBubbleResolveService resolveService, IBubbleFieldScrollService  scrollService,
            BubbleQueueService  queue)
        { 
            _resolveService = resolveService;
            _scrollService = scrollService;
            _queue = queue;
        }

        private void Start()
        {
            _scrollService?.Init();
        }
        
        public void RegisterFlyingBubble(BubbleController bubble)
        {
            if (bubble == null) return;
            bubble.StoppedOnTrigger -= OnFlyingBubbleStopped;
            bubble.StoppedOnTrigger += OnFlyingBubbleStopped;
        }

        private void OnFlyingBubbleStopped(BubbleController flying, Collider2D other)
        {
            if (flying == null || other == null || _grid == null) return;
            flying.StoppedOnTrigger -= OnFlyingBubbleStopped;

            if (!_grid.TryAttachFlyingBubble(flying, other, out var attachedCell))
                return;

            if (_resolveService == null)
            {
                _scrollService?.OnShotResolved();
                return;
            }
            
            var resolved = _resolveService.Resolve(_grid, attachedCell, _minMatchCount);
            var boardChanged = false;
            if (resolved.Matched.Count > 0)
            {
                _grid.RemoveCells(resolved.Matched, playBurst: true);
                boardChanged = true;
            }
            
            var floating = _resolveService.CollectFloating(_grid);
            if (floating.Count > 0)
            {
                _grid.RemoveCells(floating, playBurst: true);
                boardChanged = true;
            }
            
            _scrollService?.OnShotResolved();
        }
    }
}