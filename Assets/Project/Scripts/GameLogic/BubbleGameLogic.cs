using System;
using System.Collections.Generic;
using BubbleField;
using BubbleGun;
using Bubbles;
using MessagePipe;
using Project.Scripts.GameManager;
using Project.Scripts.Systems.UI.Dtos;
using UnityEngine;
using VContainer;

namespace GameLogic
{
    public class BubbleGameLogic : MonoBehaviour
    {
        [SerializeField] private BubbleFieldGrid _grid;
        [SerializeField] private int _minMatchCount = 3;
        
        private BubbleShotsService _shots;
        private IGameManagerService _gameManagerService;
        private IPublisher<GameStatusCommandDto> _gameStatusPublisher;
        private bool _endGameTriggered;
        private IBubbleResolveService _resolveService;
        private IBubbleFieldScrollService _scrollService;
        private BubbleQueueService _queue;
        private BubbleScoreService _scoreService;

        [Inject] 
        public void Construct(IBubbleResolveService resolveService, IBubbleFieldScrollService  scrollService,
            BubbleQueueService  queue, BubbleShotsService shots, BubbleScoreService scoreService, IGameManagerService gameManagerService,
            IPublisher<GameStatusCommandDto> gameStatusPublisher)
        { 
            _resolveService = resolveService;
            _scrollService = scrollService;
            _queue = queue;
            _shots = shots;
            _scoreService = scoreService;
            _gameManagerService = gameManagerService;
            _gameStatusPublisher = gameStatusPublisher;
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
            if (resolved.Matched.Count > 0)
            {
                int removedMatched = _grid.RemoveCells(resolved.Matched, playBurst: true);
                _scoreService?.AddDestroyedBubbles(removedMatched);
            }
            
            var floating = _resolveService.CollectFloating(_grid);
            if (floating.Count > 0)
            {
                int removedFloating = _grid.RemoveCells(floating, playBurst: true);
                _scoreService?.AddDestroyedBubbles(removedFloating);
            }

            CheckWinLoseAfterShotResolved();
            _scrollService?.OnShotResolved();
        }
        
        private void CheckWinLoseAfterShotResolved()
         {
            if (_endGameTriggered || _grid == null) return;
            if (!_grid.HasAnyBubbles())
            {
                _endGameTriggered = true;
                _gameStatusPublisher?.Publish(new GameStatusCommandDto { Command = EGameStatusCommand.ShowWinAndFinish });
                _gameManagerService?.FinishGame();
                return;
            }

            if (_shots != null && _shots.ShotsLeft <= 0)
            {
                _endGameTriggered = true;
                _gameStatusPublisher?.Publish(new GameStatusCommandDto { Command = EGameStatusCommand.ShowLoseAndFinish });
                _gameManagerService?.FinishGame();
            }
         }
    }
}
