using Cysharp.Threading.Tasks;
using Project.Scripts.Systems.UI;

namespace Project.Scripts.UI.LevelMapUI
{
    public class LevelMapUIPresenter : LayoutPresenterBase<LevelMapUIView>, ILevelMapUIPresenter
    {
        public override void Initialize()
        {
            base.Initialize();
            ActivateAsync().Forget();
        }
    }
}
