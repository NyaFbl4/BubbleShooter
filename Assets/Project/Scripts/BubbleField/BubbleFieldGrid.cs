using UnityEngine;
using System;
using System.Collections.Generic;
using Bubbles;
using Sirenix.OdinInspector;

namespace BubbleField
{
    public class BubbleFieldGrid : MonoBehaviour
    {
        [Serializable]
        public struct Cell : IEquatable<Cell>
        {
            public int Row;
            public int Col;
            public Cell(int row, int col) 
            {
                Row = row; 
                Col = col;
            }
            public bool Equals(Cell other) => Row == other.Row && Col == other.Col;
            public override bool Equals(object obj) => obj is Cell other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(Row, Col);
        }

        [Header("Data/Build")]
        [SerializeField] private BubbleSpawner _spawner;
        [SerializeField] private BubbleLevelData _levelData;
        [SerializeField] private bool _buildOnStart = true;
        [SerializeField] private Transform _origin;
        [SerializeField] private Transform _bubblesRoot;
        [SerializeField] private int _rows = 12;
        [SerializeField] private int _columns = 10; // как в ките: odd row = columns - 1
        [SerializeField] private float _stepX = 0.5f;
        [SerializeField] private float _stepY = 0.44f;

        private readonly Dictionary<Cell, BubbleController> _cells = new();
        private readonly Dictionary<BubbleController, Cell> _reverse = new();

        private const int MinMatchCount = 3;

        private void Start()
        {
            if(_buildOnStart)
                BuildFromLevelData();         
        }

        [Button]
        public void BuildFromLevelData()
        {
            if (_spawner == null || _levelData == null)
            {
                Debug.LogError("BubbleFieldGrid: assign _spawner and _levelData", this);
                return;
            }

            ClearField();

            _rows = Mathf.Clamp(_levelData.Rows, 1, 12);
            _columns = Mathf.Max(2, _levelData.Columns);

            for(int r = 0; r < _rows; r++)
            {
                int width = RowWidth(r);
                for(int c = 0; c < width; c++)
                {
                    if (!TryGetTile(r, c, out BubbleLevelTile tile) || !tile.HasBubble)
                        continue;
                    
                    BubbleController bubble = _spawner.Spawn(tile.Type, CellToWorld(new Cell(r,c)));
                    if (bubble == null)
                        continue;

                    Attach(bubble, new Cell(r,c));
                }
            }
        }

        [Button]
        public void ClearField()
        {
            Transform root = _bubblesRoot != null ? _bubblesRoot : transform;
            for(int i = root.childCount - 1; i >= 0; i--)
                Destroy(root.GetChild(i).gameObject);

            _cells.Clear();
            _reverse.Clear();
        }

        public void RegisterFlyingBubble(BubbleController bubble)
        {
            if (bubble == null) return;
            bubble.StoppedOnTrigger -= OnFlyingBubbleStopped;
            bubble.StoppedOnTrigger += OnFlyingBubbleStopped;
        }

        private void OnFlyingBubbleStopped(BubbleController flying, Collider2D other)
        {
            flying.StoppedOnTrigger -= OnFlyingBubbleStopped;

            Cell target;
            Cell attachedCell = default;
            bool attached = false;

            if (other.GetComponent<TopBound>() != null)
            {
                if (!TryGetNearestFreeCellOnTopRow(flying.transform.position, out target))
                    return;
                Attach(flying, target);
                attachedCell = target;
                attached = true;
            }
            else
            {
                var touched = other.GetComponent<BubbleController>();
                if (touched != null && _reverse.TryGetValue(touched, out var touchedCell))
                {
                    if (!TryPickClosestFreeCell(GetEmptyNeighbours(touchedCell), flying.transform.position, out target))
                    {
                        if (!TryGetNearestFreeCellGlobal(flying.transform.position, out target))
                            return;
                    }
                    Attach(flying, target);
                    attachedCell = target;
                    attached = true;
                }
                else if (TryGetNearestFreeCellGlobal(flying.transform.position, out target))
                {
                    Attach(flying, target);
                    attachedCell = target;
                    attached = true;
                }    
            }

            if (attached)
                ResolveBoardAfterAttach(attachedCell);
        }

