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
        private readonly LoadLevelService _loadLevelService;

        public GameBootstrap(
            BubbleFieldGrid bubbleFieldGrid,
            BubbleGameLogic bubbleGameLogic,
            IBubbleFieldScrollService scrollService,
            AddScoreUseCase addScoreUseCase,
            LoadLevelService loadLevelService)
        {
            _bubbleFieldGrid = bubbleFieldGrid;
            _bubbleGameLogic = bubbleGameLogic;
            _scrollService = scrollService;
            _addScoreUseCase = addScoreUseCase;
            _loadLevelService = loadLevelService;
            
            IGameListener.Register(this);
        }

        public void Tick()
        {

        }

        public void OnStartGame()
        {
            _loadLevelService?.LogLevelBubblesSummary();
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
