using Assets.Project.Scripts.System.Messaging;
using MessagePipe;
using VContainer;
using VContainer.Unity;

namespace Assets.Project.Scripts.System.Installers
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterMessagePipe();
        }
    }
}
