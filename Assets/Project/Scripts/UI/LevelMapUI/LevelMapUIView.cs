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

        public override void Awake()
        {
            base.Awake();

            _scroll = _root.Q<ScrollView>("LevelsMapScroll");
            _bottomAnchor = _root.Q<VisualElement>("Map1");
            _scroll?.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public override async UniTask ShowAsync()
        {
            _needInitBottom = true;
            await base.ShowAsync();
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
