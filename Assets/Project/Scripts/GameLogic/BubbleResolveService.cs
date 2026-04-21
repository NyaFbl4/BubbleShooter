using System.Collections.Generic;
using BubbleField;
using Bubbles;

namespace GameLogic
{
    public sealed class BubbleResolveService : IBubbleResolveService
    {
        public BubbleResolveResult Resolve(BubbleFieldGrid grid, BubbleFieldGrid.Cell origin, int minMatchCount)
        {
            var result = new BubbleResolveResult();
            if (grid == null) return result;

            if (!grid.TryGetBubble(origin, out var originBubble) || originBubble == null)
                return result;

            var same = CollectSameTypeCluster(grid, origin, originBubble.BubbleType);
            if (same.Count < minMatchCount)
                return result;

            result.Matched.AddRange(same);
            result.Floating.AddRange(CollectFloatingIslands(grid));
            return result;
        }

        private List<BubbleFieldGrid.Cell> CollectSameTypeCluster(BubbleFieldGrid grid, BubbleFieldGrid.Cell start, EBubbleType type)
        {
            var result = new List<BubbleFieldGrid.Cell>();
            var visited = new HashSet<BubbleFieldGrid.Cell>();
            var queue = new Queue<BubbleFieldGrid.Cell>();

            if (!grid.TryGetBubble(start, out var startBubble) || startBubble == null || startBubble.BubbleType != type)
                return result;

            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                if (!grid.TryGetBubble(cell, out var bubble) || bubble == null || bubble.BubbleType != type)
                    continue;

                result.Add(cell);

                foreach (var n in grid.GetNeighboursPublic(cell))
                {
                    if (visited.Contains(n)) continue;
                    if (!grid.TryGetBubble(n, out var nb) || nb == null) continue;
                    if (nb.BubbleType != type) continue;

                    visited.Add(n);
                    queue.Enqueue(n);
                }
            }

            return result;
        }

        private List<BubbleFieldGrid.Cell> CollectFloatingIslands(BubbleFieldGrid grid)
        {
            var floating = new List<BubbleFieldGrid.Cell>();
            var occupied = new List<BubbleFieldGrid.Cell>(grid.GetOccupiedCells());
            if (occupied.Count == 0) return floating;

            var connectedToTop = new HashSet<BubbleFieldGrid.Cell>();
            var queue = new Queue<BubbleFieldGrid.Cell>();

            foreach (var cell in occupied)
            {
                if (!grid.IsTopRow(cell)) continue;
                if (connectedToTop.Add(cell)) queue.Enqueue(cell);
            }

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                foreach (var n in grid.GetNeighboursPublic(cell))
                {
                    if (connectedToTop.Contains(n)) continue;
                    if (!grid.TryGetBubble(n, out var bubble) || bubble == null) continue;
                    connectedToTop.Add(n);
                    queue.Enqueue(n);
                }
            }

            foreach (var cell in occupied)
                if (!connectedToTop.Contains(cell))
                    floating.Add(cell);

            return floating;
        }
    }
}