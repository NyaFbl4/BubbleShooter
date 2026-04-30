using System;
using Project.Scripts.Systems.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Scripts.UI.LevelUIView
{
    public class LevelUIView : LayoutViewBase, ILevelUIView
    {
        private Button _changeBubbleBtn;
        //private VisualElement _currentBubbleImg;
        private VisualElement _nextBubbleImg;
        private VisualElement _progressFill;
        private Label _progressLbl;
        private Label _currentBubbleLbl;

        public event Action ChangeBubbleBtnClicked;

        public override void Awake()
        {
            base.Awake();

            _changeBubbleBtn = _root.Q<Button>("ChangeBubbleButton");
            var currentBubbleContainer = _root.Q<VisualElement>("CurrentBubbleContainer");
            //_currentBubbleImg = currentBubbleContainer?.Q<VisualElement>("BubbleImg");
            _nextBubbleImg = _root.Q<VisualElement>("ChangeBubbleImg");
            _progressLbl = _root.Q<Label>("ProgressLbl");
            _currentBubbleLbl = _root.Q<Label>("CurrentBubblesText");
            _progressFill = _root.Q<VisualElement>("ProgressFill");

            if (_changeBubbleBtn != null)
                _changeBubbleBtn.clicked += OnChangeBubbleBtnClicked;

            SetProgress(0);
            base.Show();
        }

        private void OnDestroy()
        {
            if (_changeBubbleBtn != null)
                _changeBubbleBtn.clicked -= OnChangeBubbleBtnClicked;
        }

        public void SetProgress(int progress)
        {
            if (_progressFill == null)
                return;

            _progressFill.style.width = Length.Percent(Mathf.Clamp(progress, 0, 100));
            _progressFill.MarkDirtyRepaint();
        }

        public void SetSwapButtonEnabled(bool enabled)
        {
            _changeBubbleBtn?.SetEnabled(enabled);
        }

        public void SetCurrentBubblesCountText(string text)
        {
            if (_currentBubbleLbl == null)
                return;

            _currentBubbleLbl.text = text;
        }

        public void SetScoreText(string text)
        {
            if (_progressLbl == null)
                return;

            _progressLbl.text = text;
        }

        public void SetNextBubbleSprite(Sprite sprite)
        {
            SetBubbleSprite(_nextBubbleImg, sprite);
        }

        private static void SetBubbleSprite(VisualElement target, Sprite sprite)
        {
            if (target == null)
                return;

            if (sprite == null)
            {
                target.style.backgroundImage = StyleKeyword.None;
                return;
            }

            target.style.backgroundImage = new StyleBackground(sprite);
        }

        private void OnChangeBubbleBtnClicked()
        {
            ChangeBubbleBtnClicked?.Invoke();
        }
    }
}
