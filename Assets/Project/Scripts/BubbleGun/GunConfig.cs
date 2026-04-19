using UnityEngine;

namespace BubbleGun
{
    [CreateAssetMenu(menuName = "Configs/Gun/Gun config", fileName = "GunConfig")]
    public class GunConfig : ScriptableObject
    {
        public float MinAngle;
        public float MaxAngle;
        public float ShotSpeed;
    }
}