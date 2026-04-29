using System;
using BubbleField;
using GameLogic;
using VContainer.Unity;

namespace Project.Scripts.GameManager
{
    public class GameBootstrap : ITickable, IDisposable, IGameStartListener, IGameFinishListener, IGameBootstrapControl
    {
        private readonly BubbleFieldGrid _bubbleFieldGrid;
        private readonly BubbleGameLogic _bubbleGameLogic;
        private readonly IBubbleFieldScrollService  _scrollService;
        private readonly BubbleScoreService _scoreService;

        public GameBootstrap(BubbleFieldGrid bubbleFieldGrid, BubbleGameLogic bubbleGameLogic, IBubbleFieldScrollService scrollService, BubbleScoreService scoreService)
        {
            _bubbleFieldGrid = bubbleFieldGrid;
            _bubbleGameLogic = bubbleGameLogic;
            _scrollService = scrollService;
            _scoreService = scoreService;
            
            IGameListener.Register(this);
        }

        public void Tick()
        {

        }

        public void OnStartGame()
        {
            _scoreService?.Reset();
            _bubbleFieldGrid.BuildFromLevelData();
            _scrollService?.Init();
        }

        public void OnFinishGame()
        {

        }

        public void Dispose()
        {
            IGameListener.Unregister(this);
        }
    }
}
