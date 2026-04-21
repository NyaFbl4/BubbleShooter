using UnityEngine;

namespace Bubbles
{
    [CreateAssetMenu(menuName = "Configs/Bubbles/Bubble Definition", fileName = "BubbleDefinition")]
    public class BubbleDefinition : ScriptableObject
    {
        [SerializeField] private EBubbleType _type;
        [SerializeField] private BubbleController _prefab;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private bool _isSpecial;
        [SerializeField, Min(0)] private int _score = 10;

        public EBubbleType Type => _type;
        public BubbleController Prefab => _prefab;
        public BubbleController BubbleController => _prefab;
        public Sprite Sprite => _sprite;
        public bool IsSpecial => _isSpecial;
        public int Score => _score;
    }
}
