using UnityEngine;
using System;
using System.Collections.Generic;
using Bubbles;
using Project.Scripts.GameManager;
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
        
        [Header("View Window")]
        [SerializeField] private int _visibleRows;
        [SerializeField] private int _extraLogicalRows;
        private int _authoredRows; 
        private int _viewStartRow; 
        
        [Header("Data/Build")]
        [SerializeField] private BubbleSpawner _spawner;
        [SerializeField] private BubbleLevelData _levelData;
        [SerializeField] private bool _buildOnStart = true;
        [SerializeField] private Transform _origin;
        [SerializeField] private Transform _bubblesRoot;
        [SerializeField] private int _maxRows = 12;
        [SerializeField] private float _stepX = 0.5f;
        [SerializeField] private float _stepY = 0.44f;
        [SerializeField] private float _cellPadding = 0.01f;

        private int _rows;
        private int _columns;
        private int _evenRowWidth;
        private int _oddRowWidth;
        private readonly List<int> _rowWidths = new();

        private readonly Dictionary<Cell, BubbleController> _cells = new();
        private readonly Dictionary<BubbleController, Cell> _reverse = new();
        private readonly List<EBubbleType> _randomMap = new();

        private const int MinMatchCount = 3;
        

        [Button]
        public void BuildFromLevelData()
        {
            if (_spawner == null || _levelData == null)
            {
                Debug.LogError("BubbleFieldGrid: assign _spawner and _levelData", this);
                return;
            }

            ClearField();

            SyncDimensionsFromLevelData();
            if (_rows == 0)
                return;
            PrepareRandomMap();

            for(int r = 0; r < _rows; r++)
            {
                int width = RowWidth(r);
                for(int c = 0; c < width; c++)
                {
                    if (!TryGetTile(r, c, out BubbleLevelTile tile) || !tile.HasBubble)
                        continue;
                    
                    EBubbleType typeToSpawn = ResolveTileType(tile);
                    BubbleController bubble = _spawner.Spawn(typeToSpawn, CellToWorld(new Cell(r,c)));
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

        [Button]
        private void RandomizeAllTilesAsRandomSlots()
        {
            if (_levelData == null || _levelData.AvailableRandomTypes == null || _levelData.AvailableRandomTypes.Count == 0)
                return;

            int maxSlot = _levelData.AvailableRandomTypes.Count;
            for (int r = 0; r < _levelData.Grid.Count; r++)
            {
                var row = _levelData.Grid[r];
                if (row == null) continue;
                for (int c = 0; c < row.Tiles.Count; c++)
                {
                    var t = row.Tiles[c];
                    t.HasBubble = true;
                    t.IsRandomBubble = true;
                    t.RandomSlot = UnityEngine.Random.Range(0, maxSlot);
                    row.Tiles[c] = t;
                }
            }
        }

        public int GetGroundRow()
        {
            int ground = -1;
            foreach (var kv in _cells)
            {
                if (kv.Key.Row > ground)
                    ground = kv.Key.Row;
            }
            return ground;
        }

        public void ShiftOriginRows(int rows)
        {
            if (rows == 0) 
                return;
            var t = _origin != null ? _origin : transform;
            t.position += Vector3.up * (rows * _stepY);
            RepositionAllBubblesToGrid();
        }

        private void RepositionAllBubblesToGrid()
        {
            foreach (var kv in _cells)
            {
                var cell = kv.Key;
                var bubble = kv.Value;
                if (bubble == null) continue;
                bubble.transform.position = CellToWorld(cell);
            }
            
            RefreshVisibleWindow();
        }
        
        public bool TryGetBubble(Cell cell, out BubbleController bubble) => _cells.TryGetValue(cell, out bubble);
        public IEnumerable<Cell> GetOccupiedCells() => _cells.Keys;
        public IEnumerable<Cell> GetNeighboursPublic(Cell cell) => GetNeighbours(cell);
        public bool IsTopRow(Cell cell) => cell.Row == 0;

        private int GetLowestOccupiedRow()
        { 
            int maxRow = -1;
            foreach (var kv in _cells)
                if (kv.Key.Row > maxRow) 
                    maxRow = kv.Key.Row;
            return maxRow;
        }
        
        private void SyncDimensionsFromLevelData()
        {
            _rowWidths.Clear();
            _rows = 0;
            _columns = 0;
            _evenRowWidth = 0;
            _oddRowWidth = 0;

            if (_levelData == null)
                return;

            int configuredRows = _levelData.Rows > 0
                ? _levelData.Rows 
                : (_levelData.Grid?.Count ?? 0);
            //_rows = Mathf.Min(configuredRows, _maxRows);
            int visible = Mathf.Max(1, _visibleRows);
            int extra = Mathf.Max(0, _extraLogicalRows);
            _authoredRows = Mathf.Clamp(configuredRows, 0, Mathf.Max(0, _maxRows - extra));
            _rows = _authoredRows + extra;
            
            if (_rows <= 0) return;
            _viewStartRow = Mathf.Max(0, _authoredRows - visible);
            
            int configuredColumns = _levelData.Columns  > 0
                ? _levelData.Columns  
                : ResolveColumnsFromAuthoredGrid();
            _columns = Mathf.Max(1, configuredColumns);
            
            _evenRowWidth = _columns;
            _oddRowWidth = Mathf.Max(1, _columns - 1);
            
            for (int i = 0; i < _authoredRows; i++)
                _rowWidths.Add((i % 2 == 0) ? _evenRowWidth : _oddRowWidth);
        }
        
        private int ResolveColumnsFromAuthoredGrid()
        {
            int maxWidth = 0;
            if (_levelData?.Grid == null)
                return 1;
            for (int i = 0; i < _levelData.Grid.Count; i++)
            {
                int w = _levelData.Grid[i]?.Tiles?.Count ?? 0;
                if (w > maxWidth)
                     maxWidth = w;
            }
            return Mathf.Max(1, maxWidth); 
        }

        private int PredictRowWidth(int row)
        {
            if (row < 0) return 0;
            return (row % 2 == 0) ? _evenRowWidth : _oddRowWidth;
        }

        private bool IsValidOrGrowableCell(Cell cell)
        {
            if (cell.Row < 0) return false;
            if (cell.Row < _rows) return IsValidCell(cell);
                 // Разрешаем "как в рефе": первый новый ряд снизу
                 if (cell.Row == _rows && _rows < _maxRows) 
                 {
                    int width = PredictRowWidth(cell.Row);
                    return cell.Col >= 0 && cell.Col < width;
                 } 
            return false;
        }

        private void EnsureRowExists(int row)
        {
            while (_rows <= row && _rows < _maxRows)
            {
                int width = PredictRowWidth(_rows);
                _rowWidths.Add(width);
                if (width > _columns)
                    _columns = width;
                _rows++;
            }
        }

        private void PrepareRandomMap()
        {
            _randomMap.Clear();

            if (_levelData.AvailableRandomTypes == null || _levelData.AvailableRandomTypes.Count == 0)
            {
                // fallback: все enum-типы
                foreach (EBubbleType t in Enum.GetValues(typeof(EBubbleType)))
                    _randomMap.Add(t);
            }
            else
            {
                _randomMap.AddRange(_levelData.AvailableRandomTypes);
            }

            for (int i = 0; i < _randomMap.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, _randomMap.Count);
                (_randomMap[i], _randomMap[j]) = (_randomMap[j], _randomMap[i]);
            }
        }

        private EBubbleType ResolveTileType(BubbleLevelTile tile)
        {
            if (!tile.IsRandomBubble)
                return tile.Type;

            if (_randomMap.Count == 0)
                return tile.Type;

            int idx = Mathf.Abs(tile.RandomSlot) % _randomMap.Count;
            return _randomMap[idx];
        }

        private void Attach(BubbleController bubble, Cell cell)
        {
            bubble.StopFlying();
            bubble.SetGridCoords(cell.Row, cell.Col);
            bubble.transform.position = CellToWorld(cell);
            bubble.transform.SetParent(_bubblesRoot != null ? _bubblesRoot : transform);

            _cells[cell] = bubble;
            _reverse[bubble] = cell;
            RefreshVisibleWindow();
        }

        private List<Cell> GetEmptyNeighbours(Cell cell)
        {
            var result = new List<Cell>(6);
            foreach(Cell n in GetNeighbours(cell))
            {
                if (!IsValidOrGrowableCell(n)) continue;
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
                if (!IsValidCell(cell)) continue;
                if (_cells.ContainsKey(cell)) continue;
                float sqr = (CellToWorld(cell) - worldPos).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = cell; found = true; }
            }
            return found;
        }

        private IEnumerable<Cell> GetNeighbours(Cell cell)
        {
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

        private int RowWidth(int row)
        {
            if (row < 0 || row >= _rows) return 0;
            return (row % 2 == 0) ? _evenRowWidth : _oddRowWidth;
            
        }

        private bool IsValidCell(Cell cell)
        {
            if (cell.Row < 0 || cell.Row >= _rows) 
                return false;
            int width = RowWidth(cell.Row);
            if (cell.Col < 0 || cell.Col >= width) 
                return false;
            return true;
        }

        public Vector3 CellToWorld(Cell cell)
        {
            Vector3 o = _origin != null ? _origin.position : transform.position;
            float rowOffset = (cell.Row % 2 == 0) ? 0f : _stepX * 0.5f;
            int visualRow = cell.Row - _viewStartRow;
            return new Vector3(
                o.x + rowOffset + cell.Col * _stepX,
                o.y - visualRow  * _stepY,
                0f
            );
        }

        private void RefreshVisibleWindow()
        {
            int visible = Mathf.Max(1, _visibleRows);
            foreach (var kv in _cells)
            {
                var cell = kv.Key;
                var bubble = kv.Value;
                if (bubble == null) continue;
                
                int vr = cell.Row - _viewStartRow;
                bool isVisible = vr >= 0 && vr < visible;
                //bubble.SetVisualVisible(isVisible);
            }
        }

        public bool TryAttachFlyingBubble(BubbleController flying, Collider2D other, out Cell attachedCell)
        {
            attachedCell = default;
            if (flying == null || other == null)
                return false;

            Cell target;

            if (other.GetComponent<TopBound>() != null)
            {
                if (!TryGetNearestFreeCellOnTopRow(flying.transform.position, out target))
                    return false;

                Attach(flying, target);
                attachedCell = target;
                return true;
            }

            var touched = other.GetComponent<BubbleController>();
            if (touched == null || !_reverse.TryGetValue(touched, out var touchedCell))
                return false;

            var candidates = GetEmptyNeighbours(touchedCell);
            if (!TryPickClosestFreeCell(candidates, flying.transform.position, out target))
                return false;

            EnsureRowExists(target.Row);
            Attach(flying, target);
            attachedCell = target;
            return true;
        }

        public void RemoveCells(List<Cell> cells, bool playBurst)
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