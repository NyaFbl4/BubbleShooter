using MessagePipe;
using Project.Scripts.GameManager;
using Project.Scripts.Systems.UI;
using Project.Scripts.Systems.UI.Dtos;
using Project.Scripts.UI.LevelMapUI;
using Project.Scripts.UI.SettingsUI;
using VContainer;

namespace Project.Scripts.UI.PauseUI
{
    public class PauseUIPresenter : LayoutPresenterBase<IPauseUIView>, IPauseUIPresenter, IGameStartListener, IGameFinishListener
    {
        [Inject] private readonly IGameManagerService _gameManagerService;
        [Inject] private readonly IPublisher<ShowPopupDto> _showPopupPublisher;
        [Inject] private readonly IPublisher<HidePopupDto> _hidePopupPublisher;

        public override void Initialize()
        {
            base.Initialize();
            IGameListener.Register(this);

            _layoutView.PlayClicked += OnPlayClicked;
            _layoutView.SettingsClicked += OnSettingsClicked;
            _layoutView.MenuClicked += OnMenuClicked;
        }

        public override void Dispose()
        {
            _layoutView.PlayClicked -= OnPlayClicked;
            _layoutView.SettingsClicked -= OnSettingsClicked;
            _layoutView.MenuClicked -= OnMenuClicked;

            IGameListener.Unregister(this);
            base.Dispose();
        }

        public void OnStartGame()
        {
            //HidePausePopup();
        }

        public void OnFinishGame()
        {
            HidePausePopup();
        }

        private void OnPlayClicked()
        {
            _gameManagerService?.ResumeGame();
            HidePausePopup();
        }

        private void OnSettingsClicked()
        {
            _showPopupPublisher?.Publish(new ShowPopupDto
            {
                TargetPopUpType = typeof(ISettingsUIPresenter)
            });
        }

        private void OnMenuClicked()
        {
            _gameManagerService?.FinishGame();
            HidePausePopup();
        }

        private void HidePausePopup()
        {
            _hidePopupPublisher?.Publish(new HidePopupDto
            {
                TargetPopUpType = typeof(IPauseUIPresenter)
            });
            
            _showPopupPublisher?.Publish((new ShowPopupDto
            {
                TargetPopUpType = typeof(ILevelMapUIPresenter)
            }));
        }
    }
}
