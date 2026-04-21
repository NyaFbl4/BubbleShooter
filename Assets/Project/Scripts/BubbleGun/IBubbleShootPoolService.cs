using System.Collections.Generic;
using Bubbles;

namespace BubbleGun
{
    public interface IBubbleShootPoolService
    {
        void Rebuild(List<EBubbleType> target);
    }
}