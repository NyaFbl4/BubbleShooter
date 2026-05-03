using System;
using GameLogic;
using MessagePipe;
using Project.Scripts.GameManager;
using Project.Scripts.Systems.UI;
using Project.Scripts.Systems.UI.Dtos;
using Project.Scripts.UI.LevelMapUI;
using Project.Scripts.UI.LevelUIView;
using VContainer;
using VContainer.Unity;

namespace Project.Scripts.UI.EndGame
{
    public class EndGamePresenter : LayoutPresenterBase<IEndGameView>, IEndGamePresenter, IGameStartListener, IDisposable
    {
        [Inject] private readonly IGameManagerService _gameManagerService;
        [Inject] private readonly BubbleScoreService _scoreService;
        [Inject] private readonly ISubscriber<GameStatusCommandDto> _gameStatusSubscriber;
        [Inject] private readonly IPublisher<ShowPopupDto> _showPopupPublisher;
        [Inject] private readonly IPublisher<HidePopupDto> _hidePopupPublisher;
        private IDisposable _subscription = DisposableBag.Empty;

        public override void Initialize()
        {
            base.Initialize();
            _subscription = _gameStatusSubscriber.Subscribe(OnGameStatusChanged);
            _layoutView.PrimaryButtonClicked += HandlePrimaryButtonClicked;
            _layoutView.SecondaryButtonClicked += HandleSecondaryButtonClicked;
            IGameListener.Register(this);
        }

        public override void Dispose()
        {
            _subscription.Dispose();
            _layoutView.PrimaryButtonClicked -= HandlePrimaryButtonClicked;
            _layoutView.SecondaryButtonClicked -= HandleSecondaryButtonClicked;
            IGameListener.Unregister(this);
            base.Dispose();
        }

        public void ShowResult(bool isPassed, int score, int starsCount, int totalStarsCount, string completionText, int timeBonusPoints = 0)
        {
            _layoutView.SetTitle(isPassed ? "VICTORY" : "DEFEAT");
            _layoutView.SetScoreText($"Score: {Math.Max(0, score)}");
            _layoutView.SetScoreVisible(true);
            _layoutView.SetCompletionText(completionText ?? string.Empty);
            _layoutView.SetCompletionVisible(false);
            _layoutView.SetStarsVisible(true);
            _layoutView.SetStars(starsCount, totalStarsCount <= 0 ? 3 : totalStarsCount);
            _showPopupPublisher.Publish(new ShowPopupDto { TargetPopUpType = typeof(IEndGamePresenter) });
        }

        public void OnStartGame()
        {
            _hidePopupPublisher.Publish(new HidePopupDto { TargetPopUpType = typeof(IEndGamePresenter) });
        }

        private void HandlePrimaryButtonClicked()
        {
            _hidePopupPublisher.Publish(new HidePopupDto
            {
                TargetPopUpType = typeof(IEndGamePresenter)
            });
            _showPopupPublisher.Publish(new ShowPopupDto
            {
                TargetPopUpType = typeof(ILevelUIPresenter)
            });
            _gameManagerService?.StartGame();
        }

        private void HandleSecondaryButtonClicked()
        {
            _hidePopupPublisher.Publish(new HidePopupDto
            {
                TargetPopUpType = typeof(IEndGamePresenter)
            });
            _hidePopupPublisher.Publish(new HidePopupDto
            {
                TargetPopUpType = typeof(ILevelUIPresenter)
            });
            _showPopupPublisher.Publish(new ShowPopupDto
            {
                TargetPopUpType = typeof(ILevelMapUIPresenter)
            });
        }

        private void OnGameStatusChanged(GameStatusCommandDto dto)
        {
            if (dto == null)
                return;

            var isWin = dto.Command == EGameStatusCommand.ShowWinAndFinish;
            var score = _scoreService?.Score ?? 0;
            ShowResult(isWin, score, isWin ? 3 : 0, 3, string.Empty);
        }
    }
}

