using System;
using UnityEngine;

namespace BubbleField
{
    public class BubbleLevelSelectionService
    {
        private readonly BubbleLevelData _runtimeLevelData;
        private readonly BubbleLevelData[] _mapLevels;

        public int CurrentLevelNumber { get; private set; }
        public string CurrentLevelName { get; private set; } = string.Empty;

        public BubbleLevelSelectionService(BubbleLevelData runtimeLevelData, BubbleLevelData[] mapLevels)
        {
            _runtimeLevelData = runtimeLevelData;
            _mapLevels = mapLevels ?? Array.Empty<BubbleLevelData>();
            CurrentLevelNumber = 0;
        }

        public bool SelectLevel(int levelNumber)
        {
            if (_runtimeLevelData == null)
            {
                Debug.LogWarning("BubbleLevelSelectionService: Runtime level data is null.");
                return false;
            }

            if (levelNumber <= 0 || levelNumber > _mapLevels.Length)
            {
                Debug.LogWarning($"BubbleLevelSelectionService: Level {levelNumber} is out of range. Configured levels: {_mapLevels.Length}.");
                return false;
            }

            var source = _mapLevels[levelNumber - 1];
            if (source == null)
            {
                Debug.LogWarning($"BubbleLevelSelectionService: Config for level {levelNumber} is null.");
                return false;
            }

            ApplySourceToRuntime(source);
            CurrentLevelNumber = levelNumber;
            CurrentLevelName = source.name;
            return true;
        }

        private void ApplySourceToRuntime(BubbleLevelData source)
        {
            _runtimeLevelData.Rows = source.Rows;
            _runtimeLevelData.Columns = source.Columns;
            _runtimeLevelData.NumBubbles = source.NumBubbles;

            _runtimeLevelData.Grid.Clear();
            if (source.Grid != null)
                _runtimeLevelData.Grid.AddRange(source.Grid);

            _runtimeLevelData.AvailableRandomTypes.Clear();
            if (source.AvailableRandomTypes != null)
                _runtimeLevelData.AvailableRandomTypes.AddRange(source.AvailableRandomTypes);

            _runtimeLevelData.OneStarPoints = source.OneStarPoints;
            _runtimeLevelData.TwoStarPoints = source.TwoStarPoints;
            _runtimeLevelData.ThreeStarPoints = source.ThreeStarPoints;
        }
    }
}
