using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Scripts.Systems.UI;
using Project.Scripts.Systems.UI.Dtos;
using VContainer;

namespace Project.Scripts.UI.LevelMapUI
{
    public class LevelMapUIPresenter : LayoutPresenterBase<LevelMapUIView>, ILevelMapUIPresenter
    {
        [Inject] private readonly IPublisher<SelectLevelDto> _selectLevelPublisher;

        public override void Initialize()
        {
            base.Initialize();
            _layoutView.LevelClicked += OnLevelClicked;
            ActivateAsync().Forget();
        }

        public override void Dispose()
        {
            _layoutView.LevelClicked -= OnLevelClicked;
            base.Dispose();
        }

        private void OnLevelClicked(int levelNumber)
        {
            _selectLevelPublisher?.Publish(new SelectLevelDto
            {
                LevelNumber = levelNumber
            });
        }
    }
}
