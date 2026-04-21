using System;
using Bubbles;

namespace BubbleField
{
    [Serializable]
    public class BubbleLevelTile
    {
        public bool HasBubble;
        public bool IsRandomBubble;
        public int RandomSlot;
        public EBubbleType Type;
    }
}