using Bubbles;

namespace Assets.Project.Scripts.System.Messaging
{
    public readonly struct BubbleBurstResolvedMessage
    {
        public readonly EBubbleType BubbleType;
        public readonly int BurstCount;

        public BubbleBurstResolvedMessage(EBubbleType bubbleType, int burstCount)
        {
            BubbleType = bubbleType;
            BurstCount = burstCount;
        }
    }
}
