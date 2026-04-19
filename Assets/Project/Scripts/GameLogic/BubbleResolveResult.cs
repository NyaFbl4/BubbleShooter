using System.Collections.Generic;
using BubbleField;

namespace GameLogic
{
    public class BubbleResolveResult
    {
        public List<BubbleFieldGrid.Cell> Matched { get; } = new();
        public List<BubbleFieldGrid.Cell> Floating { get; } = new();
    }
}