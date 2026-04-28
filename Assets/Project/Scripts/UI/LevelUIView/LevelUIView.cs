using System;
using MessagePipe;
using Project.Scripts.Systems.UI;
using Project.Scripts.UI.Dto;
using UnityEngine.UIElements;

namespace Project.Scripts.UI.LevelUIView
{
    public class LevelUIView : LayoutViewBase, ILevelUIView
    {
        private Button _changeBubbleBtn;
        private VisualElement _nextBubbleImg;
        private Label _progressLbl;
        private VisualElement _progressFill;
        
        private IPublisher<SwapBubbleCommandDto> _swapBubblePublisher;

        public event Action ChangeBubbleBtnClicked;
        
        public override void Awake()
        {
            base.Awake();
            
            _changeBubbleBtn = _root.Q<Button>("ChangeBubbleBtn");
            _nextBubbleImg =  _root.Q<VisualElement>("NextBubbleImg");
            _progressLbl = _root.Q<Label>("ProgressLbl");
            _progressFill = _root.Q<VisualElement>("ProgressFill");

            _changeBubbleBtn.clicked += OnChangeBubbleBtClicked;
        }

        private void OnDestroy()
        {
            _changeBubbleBtn.clicked -= OnChangeBubbleBtClicked;
        }

        private void OnChangeBubbleBtClicked()
        {
            ChangeBubbleBtnClicked?.Invoke();
        }
    }
}