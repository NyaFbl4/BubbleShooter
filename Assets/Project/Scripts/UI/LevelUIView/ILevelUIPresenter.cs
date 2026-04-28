using Bubbles;
using Project.Scripts.Systems.UI;
using UnityEngine;

namespace Project.Scripts.UI.LevelUIView
{
    public interface ILevelUIPresenter : ILayoutPresenter
    {
        void RefreshQueueView();
        void SetCurrentBubbleSprite(Sprite sprite);
        void SetNextBubbleSprite(Sprite sprite);
        Sprite ResolveBubbleSprite(EBubbleType type);
    }
}
