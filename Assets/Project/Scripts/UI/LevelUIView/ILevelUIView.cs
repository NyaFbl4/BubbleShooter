using System;
using Project.Scripts.Systems.UI;
using UnityEngine;

namespace Project.Scripts.UI.LevelUIView
{
    public interface ILevelUIView : ILayoutView
    {
        event Action ChangeBubbleBtnClicked;
        event Action PauseBtnClicked;
        void SetSwapButtonEnabled(bool enabled);
        void SetCurrentBubblesCountText(string text);
        void SetScoreText(string text);
        void SetProgress(int progress);
        void SetNextBubbleSprite(Sprite sprite);
    }
}
