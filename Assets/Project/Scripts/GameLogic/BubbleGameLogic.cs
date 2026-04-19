using System.Collections.Generic;
using BubbleField;
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

        [Inject] public void Construct(IBubbleResolveService resolveService)
        { 
            _resolveService = resolveService;
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
                return;
            var resolved = _resolveService.Resolve(_grid, attachedCell, _minMatchCount);
            if (resolved.Matched.Count > 0)
                _grid.RemoveCells(resolved.Matched, playBurst: true);
            if (resolved.Floating.Count > 0) 
                _grid.RemoveCells(resolved.Floating, playBurst: true);
        }
    }
}