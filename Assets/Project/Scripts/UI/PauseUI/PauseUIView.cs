using System;
using Project.Scripts.Systems.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Scripts.UI.PauseUI
{
    public class PauseUIView : AnimatedPopupViewBase, IPauseUIView
    {
        private Label _titleLabel;
        private Button _playButton;
        private Button _settingsButton;
        private Button _menuButton;

        protected override string OverlayElementName => "pause-overlay";
        protected override string PanelElementName => "pause-panel";

        public event Action PlayClicked;
        public event Action SettingsClicked;
        public event Action MenuClicked;

        public override void Awake()
        {
            base.Awake();

            _titleLabel = _root.Q<Label>("pause-title-label");
            _playButton = _root.Q<Button>("pause-play-button");
            _settingsButton = _root.Q<Button>("pause-settings-button");
            _menuButton = _root.Q<Button>("pause-menu-button");

            if (_titleLabel == null)
                Debug.LogError("PauseUIView: Label 'pause-title-label' not found in UXML.");

            if (_playButton == null)
                Debug.LogError("PauseUIView: Button 'pause-play-button' not found in UXML.");
            else
            {
                UIButtonAnimationUtility.EnableDefault(_playButton);
                _playButton.clicked += HandlePlayClicked;
            }

            if (_settingsButton == null)
                Debug.LogError("PauseUIView: Button 'pause-settings-button' not found in UXML.");
            else
            {
                UIButtonAnimationUtility.EnableDefault(_settingsButton);
                _settingsButton.clicked += HandleSettingsClicked;
            }

            if (_menuButton == null)
                Debug.LogError("PauseUIView: Button 'pause-menu-button' not found in UXML.");
            else
            {
                UIButtonAnimationUtility.EnableDefault(_menuButton);
                _menuButton.clicked += HandleMenuClicked;
            }
        }

        private void OnDestroy()
        {
            if (_playButton != null)
                _playButton.clicked -= HandlePlayClicked;

            if (_settingsButton != null)
                _settingsButton.clicked -= HandleSettingsClicked;

            if (_menuButton != null)
                _menuButton.clicked -= HandleMenuClicked;
        }

        private void HandlePlayClicked()
        {
            PlayClicked?.Invoke();
        }

        private void HandleSettingsClicked()
        {
            SettingsClicked?.Invoke();
        }

        private void HandleMenuClicked()
        {
            MenuClicked?.Invoke();
        }
    }
}
