using System.Collections.Generic;
using BubbleField;

namespace GameLogic
{
    public interface IBubbleResolveService
    {
        BubbleResolveResult Resolve(BubbleFieldGrid grid, BubbleFieldGrid.Cell origin, int minMatchCount);
        List<BubbleFieldGrid.Cell> CollectFloating(BubbleFieldGrid grid);
    }
}