        private void Attach(BubbleController bubble, Cell cell)
        {
            bubble.StopFlying();
            bubble.SetGridCoords(cell.Row, cell.Col);
            bubble.transform.position = CellToWorld(cell);
            bubble.transform.SetParent(_bubblesRoot != null ? _bubblesRoot : transform);

            _cells[cell] = bubble;
            _reverse[bubble] = cell;
        }

        private List<Cell> GetEmptyNeighbours(Cell cell)
        {
            var result = new List<Cell>(6);
            foreach(Cell n in GetNeighbours(cell))
            {
                if (!IsValidCell(n)) continue;
                if (_cells.ContainsKey(n)) continue;
                result.Add(n);
            }
            return result;
        }

        private bool TryGetTile(int row, int col, out BubbleLevelTile tile)
        {
            tile = default;
            if (_levelData == null || row < 0 || row >= _levelData.Grid.Count) return false;
            var rowData = _levelData.Grid[row];
            if (rowData == null || col < 0 || col >= rowData.Tiles.Count) return false;
            tile = rowData.Tiles[col];
            return true;
        }

        private bool TryPickClosestFreeCell(List<Cell> candidates, Vector3 worldPos, out Cell best)
        {
            best = default;
            bool found = false;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                Cell c = candidates[i];
                float sqr = (CellToWorld(c) - worldPos).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = c;
                    found = true;
                }
            }
            return found;

        }

        private bool TryGetNearestFreeCellOnTopRow(Vector3 worldPos, out Cell best)
        {
            best = default;
            var found = false;
            var bestSqr = float.MaxValue;

            int width = RowWidth(0);
            for (int c = 0; c < width; ++c)
            {
                var cell = new Cell(0, c);
                if (_cells.ContainsKey(cell)) continue;
                float sqr = (CellToWorld(cell) - worldPos).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = cell; found = true; }
            }
            return found;
        }

        private bool TryGetNearestEmptyNeighbour(Cell center, Vector3 worldPos, out Cell best)
        {
            best = default;
            var found = false;
            var bestSqr = float.MaxValue;

            foreach (var n in GetNeighbours(center))
            {
                if (!IsValidCell(n) || _cells.ContainsKey(n)) continue;
                float sqr = (CellToWorld(n) - worldPos).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = n; found = true; }
            }
            return found;
        }

        private bool TryGetNearestFreeCellGlobal(Vector3 worldPos, out Cell best)
        {
            best = default;
            var found = false;
            var bestSqr = float.MaxValue;

            for (int r = 0; r < _rows; ++r)
            {
                int width = RowWidth(r);
                for (int c = 0; c < width; ++c)
                {
                    var cell = new Cell(r, c);
                    if (_cells.ContainsKey(cell)) continue;
                    float sqr = (CellToWorld(cell) - worldPos).sqrMagnitude;
                    if (sqr < bestSqr) { bestSqr = sqr; best = cell; found = true; }
                }
            }
            return found;
        }

        private IEnumerable<Cell> GetNeighbours(Cell cell)
        {
            // как в BubbleShooterKit LevelUtils.GetNeighbours
            if (cell.Row % 2 == 0)
            {
                yield return new Cell(cell.Row - 1, cell.Col - 1);
                yield return new Cell(cell.Row - 1, cell.Col);
                yield return new Cell(cell.Row,     cell.Col - 1);
                yield return new Cell(cell.Row,     cell.Col + 1);
                yield return new Cell(cell.Row + 1, cell.Col - 1);
                yield return new Cell(cell.Row + 1, cell.Col);
            }
            else
            {
                yield return new Cell(cell.Row - 1, cell.Col);
                yield return new Cell(cell.Row - 1, cell.Col + 1);
                yield return new Cell(cell.Row,     cell.Col - 1);
                yield return new Cell(cell.Row,     cell.Col + 1);
                yield return new Cell(cell.Row + 1, cell.Col);
                yield return new Cell(cell.Row + 1, cell.Col + 1);
            }
        }

        private int RowWidth(int row) => (row % 2 == 0) ? _columns : _columns - 1;

        private bool IsValidCell(Cell cell)
        {
            if (cell.Row < 0 || cell.Row >= _rows) return false;
            int width = RowWidth(cell.Row);
            return cell.Col >= 0 && cell.Col < width;
        }

        public Vector3 CellToWorld(Cell cell)
        {
            Vector3 o = _origin != null ? _origin.position : transform.position;
            float rowOffset = (cell.Row % 2 == 0) ? 0f : _stepX * 0.5f;
            return new Vector3(
                o.x + rowOffset + cell.Col * _stepX,
                o.y - cell.Row * _stepY,
                0f
            );
        }

        private void ResolveBoardAfterAttach(Cell origin)
        {
            if (!_cells.TryGetValue(origin, out BubbleController originBubble) || originBubble == null)
                return;

            List<Cell> matchCluster = CollectSameTypeCluster(origin, originBubble.BubbleType);
            if (matchCluster.Count < MinMatchCount)
                return;

            RemoveCells(matchCluster, true);
            RemoveFloatingIslands();
        }

        private List<Cell> CollectSameTypeCluster(Cell start, EBubbleType type)
        {
            var result = new List<Cell>();
            var visited = new HashSet<Cell>();
            var queue = new Queue<Cell>();

            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Cell current = queue.Dequeue();
                result.Add(current);

                foreach (Cell n in GetNeighbours(current))
                {
                    if (!IsValidCell(n) || visited.Contains(n))
                        continue;
                    if (!_cells.TryGetValue(n, out BubbleController b) || b == null)
                        continue;
                    if (b.BubbleType != type)
                        continue;

                    visited.Add(n);
                    queue.Enqueue(n);
                }
            }

            return result;
        }

        private void RemoveFloatingIslands()
        {
            var visited = new HashSet<Cell>();
            var floating = new List<Cell>();
            var starts = new List<Cell>(_cells.Keys); // копия, чтобы безопасно удалять позже

            for (int i = 0; i < starts.Count; i++)
            {
                Cell start = starts[i];
                if (visited.Contains(start) || !_cells.ContainsKey(start))
                    continue;

                bool touchesTop;
                List<Cell> island = CollectIsland(start, visited, out touchesTop);
                if (!touchesTop)
                    floating.AddRange(island);
            }

            if (floating.Count > 0)
                RemoveCells(floating, true);
        }

        private List<Cell> CollectIsland(Cell start, HashSet<Cell> visited, out bool touchesTop)
        {
            var island = new List<Cell>();
            var queue = new Queue<Cell>();

            touchesTop = false;
            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Cell current = queue.Dequeue();
                island.Add(current);

                if (current.Row == 0)
                    touchesTop = true;

                foreach (Cell n in GetNeighbours(current))
                {
                    if (!IsValidCell(n) || visited.Contains(n))
                        continue;
                    if (!_cells.ContainsKey(n))
                        continue;

                    visited.Add(n);
                    queue.Enqueue(n);
                }
            }

            return island;
        }

        private void RemoveCells(List<Cell> cells, bool playBurst)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                if (!_cells.TryGetValue(cell, out BubbleController bubble) || bubble == null)
                    continue;

                _cells.Remove(cell);
                _reverse.Remove(bubble);
                bubble.SetGridCoords(-1, -1);

                if (playBurst)
                    bubble.Burst();
                else
                    Destroy(bubble.gameObject);
            }
        }
    }
}