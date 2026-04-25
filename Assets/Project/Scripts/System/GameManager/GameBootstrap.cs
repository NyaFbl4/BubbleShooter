using System;
using VContainer.Unity;

namespace Project.Scripts.GameManager
{
    public class GameBootstrap : ITickable, IDisposable, IGameStartListener, IGameFinishListener, IGameBootstrapControl
    {
        public GameBootstrap()
        {
            IGameListener.Register(this);
        }

        public void Tick()
        {

        }

        public void OnStartGame()
        {

        }

        public void OnFinishGame()
        {

        }

        public void Dispose()
        {
            IGameListener.Unregister(this);
        }

        public void RestartInitialSpawn()
        {
            
        }
    }
}
