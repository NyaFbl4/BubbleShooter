using System.Collections.Generic;
using Bubbles;
using UnityEngine;

namespace BubbleField
{
    public class BubbleGameLogic : MonoBehaviour
    {
        [SerializeField] private BubbleFieldGrid _grid;
        [SerializeField] private int _minMatchCount = 3;

        public void RegisterFlyingBubble(BubbleController bubble)
        {
            if (bubble == null) return;
            bubble.StoppedOnTrigger -= OnFlyingBubbleStopped;
            bubble.StoppedOnTrigger += OnFlyingBubbleStopped;
        }

        private void OnFlyingBubbleStopped(BubbleController flying, Collider2D other)
        {
            if (flying == null || other == null || _grid == null) return;
            flying.StoppedOnTrigger -= OnFlyingBubbleStopped;

            if (!_grid.TryAttachFlyingBubble(flying, other, out var attachedCell))
                return;

            ResolveAfterAttach(attachedCell);
        }

        private void ResolveAfterAttach(BubbleFieldGrid.Cell origin)
        {
            if (!_grid.TryGetBubble(origin, out var originBubble) || originBubble == null)
                return;

            var same = CollectSameTypeCluster(origin, originBubble.BubbleType);
            if (same.Count < _minMatchCount)
                return;

            _grid.RemoveCells(same, playBurst: true);

            var floating = CollectFloatingIslands();
            if (floating.Count > 0)
                _grid.RemoveCells(floating, playBurst: true);
        }

        private List<BubbleFieldGrid.Cell> CollectSameTypeCluster(BubbleFieldGrid.Cell start, EBubbleType type)
        {
            var result = new List<BubbleFieldGrid.Cell>();
            if (_grid == null)
                return result;

            if (!_grid.TryGetBubble(start, out var startBubble) || startBubble == null)
                return result;

            if (startBubble.BubbleType != type)
                return result;

            var visited = new HashSet<BubbleFieldGrid.Cell>();
            var queue = new Queue<BubbleFieldGrid.Cell>();

            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();

                if (!_grid.TryGetBubble(cell, out var bubble) || bubble == null)
                    continue;
                if (bubble.BubbleType != type)
                    continue;

                result.Add(cell);

                foreach (var n in _grid.GetNeighboursPublic(cell))
                {
                    if (visited.Contains(n))
                        continue;
                    if (!_grid.TryGetBubble(n, out var nb) || nb == null)
                        continue;
                    if (nb.BubbleType != type)
                        continue;

                    visited.Add(n);
                    queue.Enqueue(n);
                }
            }

            return result;
        }

        private List<BubbleFieldGrid.Cell> CollectFloatingIslands()
        {
            var floating = new List<BubbleFieldGrid.Cell>();
            if (_grid == null)
                return floating;

            var occupied = new List<BubbleFieldGrid.Cell>(_grid.GetOccupiedCells());
            if (occupied.Count == 0)
                return floating;

            // Все клетки, связанные с верхом
            var connectedToTop = new HashSet<BubbleFieldGrid.Cell>();
            var queue = new Queue<BubbleFieldGrid.Cell>();

            foreach (var cell in occupied)
            {
                if (!_grid.IsTopRow(cell))
                    continue;

                if (connectedToTop.Add(cell))
                    queue.Enqueue(cell);
            }

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();

                foreach (var n in _grid.GetNeighboursPublic(cell))
                {
                    if (connectedToTop.Contains(n))
                        continue;
                    if (!_grid.TryGetBubble(n, out var bubble) || bubble == null)
                        continue;

                    connectedToTop.Add(n);
                    queue.Enqueue(n);
                }
            }

            // Всё, что занято, но не связано с верхом — висячее
            foreach (var cell in occupied)
            {
                if (!connectedToTop.Contains(cell))
                    floating.Add(cell);
            }

            return floating;
        }

    }
}