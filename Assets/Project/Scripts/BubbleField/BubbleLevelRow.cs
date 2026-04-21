using System;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleField
{
    [CreateAssetMenu(menuName = "Configs/Bubbles/Level Row Data", fileName = "RowData")]
    public class BubbleLevelRow : ScriptableObject
    {
        public List<BubbleLevelTile> Tiles = new();
    }
}