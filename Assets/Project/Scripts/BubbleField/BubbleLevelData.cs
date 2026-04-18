using System.Collections.Generic;
using Bubbles;
using UnityEngine;

namespace BubbleField
{
    [CreateAssetMenu(menuName = "Configs/Bubbles/Level Data", fileName = "LevelData")]
    public class BubbleLevelData: ScriptableObject 
    {
        public List<BubbleLevelRow> Grid = new();
        public List<EBubbleType> AvailableRandomTypes = new();
    }
}