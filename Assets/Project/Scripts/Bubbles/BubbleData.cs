namespace Assets.Project.Scripts.Bubbles
{
    public class BubbleData
    {
        private EBubbleType _bubbleType;
        private int _requiredConnectedCount;
        private int _maxBurstCount;

        public EBubbleType BubbleType => _bubbleType;
        public int RequiredConnectedCount => _requiredConnectedCount;
        public int MaxBurstCount => _maxBurstCount;

        public BubbleData(EBubbleType bubbleType, int requiredConnectedCount, int maxBurstCount)
        {
            _bubbleType = bubbleType;
            _requiredConnectedCount = requiredConnectedCount;
            _maxBurstCount = maxBurstCount;
        }
    }
}
