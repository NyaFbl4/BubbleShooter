using System.Collections.Generic;
using UnityEngine;

namespace Bubbles
{
    [CreateAssetMenu(menuName = "Configs/Bubbles/Bubble Catalog", fileName = "BubbleCatalog")]
    public class BubbleCatalog : ScriptableObject
    {
        [SerializeField] private List<BubbleDefinition> _definitions = new();
        private readonly Dictionary<EBubbleType, BubbleDefinition> _map = new();
        private bool _built;

        public IReadOnlyList<BubbleDefinition> Definitions => _definitions;

        private void OnEnable()
        {
            Rebuild();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Rebuild();         
        }
#endif

        public bool TryGet(EBubbleType type, out BubbleDefinition definition)
        {
            EnsureBuilt();
            return _map.TryGetValue(type, out definition);        
        }

        public void Rebuild()
        {
            _map.Clear();
            
            foreach (BubbleDefinition def in _definitions)
            {
                if (def == null)
                    continue;
                
                if (_map.ContainsKey(def.Type))
                {
                    Debug.LogError($"Duplicate bubble type in catalog: {def.Type}", this);
                    continue;
                }
                
                _map.Add(def.Type, def);
            }

            _built = true;
        }

        private void EnsureBuilt()
        {
            if (!_built)
                Rebuild();
        }
    }
}
