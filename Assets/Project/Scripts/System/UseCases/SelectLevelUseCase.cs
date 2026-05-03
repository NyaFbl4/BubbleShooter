using System;
using BubbleField;
using MessagePipe;
using Project.Scripts.GameManager;
using Project.Scripts.Systems.UI.Dtos;
using Project.Scripts.UI.LevelMapUI;
using Project.Scripts.UI.LevelUIView;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Project.Scripts.System.UseCases
{
    public class SelectLevelUseCase : IInitializable, IDisposable
    {
        [Inject] private readonly ISubscriber<SelectLevelDto> _selectLevelSubscriber;
        [Inject] private readonly IPublisher<ShowPopupDto> _showPopupPublisher;
        [Inject] private readonly IPublisher<HidePopupDto> _hidePopupPublisher;
        [Inject] private readonly BubbleLevelSelectionService _levelSelectionService;
        [Inject] private readonly IGameManagerService _gameManagerService;

        private IDisposable _subscription = DisposableBag.Empty;

        public void Initialize()
        {
            _subscription = _selectLevelSubscriber.Subscribe(Handle);
        }

        private void Handle(SelectLevelDto message)
        {
            if (message == null)
                return;

            if (_levelSelectionService == null)
            {
                Debug.LogWarning("SelectLevelUseCase: BubbleLevelSelectionService is null.");
                return;
            }

            if (!_levelSelectionService.SelectLevel(message.LevelNumber))
                return;

            Debug.Log($"Start level: {message.LevelNumber} ({_levelSelectionService.CurrentLevelName})");

            _hidePopupPublisher?.Publish(new HidePopupDto
            {
                TargetPopUpType = typeof(ILevelMapUIPresenter)
            });

            _showPopupPublisher?.Publish(new ShowPopupDto
            {
                TargetPopUpType = typeof(ILevelUIPresenter)
            });

            _gameManagerService?.StartGame();
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}
