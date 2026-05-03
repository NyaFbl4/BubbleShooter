using Assets.Project.Scripts.System.Messaging;
using BubbleField;
using BubbleGun;
using Bubbles;
using GameLogic;
using MessagePipe;
using Project.Scripts.GameManager;
using Project.Scripts.System.UseCases;
using Project.Scripts.Systems.UI;
using Project.Scripts.UI.EndGame;
using Project.Scripts.UI.LevelMapUI;
using Project.Scripts.UI.LevelUIView;
using Project.Scripts.UI.PauseUI;
using Project.Scripts.UI.SettingsUI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Installers
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private BubbleGunController  _bubbleGun;
        [SerializeField] private BubbleGameLogic _gameLogic;
        [SerializeField] private BubbleFieldGrid _grid;
        [SerializeField] private BubbleSpawner _bubbleSpawner;
        [SerializeField] private GameManagerHelper _gameManagerHelper;

        [Header("Configs")] 
        [SerializeField] private GunConfig _gunConfig;
        [SerializeField] private BubbleLevelData _levelData;
        [SerializeField] private BubbleLevelData[] _mapLevels;
        [SerializeField] private BubbleCatalog _bubbleCatalog;
        [SerializeField] private LayoutsRepository _layoutsRepository;
        
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterSystem(builder);
            RegisterUseCases(builder);
            RegisterViews(builder);
            RegisterPresenters(builder);
            RegisterComponentOnScene(builder);
            RegisterConfigs(builder);
        }

        private void RegisterUseCases(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<HidePopUpUseCase>(Lifetime.Singleton);
            builder.RegisterEntryPoint<ShowPopUpUseCase>(Lifetime.Singleton);
            builder.RegisterEntryPoint<SelectLevelUseCase>(Lifetime.Singleton);
            builder.Register<AddScoreUseCase>(Lifetime.Singleton);
        }

        private void RegisterViews(IContainerBuilder builder)
        {
            if (_layoutsRepository == null)
                return;

            foreach (var prefab in _layoutsRepository.Views)
            {
                if (prefab ==  null)
                    continue;
                
                builder.RegisterComponentInNewPrefab(prefab, Lifetime.Scoped).AsSelf().AsImplementedInterfaces();
            }
        }
        
        private void RegisterSystem(IContainerBuilder builder)
        {
            builder.RegisterMessagePipe();
            builder.RegisterEntryPoint<GameManagerService>(Lifetime.Singleton).As<IGameManagerService>();
            builder.RegisterEntryPoint<GameBootstrap>(Lifetime.Singleton).As<IGameBootstrapControl>();
            builder.RegisterEntryPoint<UIController>().As<IUIController>();
            builder.RegisterEntryPoint<UIMessageHandler>(Lifetime.Singleton);
            builder.Register<BubbleGunService>(Lifetime.Singleton);
            builder.Register<BubbleQueueService>(Lifetime.Singleton);
            builder.Register<BubbleShotsService>(Lifetime.Singleton);
            builder.Register<BubbleScoreService>(Lifetime.Singleton);
            builder.Register<LoadLevelService>(Lifetime.Singleton);
            builder.Register<BubbleLevelSelectionService>(Lifetime.Singleton);
            builder.Register<IBubbleResolveService, BubbleResolveService>(Lifetime.Singleton);
            builder.Register<IBubbleFieldScrollService, BubbleFieldScrollService>(Lifetime.Singleton);
            builder.Register<IBubbleShootPoolService, BubbleShootPoolService>(Lifetime.Singleton);
        }

        private void RegisterPresenters(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<LevelUIPresenter>(Lifetime.Singleton).As<ILevelUIPresenter>();
            builder.RegisterEntryPoint<LevelMapUIPresenter>(Lifetime.Singleton).As<ILevelMapUIPresenter>();
            builder.RegisterEntryPoint<SettingsUIPresenter>(Lifetime.Singleton).As<ISettingsUIPresenter>();
            builder.RegisterEntryPoint<PauseUIPresenter>(Lifetime.Singleton).As<IPauseUIPresenter>();
            builder.RegisterEntryPoint<EndGamePresenter>(Lifetime.Singleton).As<IEndGamePresenter>();
        }

        private void RegisterConfigs(IContainerBuilder builder)
        {
            if (_gunConfig != null)
                builder.RegisterInstance(_gunConfig);
            else
                Debug.LogError("GameLifetimeScope: GunConfig is not assigned.");

            if (_levelData != null)
                builder.RegisterInstance(_levelData);
            else
                Debug.LogError("GameLifetimeScope: BubbleLevelData (_levelData) is not assigned.");

            builder.RegisterInstance(_mapLevels ?? System.Array.Empty<BubbleLevelData>());

            if (_bubbleCatalog != null)
                builder.RegisterInstance(_bubbleCatalog);
            else
                Debug.LogError("GameLifetimeScope: BubbleCatalog is not assigned.");
        }
        
        private void RegisterComponentOnScene(IContainerBuilder builder)
        {
            builder.RegisterComponent(_gameManagerHelper).AsSelf();
            builder.RegisterComponent(_bubbleGun).AsSelf();
            builder.RegisterComponent(_gameLogic).AsSelf();
            if (_bubbleSpawner == null)
                _bubbleSpawner = FindFirstObjectByType<BubbleSpawner>();

            if (_bubbleSpawner != null)
                builder.RegisterComponent(_bubbleSpawner).AsSelf();
            else
                Debug.LogWarning("GameLifetimeScope: BubbleSpawner is not assigned and was not found in scene.");

            builder.RegisterComponent(_grid).AsSelf();
        }
    }
}

