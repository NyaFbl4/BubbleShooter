using System;
using BubbleField;
using GameLogic;
using MessagePipe;
using Project.Scripts.Systems.UI.Dtos;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Project.Scripts.System.UseCases
{
    public class SaveLevelStarsProgressUseCase : IInitializable, IDisposable
    {
        private const string LevelStarsKeyPrefix = "level_stars_";
        private const string LevelPassedKeyPrefix = "level_passed_";

        [Inject] private readonly ISubscriber<GameStatusCommandDto> _gameStatusSubscriber;
        [Inject] private readonly BubbleScoreService _scoreService;
        [Inject] private readonly BubbleLevelSelectionService _levelSelectionService;
        [Inject] private readonly BubbleLevelData _levelData;

        private IDisposable _subscription = DisposableBag.Empty;

        public void Initialize()
        {
            _subscription = _gameStatusSubscriber.Subscribe(Handle);
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }

        private void Handle(GameStatusCommandDto dto)
        {
            if (dto == null || dto.Command != EGameStatusCommand.ShowWinAndFinish)
                return;

            if (_levelSelectionService == null)
                return;

            var levelNumber = _levelSelectionService.CurrentLevelNumber;
            if (levelNumber <= 0)
                return;

            var score = _scoreService?.Score ?? 0;
            var stars = CalculateStarsForScore(score);
            SaveBestStars(levelNumber, stars);
            SavePassedLevel(levelNumber);
        }

        private int CalculateStarsForScore(int score)
        {
            if (_levelData == null)
                return 3;

            if (_levelData.ThreeStarPoints > 0 && score >= _levelData.ThreeStarPoints)
                return 3;
            if (_levelData.TwoStarPoints > 0 && score >= _levelData.TwoStarPoints)
                return 2;
            if (_levelData.OneStarPoints > 0 && score >= _levelData.OneStarPoints)
                return 1;

            return 1;
        }

        private static void SaveBestStars(int levelNumber, int stars)
        {
            var safeStars = Mathf.Clamp(stars, 1, 3);
            var key = LevelStarsKeyPrefix + levelNumber;
            var previousBest = PlayerPrefs.GetInt(key, 0);
            if (safeStars <= previousBest)
                return;

            PlayerPrefs.SetInt(key, safeStars);
            PlayerPrefs.Save();
        }

        private static void SavePassedLevel(int levelNumber)
        {
            var key = LevelPassedKeyPrefix + levelNumber;
            if (PlayerPrefs.GetInt(key, 0) == 1)
                return;

            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }
    }
}
