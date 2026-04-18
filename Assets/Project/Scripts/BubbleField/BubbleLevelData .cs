using System.Collections.Generic;
using UnityEngine;

namespace BubbleField
{
    [CreateAssetMenu(menuName = "Configs/Bubbles/Level Data", fileName = "BubbleLevelData")]
    public class BubbleLevelData: ScriptableObject 
    {
        [Min(1)] public int Rows = 12;
        [Min(2)] public int Columns = 10;
        public List<BubbleLevelRow> Grid = new();
    }
}