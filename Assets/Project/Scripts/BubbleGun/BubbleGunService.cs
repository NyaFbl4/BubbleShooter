using BubbleField;
using Bubbles;
using GameLogic;
using UnityEngine;

namespace BubbleGun
{
    public class BubbleGunService
    {
        public bool TryCalculateAim(Vector3 pivotPos, Vector3 mouseWorld, float 
                minAngle, float maxAngle, out float zRotation)
        {
            zRotation = 0f;
            Vector2 dir = mouseWorld - pivotPos;
            if (dir.sqrMagnitude < 0.0001f)
                return false;
                
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            zRotation = angle - 90f;
            return true;
        }

        public BubbleController Shoot(BubbleSpawner spawner, Transform shootPoint, EBubbleType bubbleType,
            float shotSpeed, BubbleGameLogic gameLogic)
        {
            if (spawner == null)
                return null;

            BubbleController spawned = spawner.Spawn(bubbleType, shootPoint.position);
            spawned.transform.up = shootPoint.up;
            spawned.Shoot(shootPoint.up, shotSpeed);
            gameLogic?.RegisterFlyingBubble(spawned);
            return spawned;
        }
    }
}