using UnityEngine;

namespace BubbleField
{
    [RequireComponent(typeof(Collider2D))]
    public class TopBound : MonoBehaviour
    {
        private void Reset()
        {
            var c = GetComponent<Collider2D>();
            c.isTrigger = true;            
        }
    }
}