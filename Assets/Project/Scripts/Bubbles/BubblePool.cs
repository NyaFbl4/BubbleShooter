using System.Collections.Generic;
using Bubbles;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Bubble pool", fileName = "BubblePool")]
public class BubblePool : ScriptableObject
{
    public List<BubbleController> PoolBubbles;    
}
