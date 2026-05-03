using System;
using Project.Scripts.Systems.UI;

namespace Project.Scripts.UI.EndGame
{
    public interface IEndGameView : ILayoutView
    {
        event Action PrimaryButtonClicked;
        event Action SecondaryButtonClicked;

        void SetTitle(string message);
        void SetScoreText(string text);
        void SetScoreVisible(bool isVisible);
        void SetCompletionText(string text);
        void SetCompletionVisible(bool isVisible);
        void SetStars(int activeStarsCount, int totalStarsCount = 3);
        void SetStarsVisible(bool isVisible);
    }
}
