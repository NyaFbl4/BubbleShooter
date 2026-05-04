using Bubbles;
using BubbleGun;
using GameLogic;
using MessagePipe;
using Project.Scripts.GameManager;
using Project.Scripts.Systems.UI;
using Project.Scripts.Systems.UI.Dtos;
using Project.Scripts.UI.PauseUI;
using UnityEngine;
using VContainer;

namespace Project.Scripts.UI.LevelUIView
{
    public class LevelUIPresenter : LayoutPresenterBase<LevelUIView>, ILevelUIPresenter, IGameStartListener, IGameFinishListener
    {
        [Inject] private readonly BubbleQueueService _queue;
        [Inject] private readonly BubbleCatalog _catalog;
        [Inject] private readonly BubbleShotsService _shots;
        [Inject] private readonly GunConfig _gunConfig;
        [Inject] private readonly IGameManagerService _gameManagerService;
        [Inject] private readonly IPublisher<SwapBubbleCommandDto> _swapPublisher;
        [Inject] private readonly IPublisher<ShowPopupDto> _showPopUpPublisher;
        [Inject] private readonly IPublisher<HidePopupDto> _hidePopUpPublisher;

        /*[Inject]
        public void Construct(
            BubbleQueueService queue,
            BubbleCatalog catalog,
            BubbleShotsService shots,
            GunConfig gunConfig,
            IPublisher<SwapBubbleCommandDto> swapPublisher)
        {
            _queue = queue;
            _catalog = catalog;
            _shots = shots;
            _gunConfig = gunConfig;
            _swapPublisher = swapPublisher;
        }*/

        public override void Initialize()
        {
            base.Initialize();
            IGameListener.Register(this);

            _layoutView.ChangeBubbleBtnClicked += OnChangeBubbleClicked;
            _layoutView.PauseBtnClicked += OnPauseClicked;
            if (_queue != null)
                _queue.QueueChanged += RefreshQueueView;
            if (_shots != null)
                _shots.ShotsChanged += OnShotsChanged;

            RefreshQueueView();
            OnShotsChanged(_shots?.ShotsLeft ?? 0);
            SetScoreText("0");
            SetCurrentProgress(0);
        }

        public override void Dispose()
        {
            _layoutView.ChangeBubbleBtnClicked -= OnChangeBubbleClicked;
            _layoutView.PauseBtnClicked -= OnPauseClicked;
            if (_queue != null)
                _queue.QueueChanged -= RefreshQueueView;
            if (_shots != null)
                _shots.ShotsChanged -= OnShotsChanged;

            IGameListener.Unregister(this);
            base.Dispose();
        }

        public void RefreshQueueView()
        {
            if (_queue == null)
            {
                SetNextBubbleSprite(null);
                return;
            }

            var currentSprite = _queue.HasCurrent ? ResolveBubbleSprite(_queue.CurrentType) : null;
            var nextSprite = _queue.HasNext ? ResolveBubbleSprite(_queue.NextType) : null;
            
            SetNextBubbleSprite(nextSprite);
            UpdateSwapEnabled();
        }

        public void SetNextBubbleSprite(Sprite sprite)
        {
            _layoutView.SetNextBubbleSprite(sprite);
        }

        public Sprite ResolveBubbleSprite(EBubbleType type)
        {
            if (_catalog == null)
                return null;

            if (!_catalog.TryGet(type, out var def) || def == null)
                return null;

            if (def.Sprite != null)
                return def.Sprite;

            if (def.Prefab == null)
                return null;

            var prefabRenderer = def.Prefab.GetComponentInChildren<SpriteRenderer>();
            return prefabRenderer != null ? prefabRenderer.sprite : null;
        }

        private void OnChangeBubbleClicked()
        {
            if (!CanSwap())
                return;

            _swapPublisher?.Publish(new SwapBubbleCommandDto());
        }

        private void OnPauseClicked()
        {
            _gameManagerService?.PauseGame();
            _showPopUpPublisher?.Publish(new ShowPopupDto
            {
                TargetPopUpType = typeof(IPauseUIPresenter)
            });
        }

        private void OnShotsChanged(int shotsLeft)
        {
            _layoutView.SetCurrentBubblesCountText(shotsLeft.ToString());
            UpdateSwapEnabled();
        }

        public void SetScoreText(string text)
        {
            _layoutView.SetScoreText(text);
        }

        private void UpdateSwapEnabled()
        {
            _layoutView.SetSwapButtonEnabled(CanSwap());
        }

        private bool CanSwap()
        {
            if (_gunConfig == null || !_gunConfig.AllowSwap)
                return false;

            if (_shots != null && !_shots.HasShots)
                return false;

            if (_queue == null)
                return false;

            return _queue.HasCurrent && _queue.HasNext;
        }

        public void SetCurrentProgress(int progress)
        {
            _layoutView.SetProgress(progress);
        }
        
        public void OnStartGame()
        {
            /*_showPopUpPublisher.Publish(new ShowPopupDto
            {
                TargetPopUpType = typeof(ILevelUIPresenter)
            });*/
        }

        public void OnFinishGame()
        {
            /*_hidePopUpPublisher.Publish(new HidePopupDto
            {
                TargetPopUpType = typeof(ILevelUIPresenter)
            });*/
        }
    }
}
