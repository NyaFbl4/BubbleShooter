using System.Collections.Generic;
using System.Text;
using Bubbles;
using UnityEngine;

namespace BubbleField
{
    public class LoadLevelService
    {
        private readonly BubbleLevelData _levelData;

        public LoadLevelService(BubbleLevelData levelData)
        {
            _levelData = levelData;
        }

        public void LogLevelBubblesSummary()
        {
            if (_levelData == null)
            {
                Debug.LogWarning("LoadLevelService: LevelData is null.");
                return;
            }

            var byType = new Dictionary<EBubbleType, int>();
            var totalTiles = 0;
            var emptyTiles = 0;
            var bubblesTotal = 0;
            var randomTiles = 0;
            var invalidRandomSlots = 0;

            var grid = _levelData.Grid;
            if (grid != null)
            {
                for (var row = 0; row < grid.Count; row++)
                {
                    var rowData = grid[row];
                    if (rowData == null || rowData.Tiles == null)
                        continue;

                    for (var col = 0; col < rowData.Tiles.Count; col++)
                    {
                        totalTiles++;
                        var tile = rowData.Tiles[col];
                        if (tile == null || !tile.HasBubble)
                        {
                            emptyTiles++;
                            continue;
                        }

                        bubblesTotal++;

                        if (tile.IsRandomBubble)
                        {
                            randomTiles++;
                            if (!TryResolveRandomType(tile.RandomSlot, out var randomType))
                            {
                                invalidRandomSlots++;
                                continue;
                            }

                            AddCount(byType, randomType);
                            continue;
                        }

                        AddCount(byType, tile.Type);
                    }
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== LoadLevelService: Level bubble summary ===");
            sb.AppendLine($"LevelData: {_levelData.name}");
            sb.AppendLine($"Rows={_levelData.Rows}, Columns={_levelData.Columns}, NumBubbles(shots)={_levelData.NumBubbles}");
            sb.AppendLine($"Tiles total={totalTiles}, empty={emptyTiles}, bubbles={bubblesTotal}");
            sb.AppendLine($"Random tiles={randomTiles}, invalid random slots={invalidRandomSlots}");
            sb.AppendLine("Bubbles by type:");

            foreach (EBubbleType type in System.Enum.GetValues(typeof(EBubbleType)))
            {
                byType.TryGetValue(type, out var count);
                sb.AppendLine($"- {type}: {count}");
            }

            Debug.Log(sb.ToString());
        }

        private bool TryResolveRandomType(int randomSlot, out EBubbleType type)
        {
            type = default;
            if (_levelData.AvailableRandomTypes == null || _levelData.AvailableRandomTypes.Count == 0)
                return false;

            var idx = Mathf.Abs(randomSlot) % _levelData.AvailableRandomTypes.Count;
            type = _levelData.AvailableRandomTypes[idx];
            return true;
        }

        private static void AddCount(Dictionary<EBubbleType, int> byType, EBubbleType type)
        {
            byType.TryGetValue(type, out var prev);
            byType[type] = prev + 1;
        }
    }
}
