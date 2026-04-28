using System;
using Project.Scripts.Systems.UI;
using UnityEngine.UIElements;

namespace Project.Scripts.UI.LevelUIView
{
    public interface ILevelUIView : ILayoutView
    {
        event Action ChangeBubbleBtnClicked;
        /*Button ChangeBubbleBtn { get; }
        VisualElement NextBubbleImg { get; }
        Label ProgressLbl { get; }
        VisualElement ProgressFill { get; }*/
    }
}