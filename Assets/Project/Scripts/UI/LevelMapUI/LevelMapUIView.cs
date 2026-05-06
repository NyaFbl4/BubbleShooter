using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Systems.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Scripts.UI.LevelMapUI
{
    public class LevelMapUIView : LayoutViewBase
    {
        private const string LevelStarsKeyPrefix = "level_stars_";
        private const string LevelPassedKeyPrefix = "level_passed_";
        private const string LevelButtonSpritePathPrefix = "LevelsMap/Parts/buttons/level_btn_";
        private const string DigitSpritePathPrefix = "LevelsMap/Parts/buttons/num_a_";

        private ScrollView _scroll;
        private VisualElement _bottomAnchor;
        private bool _needInitBottom;

        private readonly Dictionary<int, Button> _levelButtons = new();
        private readonly Dictionary<int, Sprite> _buttonSprites = new();
        private readonly Dictionary<int, Sprite> _digitSprites = new();
        public event Action<int> LevelClicked;

        public override void Awake()
        {
            base.Awake();

            _scroll = _root.Q<ScrollView>("LevelsMapScroll");
            _bottomAnchor = _root.Q<VisualElement>("Map1");
            _scroll?.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            RegisterLevelButtons();
            ApplyLevelButtonsProgressVisual();
            ApplyLevelButtonsLockState();
        }

        public override async UniTask ShowAsync()
        {
            _needInitBottom = true;
            ApplyLevelButtonsProgressVisual();
            ApplyLevelButtonsLockState();
            await base.ShowAsync();
        }
        
        private bool TryParseLevelId(string name, out int id)
        {
            id = 0;
            var suffix = name.Replace("level_", "");
            return int.TryParse(suffix, out id);
        }

        private void RegisterLevelButtons()
        {
            _levelButtons.Clear();
            var registeredCount = 0;

            _root.Query<VisualElement>()
                .ForEach(element =>
                {
                    if (string.IsNullOrEmpty(element.name) || !element.name.StartsWith("level_"))
                        return;

                    if (!TryParseLevelId(element.name, out var levelId))
                        return;

                    var button = element as Button ?? element.Q<Button>();
                    if (button == null)
                        return;

                    _levelButtons[levelId] = button;
                    button.text = string.Empty;
                    ApplyLevelNumberVisual(button, levelId);
                    button.clicked += () => OnLevelButtonClicked(levelId);
                    registeredCount++;
                });

            Debug.Log($"LevelMapUIView: registered level buttons = {registeredCount}");
        }

        private void ApplyLevelButtonsProgressVisual()
        {
            foreach (var pair in _levelButtons)
            {
                var levelNumber = pair.Key;
                var button = pair.Value;
                if (button == null)
                    continue;

                var stars = Mathf.Clamp(PlayerPrefs.GetInt(LevelStarsKeyPrefix + levelNumber, 0), 0, 3);
                if (stars <= 0)
                    continue;

                var sprite = GetButtonSprite(stars);
                if (sprite == null)
                    continue;

                button.style.backgroundImage = new StyleBackground(sprite);
            }
        }

        private Sprite GetButtonSprite(int stars)
        {
            var safeStars = Mathf.Clamp(stars, 1, 3);
            if (_buttonSprites.TryGetValue(safeStars, out var cached) && cached != null)
                return cached;

            var sprite = Resources.Load<Sprite>(LevelButtonSpritePathPrefix + safeStars);
            _buttonSprites[safeStars] = sprite;
            return sprite;
        }

        private void ApplyLevelButtonsLockState()
        {
            foreach (var pair in _levelButtons)
            {
                var levelNumber = pair.Key;
                var button = pair.Value;
                if (button == null)
                    continue;

                // Keep visual state unchanged. Lock is handled in click callback.
                button.SetEnabled(true);
            }
        }

        private void OnLevelButtonClicked(int levelNumber)
        {
            if (!IsLevelUnlocked(levelNumber))
            {
                Debug.Log($"LevelMapUIView: level {levelNumber} is locked.");
                return;
            }

            LevelClicked?.Invoke(levelNumber);
        }

        private bool IsLevelUnlocked(int levelNumber)
        {
            if (levelNumber <= 1)
                return true;

            for (var i = 1; i < levelNumber; i++)
            {
                if (PlayerPrefs.GetInt(LevelPassedKeyPrefix + i, 0) < 1)
                    return false;
            }

            return true;
        }

        private void ApplyLevelNumberVisual(Button button, int levelNumber)
        {
            if (button == null)
                return;

            var num1 = button.Q<VisualElement>("num_1");
            var num2 = button.Q<VisualElement>("num_2");
            if (num1 == null)
                return;

            var safeLevel = Mathf.Max(0, levelNumber);
            if (safeLevel < 10)
            {
                SetDigitSprite(num1, safeLevel);
                if (num2 != null)
                    num2.style.display = DisplayStyle.None;
                return;
            }

            var twoDigits = safeLevel % 100;
            var tens = twoDigits / 10;
            var ones = twoDigits % 10;

            SetDigitSprite(num1, tens);
            if (num2 != null)
            {
                num2.style.display = DisplayStyle.Flex;
                SetDigitSprite(num2, ones);
            }
        }

        private void SetDigitSprite(VisualElement target, int digit)
        {
            if (target == null)
                return;

            var safeDigit = Mathf.Clamp(digit, 0, 9);
            var sprite = GetDigitSprite(safeDigit);
            if (sprite == null)
                return;

            target.style.backgroundImage = new StyleBackground(sprite);
        }

        private Sprite GetDigitSprite(int digit)
        {
            var safeDigit = Mathf.Clamp(digit, 0, 9);
            if (_digitSprites.TryGetValue(safeDigit, out var cached) && cached != null)
                return cached;

            var sprite = Resources.Load<Sprite>(DigitSpritePathPrefix + safeDigit);
            _digitSprites[safeDigit] = sprite;
            return sprite;
        }

        private void OnGeometryChanged(GeometryChangedEvent _)
        {
            TrySetBottom();
        }

        private void TrySetBottom()
        {
            if (!_needInitBottom || _scroll == null)
                return;

            var maxY = Mathf.Max(
                0f,
                _scroll.contentContainer.layout.height - _scroll.contentViewport.layout.height);

            if (maxY <= 0.01f)
                return; // Layout not ready yet.

            if (_bottomAnchor != null)
                _scroll.ScrollTo(_bottomAnchor);
            else
                _scroll.scrollOffset = new Vector2(0f, maxY);

            _needInitBottom = false;
        }


        private void OnDestroy()
        {
            _scroll?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
    }
}
