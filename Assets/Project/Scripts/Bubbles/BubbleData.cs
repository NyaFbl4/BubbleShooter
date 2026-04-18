namespace Bubbles
{
    public class BubbleData
    {
        private EBubbleType _bubbleType;
        public EBubbleType BubbleType => _bubbleType;

        public BubbleData(EBubbleType bubbleType)
        {
            _bubbleType = bubbleType;
        }
    }
}
