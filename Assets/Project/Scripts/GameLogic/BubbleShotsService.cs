using System;
using BubbleField;
using UnityEngine;

namespace GameLogic
{
    public class BubbleShotsService
    {
        private readonly BubbleLevelData _levelData;
        public int ShotsLeft { get;private set; }
        public bool HasShots => ShotsLeft > 0;
        public event Action<int> ShotsChanged;

        public BubbleShotsService(BubbleLevelData levelData)
        {
            _levelData = levelData;
        }

        public void ResetFromLevel()
        {
            ShotsLeft = Mathf.Max(0, _levelData != null ? _levelData.NumBubbles : 0);
            ShotsChanged?.Invoke(ShotsLeft);
        }

        public bool TryConsumeOne()
        {
            if (ShotsLeft <= 0) return false;
            ShotsLeft--;
            ShotsChanged?.Invoke(ShotsLeft);
            return true;
        }
    }

    public class BubbleScoreService
    {
        private const int DefaultBubbleScore = 10;

        public int Score { get; private set; }
        public event Action<int> ScoreChanged;

        public void Reset()
        {
            Score = 0;
            ScoreChanged?.Invoke(Score);
        }

        public void AddDestroyedBubbles(int destroyedCount)
        {
            if (destroyedCount <= 0)
                return;

            var add = Mathf.Max(0, destroyedCount) * DefaultBubbleScore;
            Score += add;
            ScoreChanged?.Invoke(Score);
        }
    }
}
