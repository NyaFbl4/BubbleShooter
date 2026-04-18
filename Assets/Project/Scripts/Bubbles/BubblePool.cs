using System.Collections.Generic;
using Assets.Project.Scripts.Bubbles;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Bubble pool", fileName = "BubblePool")]
public class BubblePool : ScriptableObject
{
    public List<BubbleController> PoolBubbles;    
}
