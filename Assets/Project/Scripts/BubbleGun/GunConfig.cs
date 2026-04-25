using UnityEngine;
using UnityEngine.InputSystem;

namespace BubbleGun
{
    [CreateAssetMenu(menuName = "Configs/Gun/Gun config", fileName = "GunConfig")]
    public class GunConfig : ScriptableObject
    {
        public float MinAngle;
        public float MaxAngle;
        public float ShotSpeed;

        [Header("Swap")] 
        public bool AllowSwap;
        public bool AllowRightClickSwap;
        public Key SwapKey = Key.Q;
    }
}