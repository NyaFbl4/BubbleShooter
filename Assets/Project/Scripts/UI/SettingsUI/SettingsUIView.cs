using System;
using Project.Scripts.Systems.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Scripts.UI.SettingsUI
{
    public class SettingsUIView : AnimatedPopupViewBase, ISettingsUIView
    {
        private const string ToggleOnPath = "UI/Settings Panel/on_1";
        private const string ToggleOffPath = "UI/Settings Panel/off_1";
        private const float VolumeStep = 0.05f;

        private Label _titleLabel;
        private Label _musicLabel;
        private Label _soundLabel;
        private Label _musicToggleStateLabel;
        private Label _soundToggleStateLabel;
        private Button _closeButton;
        private Button _musicToggleButton;
        private Button _soundToggleButton;
        private Button _musicVolumeMinusButton;
        private Button _musicVolumePlusButton;
        private Button _soundVolumeMinusButton;
        private Button _soundVolumePlusButton;
        private VisualElement _musicVolumeFill;
        private VisualElement _soundVolumeFill;
        private Sprite _toggleOnSprite;
        private Sprite _toggleOffSprite;
        private string _toggleOnText = "ON";
        private string _toggleOffText = "OFF";
        private float _musicVolume = 1f;
        private float _soundVolume = 1f;

        protected override string OverlayElementName => "settings-overlay";
        protected override string PanelElementName => "settings-panel";

        public event Action CloseClicked;
        public event Action MusicToggleClicked;
        public event Action SoundToggleClicked;
        public event Action<float> MusicVolumeChanged;
        public event Action<float> SoundVolumeChanged;

        public override void Awake()
        {
            base.Awake();

            _titleLabel = _root.Q<Label>("setting-title-label");
            _musicLabel = _root.Q<Label>("settings-music-label");
            _soundLabel = _root.Q<Label>("settings-sound-label");
            _musicToggleStateLabel = _root.Q<Label>("settings-music-label-on-of")
                                     ?? _root.Q<Label>("settings-music-toggle-state-label");
            _soundToggleStateLabel = _root.Q<Label>("settings-sound-label-on-of")
                                     ?? _root.Q<Label>("settings-sound-toggle-state-label");
            _closeButton = _root.Q<Button>("settings-close-button");
            _musicToggleButton = _root.Q<Button>("settings-music-toggle-button");
            _soundToggleButton = _root.Q<Button>("settings-sound-toggle-button");

            _musicVolumeMinusButton = _root.Q<Button>("settings-music-volume-minus-button");
            _musicVolumePlusButton = _root.Q<Button>("settings-music-volume-plus-button");
            _soundVolumeMinusButton = _root.Q<Button>("settings-sound-volume-minus-button");
            _soundVolumePlusButton = _root.Q<Button>("settings-sound-volume-plus-button");
            _musicVolumeFill = _root.Q<VisualElement>("settings-music-volume-fill");
            _soundVolumeFill = _root.Q<VisualElement>("settings-sound-volume-fill");

            if (_closeButton == null)
                Debug.LogError("SettingsUIView: Button 'settings-close-button' not found in UXML.");
            else
            {
                UIButtonAnimationUtility.EnableDefault(_closeButton);
                _closeButton.clicked += HandleCloseClicked;
            }

            if (_musicToggleButton == null)
                Debug.LogError("SettingsUIView: Button 'settings-music-toggle-button' not found in UXML.");
            else
            {
                UIButtonAnimationUtility.EnableDefault(_musicToggleButton);
                _musicToggleButton.clicked += HandleMusicToggleClicked;
            }

            if (_soundToggleButton == null)
                Debug.LogError("SettingsUIView: Button 'settings-sound-toggle-button' not found in UXML.");
            else
            {
                UIButtonAnimationUtility.EnableDefault(_soundToggleButton);
                _soundToggleButton.clicked += HandleSoundToggleClicked;
            }

            if (_musicVolumeMinusButton == null || _musicVolumePlusButton == null || _musicVolumeFill == null)
            {
                Debug.LogError("SettingsUIView: music volume controls are not found in UXML.");
            }
            else
            {
                UIButtonAnimationUtility.EnableDefault(_musicVolumeMinusButton);
                UIButtonAnimationUtility.EnableDefault(_musicVolumePlusButton);
                _musicVolumeMinusButton.clicked += HandleMusicVolumeMinusClicked;
                _musicVolumePlusButton.clicked += HandleMusicVolumePlusClicked;
            }

            if (_soundVolumeMinusButton == null || _soundVolumePlusButton == null || _soundVolumeFill == null)
            {
                Debug.LogError("SettingsUIView: sound volume controls are not found in UXML.");
            }
            else
            {
                UIButtonAnimationUtility.EnableDefault(_soundVolumeMinusButton);
                UIButtonAnimationUtility.EnableDefault(_soundVolumePlusButton);
                _soundVolumeMinusButton.clicked += HandleSoundVolumeMinusClicked;
                _soundVolumePlusButton.clicked += HandleSoundVolumePlusClicked;
            }

            _toggleOnSprite = LoadToggleSprite(ToggleOnPath);
            _toggleOffSprite = LoadToggleSprite(ToggleOffPath);

            if (_toggleOnSprite == null || _toggleOffSprite == null)
                Debug.LogWarning("SettingsUIView: ON/OFF sprites not found in Resources/UI/Settings Panel.");

            if (_musicToggleStateLabel != null)
                _musicToggleStateLabel.text = _toggleOnText;

            if (_soundToggleStateLabel != null)
                _soundToggleStateLabel.text = _toggleOnText;
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.clicked -= HandleCloseClicked;

            if (_musicToggleButton != null)
                _musicToggleButton.clicked -= HandleMusicToggleClicked;

            if (_soundToggleButton != null)
                _soundToggleButton.clicked -= HandleSoundToggleClicked;

            if (_musicVolumeMinusButton != null)
                _musicVolumeMinusButton.clicked -= HandleMusicVolumeMinusClicked;

            if (_musicVolumePlusButton != null)
                _musicVolumePlusButton.clicked -= HandleMusicVolumePlusClicked;

            if (_soundVolumeMinusButton != null)
                _soundVolumeMinusButton.clicked -= HandleSoundVolumeMinusClicked;

            if (_soundVolumePlusButton != null)
                _soundVolumePlusButton.clicked -= HandleSoundVolumePlusClicked;
        }

        public void SetMusicEnabled(bool isEnabled)
        {
            ApplyToggleVisual(_musicToggleButton, _musicToggleStateLabel, isEnabled);
        }

        public void SetSoundEnabled(bool isEnabled)
        {
            ApplyToggleVisual(_soundToggleButton, _soundToggleStateLabel, isEnabled);
        }

        public void SetMusicVolume(float value)
        {
            _musicVolume = Mathf.Clamp01(value);
            ApplyVolumeFill(_musicVolumeFill, _musicVolume);
        }

        public void SetSoundVolume(float value)
        {
            _soundVolume = Mathf.Clamp01(value);
            ApplyVolumeFill(_soundVolumeFill, _soundVolume);
        }

        private void HandleCloseClicked()
        {
            CloseClicked?.Invoke();
        }

        private void HandleMusicToggleClicked()
        {
            MusicToggleClicked?.Invoke();
        }

        private void HandleSoundToggleClicked()
        {
            SoundToggleClicked?.Invoke();
        }

        private void HandleMusicVolumeMinusClicked()
        {
            ChangeMusicVolume(-VolumeStep);
        }

        private void HandleMusicVolumePlusClicked()
        {
            ChangeMusicVolume(VolumeStep);
        }

        private void HandleSoundVolumeMinusClicked()
        {
            ChangeSoundVolume(-VolumeStep);
        }

        private void HandleSoundVolumePlusClicked()
        {
            ChangeSoundVolume(VolumeStep);
        }

        private void ChangeMusicVolume(float delta)
        {
            var newValue = Mathf.Clamp01(_musicVolume + delta);
            if (Mathf.Approximately(newValue, _musicVolume))
                return;

            SetMusicVolume(newValue);
            MusicVolumeChanged?.Invoke(newValue);
        }

        private void ChangeSoundVolume(float delta)
        {
            var newValue = Mathf.Clamp01(_soundVolume + delta);
            if (Mathf.Approximately(newValue, _soundVolume))
                return;

            SetSoundVolume(newValue);
            SoundVolumeChanged?.Invoke(newValue);
        }

        private static void ApplyVolumeFill(VisualElement fill, float value)
        {
            if (fill == null)
                return;

            fill.style.width = Length.Percent(Mathf.Clamp01(value) * 100f);
        }

        private void ApplyToggleVisual(Button button, Label stateLabel, bool isEnabled)
        {
            if (button == null)
                return;

            var stateText = isEnabled ? _toggleOnText : _toggleOffText;
            if (stateLabel != null)
                stateLabel.text = stateText;

            var sprite = isEnabled ? _toggleOnSprite : _toggleOffSprite;
            if (sprite != null)
            {
                button.style.backgroundImage = new StyleBackground(sprite);
                button.text = string.Empty;
                return;
            }

            button.text = stateText;
        }

        private static Sprite LoadToggleSprite(string resourcePath)
        {
            var sprites = Resources.LoadAll<Sprite>(resourcePath);
            if (sprites != null && sprites.Length > 0)
                return sprites[0];

            return Resources.Load<Sprite>(resourcePath);
        }
    }
}
