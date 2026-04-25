using UnityEngine;
using VContainer;
using Sirenix.OdinInspector;

namespace Project.Scripts.GameManager
{
    public class GameManagerHelper : MonoBehaviour
    {
        private IGameManagerService _gameManagerService;
        //private IGameBootstrapControl _gameBootstrapControl;

        [Inject]
        public void Construct(
            IGameManagerService gameManagerService)
        {
            _gameManagerService = gameManagerService;
            //_gameBootstrapControl = gameBootstrapControl;
        }

        [Button]
        public void StartGame()
        {
            if (_gameManagerService == null)
            {
                Debug.LogError("GameManagerService is null. Ensure GameManager is registered in GameLifetimeScope.");
                return;
            }

            _gameManagerService.StartGame();
        }

        [Button]
        public void FinishGame()
        {
            if (_gameManagerService == null)
            {
                Debug.LogError("GameManagerService is null. Ensure GameManager is registered in GameLifetimeScope.");
                return;
            }

            _gameManagerService.FinishGame();
        }
    }
}
