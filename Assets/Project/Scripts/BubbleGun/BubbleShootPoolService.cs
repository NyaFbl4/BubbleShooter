using System.Collections.Generic;
using BubbleField;
using Bubbles;

namespace BubbleGun
{
    public class BubbleShootPoolService : IBubbleShootPoolService
    {
        private readonly BubbleFieldGrid _grid;
        private readonly BubbleLevelData _levelData;
        private readonly BubbleCatalog _bubbleCatalog;

        public BubbleShootPoolService(BubbleFieldGrid grid, 
            BubbleLevelData levelData, BubbleCatalog bubbleCatalog)
        {
            _grid = grid;
            _levelData = levelData;
            _bubbleCatalog = bubbleCatalog;
        }

        public void Rebuild(List<EBubbleType> target)
        {
            target.Clear();
            TryFillFromField(target);
            if (target.Count == 0) TryFillFromLevelConfig(target);
            if (target.Count == 0) TryFillFromCatalog(target);
        }

        private void TryFillFromField(List<EBubbleType> target)
        {
            if (_grid == null) return;
            foreach (var cell in _grid.GetOccupiedCells())
            {
                if (!_grid.TryGetBubble(cell, out var bubble) || bubble == null) continue;
                AddIfSelectable(target, bubble.BubbleType);
            }
        }

        private void TryFillFromLevelConfig(List<EBubbleType> target)
        {
            if (_levelData?.AvailableRandomTypes == null) return;
            foreach (var type in _levelData.AvailableRandomTypes)
                AddIfSelectable(target, type);
        }

        private void TryFillFromCatalog(List<EBubbleType> target)
        {
            if (_bubbleCatalog?.Definitions == null) return;
            foreach (var def in _bubbleCatalog.Definitions)
            {
                if (def == null || def.Prefab == null || def.IsSpecial) continue;
                AddUnique(target, def.Type);
            }
        }

        private void AddIfSelectable(List<EBubbleType> target, EBubbleType type)
        {
            if (_bubbleCatalog == null) return;
            if (!_bubbleCatalog.TryGet(type, out var def) || def == null || def.Prefab == null) return;
            if (def.IsSpecial) return;
            AddUnique(target, type);
        }

        private static void AddUnique(List<EBubbleType> target, EBubbleType type)
        {
            if (!target.Contains(type)) target.Add(type);
        }
    }
}