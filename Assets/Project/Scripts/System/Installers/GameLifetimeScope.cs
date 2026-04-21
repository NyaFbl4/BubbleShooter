using Assets.Project.Scripts.System.Messaging;
using BubbleField;
using BubbleGun;
using Bubbles;
using GameLogic;
using MessagePipe;
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

        [Header("Configs")] 
        [SerializeField] private GunConfig _gunConfig;
        [SerializeField] private BubbleLevelData _levelData;
        [SerializeField] private BubbleCatalog _bubbleCatalog;
        
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterSystem(builder);
            RegisterComponentOnScene(builder);
            RegisterConfigs(builder);
        }
        
        private void RegisterSystem(IContainerBuilder builder)
        {
            builder.RegisterMessagePipe();
            builder.Register<BubbleGunService>(Lifetime.Singleton);
            builder.Register<BubbleQueueService>(Lifetime.Singleton);
            builder.Register<IBubbleResolveService, BubbleResolveService>(Lifetime.Singleton);
            builder.Register<IBubbleFieldScrollService, BubbleFieldScrollService>(Lifetime.Singleton);
        }

        private void RegisterConfigs(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gunConfig);
            builder.RegisterInstance(_levelData);
            builder.RegisterInstance(_bubbleCatalog);
        }
        
        private void RegisterComponentOnScene(IContainerBuilder builder)
        {
            builder.RegisterComponent(_bubbleGun).AsSelf();
            builder.RegisterComponent(_gameLogic).AsSelf();
            builder.RegisterComponent(_grid).AsSelf();
        }
    }
}
