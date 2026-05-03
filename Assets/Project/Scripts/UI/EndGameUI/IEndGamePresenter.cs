using Project.Scripts.Systems.UI;

namespace Project.Scripts.UI.EndGame
{
    public interface IEndGamePresenter : ILayoutPresenter
    {
        void ShowResult(bool isPassed, int score, int starsCount, int totalStarsCount, string completionText, int timeBonusPoints = 0);
    }
}
