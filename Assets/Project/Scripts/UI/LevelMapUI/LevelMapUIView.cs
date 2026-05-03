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
        private ScrollView _scroll;
        private VisualElement _bottomAnchor;
        private bool _needInitBottom;

        private readonly Dictionary<int, Button> _levelButtons = new();
        public event Action<int> LevelClicked;

        public override void Awake()
        {
            base.Awake();

            _scroll = _root.Q<ScrollView>("LevelsMapScroll");
            _bottomAnchor = _root.Q<VisualElement>("Map1");
            _scroll?.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            RegisterLevelButtons();
        }

        public override async UniTask ShowAsync()
        {
            _needInitBottom = true;
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
                    button.clicked += () => LevelClicked?.Invoke(levelId);
                    registeredCount++;
                });

            Debug.Log($"LevelMapUIView: registered level buttons = {registeredCount}");
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
