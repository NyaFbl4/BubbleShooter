using MessagePipe;
using Project.Scripts.GameManager;
using Project.Scripts.Systems.UI;
using Project.Scripts.Systems.UI.Dtos;
using VContainer;

namespace Project.Scripts.UI.SettingsUI
{
    public class SettingsUIPresenter : LayoutPresenterBase<ISettingsUIView>, ISettingsUIPresenter, IGameStartListener, IGameFinishListener
    {
        [Inject] private readonly IPublisher<HidePopupDto> _hidePopupPublisher;

        private bool _musicEnabled = true;
        private bool _soundEnabled = true;
        private float _musicVolume = 1f;
        private float _soundVolume = 1f;

        public override void Initialize()
        {
            base.Initialize();
            IGameListener.Register(this);

            _layoutView.CloseClicked += OnCloseClicked;
            _layoutView.MusicToggleClicked += OnMusicToggleClicked;
            _layoutView.SoundToggleClicked += OnSoundToggleClicked;
            _layoutView.MusicVolumeChanged += OnMusicVolumeChanged;
            _layoutView.SoundVolumeChanged += OnSoundVolumeChanged;

            _layoutView.SetMusicEnabled(_musicEnabled);
            _layoutView.SetSoundEnabled(_soundEnabled);
            _layoutView.SetMusicVolume(_musicVolume);
            _layoutView.SetSoundVolume(_soundVolume);
        }

        public override void Dispose()
        {
            _layoutView.CloseClicked -= OnCloseClicked;
            _layoutView.MusicToggleClicked -= OnMusicToggleClicked;
            _layoutView.SoundToggleClicked -= OnSoundToggleClicked;
            _layoutView.MusicVolumeChanged -= OnMusicVolumeChanged;
            _layoutView.SoundVolumeChanged -= OnSoundVolumeChanged;

            IGameListener.Unregister(this);
            base.Dispose();
        }

        public void OnStartGame()
        {
            HideSettingsPopup();
        }

        public void OnFinishGame()
        {
            HideSettingsPopup();
        }

        private void OnCloseClicked()
        {
            HideSettingsPopup();
        }

        private void OnMusicToggleClicked()
        {
            _musicEnabled = !_musicEnabled;
            _layoutView.SetMusicEnabled(_musicEnabled);
        }

        private void OnSoundToggleClicked()
        {
            _soundEnabled = !_soundEnabled;
            _layoutView.SetSoundEnabled(_soundEnabled);
        }

        private void OnMusicVolumeChanged(float value)
        {
            _musicVolume = value;
        }

        private void OnSoundVolumeChanged(float value)
        {
            _soundVolume = value;
        }

        private void HideSettingsPopup()
        {
            _hidePopupPublisher?.Publish(new HidePopupDto
            {
                TargetPopUpType = typeof(ISettingsUIPresenter)
            });
        }
    }
}
