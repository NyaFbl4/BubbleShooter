using System;
using Project.Scripts.Systems.UI;

namespace Project.Scripts.UI.PauseUI
{
    public interface IPauseUIView : ILayoutView
    {
        event Action PlayClicked;
        event Action SettingsClicked;
        event Action MenuClicked;
    }
}
