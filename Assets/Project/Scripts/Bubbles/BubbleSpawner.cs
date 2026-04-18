using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Bubbles
{
    public class BubbleSpawner : MonoBehaviour
    {
        [SerializeField] private BubbleCatalog _bubbleCatalog;
        [SerializeField] private Transform _spawnParent;

        private readonly Dictionary<EBubbleType, BubbleController> _prefabsByType = new();

        private void Awake()
        {
            RebuildLookup();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                RebuildLookup();
        }
#endif

        [Button]
        public void RebuildLookup()
        {
            _prefabsByType.Clear();

            if (_bubbleCatalog == null)
            {
                Debug.LogError("BubbleCatalog is not assigned.", this);
                return;
            }

            _bubbleCatalog.Rebuild();

            foreach (BubbleDefinition definition in _bubbleCatalog.Definitions)
            {
                if (definition == null)
                    continue;

                if (definition.Prefab == null)
                {
                    Debug.LogError($"Prefab is missing for type {definition.Type}", this);
                    continue;
                }

                if (_prefabsByType.ContainsKey(definition.Type))
                {
                    Debug.LogError($"Duplicate prefab for type {definition.Type}", this);
                    continue;
                }

                _prefabsByType.Add(definition.Type, definition.Prefab);
            }
        }

        public BubbleController Spawn(EBubbleType bubbleType, Vector3 worldPosition)
        {
            if (_prefabsByType.Count == 0)
                RebuildLookup();

            if (!_prefabsByType.TryGetValue(bubbleType, out BubbleController bubblePrefab))
            {
                Debug.LogError($"Bubble prefab is not configured for type {bubbleType} on {name}", this);
                return null;
            }

            Transform parent = _spawnParent != null ? _spawnParent : transform;
            return Instantiate(bubblePrefab, worldPosition, Quaternion.identity, parent);
        }

        public BubbleController Spawn(EBubbleType bubbleType, Transform spawnPoint)
        {
            if (spawnPoint == null)
            {
                Debug.LogError("Spawn point is null.", this);
                return null;
            }

            return Spawn(bubbleType, spawnPoint.position);
        }
    }
}
