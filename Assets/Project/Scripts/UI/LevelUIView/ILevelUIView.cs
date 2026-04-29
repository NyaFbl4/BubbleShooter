using System;
using Project.Scripts.Systems.UI;
using UnityEngine;

namespace Project.Scripts.UI.LevelUIView
{
    public interface ILevelUIView : ILayoutView
    {
        event Action ChangeBubbleBtnClicked;
        void SetSwapButtonEnabled(bool enabled);
        void SetCurrentBubblesCountText(string text);
        void SetScoreText(string text);
        void SetCurrentBubbleSprite(Sprite sprite);
        void SetNextBubbleSprite(Sprite sprite);
    }
}
