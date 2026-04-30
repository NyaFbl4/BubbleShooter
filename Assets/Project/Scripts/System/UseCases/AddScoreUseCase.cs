using BubbleField;
using GameLogic;
using Project.Scripts.UI.LevelUIView;
using UnityEngine;

namespace Project.Scripts.System.UseCases
{
    public class AddScoreUseCase
    {
        private readonly BubbleScoreService _scoreService;
        private readonly BubbleLevelData _levelData;
        private readonly ILevelUIPresenter _levelPresenter;

        public AddScoreUseCase(
            BubbleScoreService scoreService,
            BubbleLevelData levelData,
            ILevelUIPresenter levelPresenter)
        {
            _scoreService = scoreService;
            _levelData = levelData;
            _levelPresenter = levelPresenter;
        }

        public void ResetScore()
        {
            _scoreService?.Reset();
            PushUi();
        }

        public void AddDestroyedBubbles(int destroyedCount)
        {
            _scoreService?.AddDestroyedBubbles(destroyedCount);
            PushUi();
        }

        private void PushUi()
        {
            if (_levelPresenter == null || _scoreService == null)
                return;

            var score = _scoreService.Score;
            _levelPresenter.SetScoreText(score.ToString());

            var target = _levelData != null ? _levelData.ThreeStarPoints : 0;
            var progressPercent = target > 0
                ? Mathf.Clamp(Mathf.RoundToInt(score * 100f / target), 0, 100)
                : 0;
            Debug.Log($"AddScoreUseCase {progressPercent}%");
            Debug.Log($"AddScoreUseCase target {target}");
            _levelPresenter.SetCurrentProgress(progressPercent);
        }
    }
}
