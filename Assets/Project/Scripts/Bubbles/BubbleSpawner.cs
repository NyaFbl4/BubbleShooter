using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Assets.Project.Scripts.Bubbles
{
    public class BubbleSpawner : MonoBehaviour
    {
        [SerializeField] private BubblePool _bubblePool;
        [SerializeField] private Transform _spawnParent;

        private readonly Dictionary<EBubbleType, BubbleController> _prefabsByType =
            new Dictionary<EBubbleType, BubbleController>();

        private void Awake()
        {
            RebuildLookup();
        }

        [Button] // скоррее всего не нужена фукнция
        public void RebuildLookup()
        {
            _prefabsByType.Clear();

            if (_bubblePool == null)
                return;

            foreach (BubbleController bubblePrefab in _bubblePool.PoolBubbles)
            {
                if (bubblePrefab == null)
                    continue;

                EBubbleType bubbleType = bubblePrefab.BubbleType;
                if (_prefabsByType.ContainsKey(bubbleType))
                {
                    Debug.LogError($"Duplicate bubble prefab for type {bubbleType} on {name}", this);
                    continue;
                }

                _prefabsByType.Add(bubbleType, bubblePrefab);
            }
        }

        public BubbleController Spawn(EBubbleType bubbleType, Vector3 worldPosition)
        {
            if (_prefabsByType.Count == 0)
            {
                RebuildLookup();
            }

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
