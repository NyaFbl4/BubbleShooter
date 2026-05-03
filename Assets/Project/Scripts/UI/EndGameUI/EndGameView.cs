using System;
using System.Collections.Generic;
using Project.Scripts.Systems.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Scripts.UI.EndGame
{
    public class EndGameView : AnimatedPopupViewBase, IEndGameView
    {
        private static readonly string[] StarsSpriteByActiveCount =
        {
            "UI/EndGamePanel/star_4",
            "UI/EndGamePanel/star_3",
            "UI/EndGamePanel/star_2",
            "UI/EndGamePanel/star_1"
        };

        private readonly Dictionary<string, Sprite> _spriteCache = new();

        private Label _titleLabel;
        private Label _scoreLabel;
        private Label _completionLabel;
        private VisualElement _starsContainer;
        private Button _primaryActionButton;
        private Button _secondaryActionButton;

        protected override string OverlayElementName => "end-game-overlay";
        protected override string PanelElementName => "end-game-panel";

        public event Action PrimaryButtonClicked;
        public event Action SecondaryButtonClicked;

        public override void Awake()
        {
            base.Awake();

            QueryElements();
            EnsureRequiredElements();

            if (_primaryActionButton != null)
            {
                UIButtonAnimationUtility.EnableDefault(_primaryActionButton);
                _primaryActionButton.clicked += HandlePrimaryActionClicked;
            }

            if (_secondaryActionButton != null)
            {
                UIButtonAnimationUtility.EnableDefault(_secondaryActionButton);
                _secondaryActionButton.clicked += HandleSecondaryActionClicked;
            }

            SetCompletionVisible(false);
            SetStars(activeStarsCount: 0, totalStarsCount: 3);
        }

        private void OnDestroy()
        {
            if (_primaryActionButton != null)
                _primaryActionButton.clicked -= HandlePrimaryActionClicked;

            if (_secondaryActionButton != null)
                _secondaryActionButton.clicked -= HandleSecondaryActionClicked;
        }

        public void SetTitle(string message)
        {
            if (_titleLabel == null)
                return;

            _titleLabel.text = message;
        }

        public void SetScoreText(string text)
        {
            if (_scoreLabel == null)
                return;

            _scoreLabel.text = text;
        }

        public void SetScoreVisible(bool isVisible)
        {
            if (_scoreLabel == null)
                return;

            _scoreLabel.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetCompletionText(string text)
        {
            if (_completionLabel == null)
                return;

            _completionLabel.text = text;
        }

        public void SetCompletionVisible(bool isVisible)
        {
            if (_completionLabel == null)
                return;

            _completionLabel.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetStars(int activeStarsCount, int totalStarsCount = 3)
        {
            if (_starsContainer == null)
                return;

            var total = Mathf.Max(0, totalStarsCount);
            var active = Mathf.Clamp(activeStarsCount, 0, total);
            var normalizedActiveStars = NormalizeStarsCount(active, total);
            var starsSpritePath = StarsSpriteByActiveCount[normalizedActiveStars];
            ApplyBackgroundSprite(_starsContainer, starsSpritePath);
        }

        public void SetStarsVisible(bool isVisible)
        {
            if (_starsContainer == null)
                return;

            _starsContainer.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void QueryElements()
        {
            _titleLabel = _root.Q<Label>("end-game-title-label");
            _scoreLabel = _root.Q<Label>("end-game-score-label");
            _completionLabel = _root.Q<Label>("end-game-completion-label");
            _starsContainer = _root.Q<VisualElement>("end-game-stars-container");
            _primaryActionButton = _root.Q<Button>("end-game-primary-button") ?? _root.Q<Button>("end-game-menu-button");
            _secondaryActionButton = _root.Q<Button>("end-game-secondary-button");
        }

        private void EnsureRequiredElements()
        {
            if (_titleLabel == null ||
                _scoreLabel == null ||
                _starsContainer == null ||
                _primaryActionButton == null ||
                _secondaryActionButton == null)
            {
                Debug.LogError("EndGameView: Required elements not found in EndGameView.uxml.");
            }
        }

        private static int NormalizeStarsCount(int activeStarsCount, int totalStarsCount)
        {
            if (totalStarsCount <= 0)
                return 0;

            var maxVisualStars = StarsSpriteByActiveCount.Length - 1;
            var normalized = Mathf.RoundToInt((float)activeStarsCount / totalStarsCount * maxVisualStars);
            return Mathf.Clamp(normalized, 0, maxVisualStars);
        }

        private void ApplyBackgroundSprite(VisualElement element, string spritePath)
        {
            if (element == null)
                return;

            var sprite = GetSprite(spritePath);
            element.style.backgroundImage = sprite != null
                ? new StyleBackground(sprite)
                : new StyleBackground();
        }

        private Sprite GetSprite(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                return null;

            if (_spriteCache.TryGetValue(resourcePath, out var cachedSprite))
                return cachedSprite;

            var sprite = Resources.Load<Sprite>(resourcePath);
            _spriteCache[resourcePath] = sprite;
            return sprite;
        }

        private void HandlePrimaryActionClicked()
        {
            PrimaryButtonClicked?.Invoke();
        }

        private void HandleSecondaryActionClicked()
        {
            SecondaryButtonClicked?.Invoke();
        }
    }
}

