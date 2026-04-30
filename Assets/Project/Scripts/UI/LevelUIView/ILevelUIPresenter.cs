using Bubbles;
using Project.Scripts.Systems.UI;
using UnityEngine;

namespace Project.Scripts.UI.LevelUIView
{
    public interface ILevelUIPresenter : ILayoutPresenter
    {
        void RefreshQueueView();
        void SetNextBubbleSprite(Sprite sprite);
        void SetScoreText(string text);
        void SetCurrentProgress(int progress);
        Sprite ResolveBubbleSprite(EBubbleType type);
    }
}
