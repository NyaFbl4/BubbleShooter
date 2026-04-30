using System;
using UnityEngine;

namespace GameLogic
{
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
