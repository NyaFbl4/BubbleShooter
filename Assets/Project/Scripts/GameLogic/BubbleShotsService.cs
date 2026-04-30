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
}
