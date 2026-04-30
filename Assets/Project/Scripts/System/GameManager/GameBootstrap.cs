using System;
using BubbleField;
using GameLogic;
using Project.Scripts.System.UseCases;
using VContainer.Unity;

namespace Project.Scripts.GameManager
{
    public class GameBootstrap : ITickable, IDisposable, IGameStartListener, IGameFinishListener, IGameBootstrapControl
    {
        private readonly BubbleFieldGrid _bubbleFieldGrid;
        private readonly BubbleGameLogic _bubbleGameLogic;
        private readonly IBubbleFieldScrollService  _scrollService;
        private readonly AddScoreUseCase _addScoreUseCase;

        public GameBootstrap(BubbleFieldGrid bubbleFieldGrid, BubbleGameLogic bubbleGameLogic, IBubbleFieldScrollService scrollService, AddScoreUseCase addScoreUseCase)
        {
            _bubbleFieldGrid = bubbleFieldGrid;
            _bubbleGameLogic = bubbleGameLogic;
            _scrollService = scrollService;
            _addScoreUseCase = addScoreUseCase;
            
            IGameListener.Register(this);
        }

        public void Tick()
        {

        }

        public void OnStartGame()
        {
            _addScoreUseCase?.ResetScore();
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
