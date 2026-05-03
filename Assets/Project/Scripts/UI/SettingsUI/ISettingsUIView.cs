using System;
using Project.Scripts.Systems.UI;

namespace Project.Scripts.UI.SettingsUI
{
    public interface ISettingsUIView : ILayoutView
    {
        event Action CloseClicked;
        event Action MusicToggleClicked;
        event Action SoundToggleClicked;
        event Action<float> MusicVolumeChanged;
        event Action<float> SoundVolumeChanged;

        void SetMusicEnabled(bool isEnabled);
        void SetSoundEnabled(bool isEnabled);
        void SetMusicVolume(float value);
        void SetSoundVolume(float value);
    }
}