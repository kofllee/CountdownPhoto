using System;

namespace _PhotoCountdown.Core.Settings
{
    public sealed class GameSettingsStorage
    {
        private const string MasterVolumeKey = "PhotoCountdown.MasterVolume";
        private const string MusicVolumeKey = "PhotoCountdown.MusicVolume";
        private const string EffectsVolumeKey = "PhotoCountdown.EffectsVolume";

        public GameSettings Load()
        {
            float masterVolume = UnityEngine.PlayerPrefs.GetFloat(
                MasterVolumeKey,
                GameSettings.DefaultMasterVolume);

            float musicVolume = UnityEngine.PlayerPrefs.GetFloat(
                MusicVolumeKey,
                GameSettings.DefaultMusicVolume);

            float effectsVolume = UnityEngine.PlayerPrefs.GetFloat(
                EffectsVolumeKey,
                GameSettings.DefaultEffectsVolume);

            return new GameSettings(masterVolume, musicVolume, effectsVolume);
        }

        public void Save(GameSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            UnityEngine.PlayerPrefs.SetFloat(MasterVolumeKey, settings.MasterVolume);
            UnityEngine.PlayerPrefs.SetFloat(MusicVolumeKey, settings.MusicVolume);
            UnityEngine.PlayerPrefs.SetFloat(EffectsVolumeKey, settings.EffectsVolume);
            UnityEngine.PlayerPrefs.Save();
        }

        public void Delete()
        {
            UnityEngine.PlayerPrefs.DeleteKey(MasterVolumeKey);
            UnityEngine.PlayerPrefs.DeleteKey(MusicVolumeKey);
            UnityEngine.PlayerPrefs.DeleteKey(EffectsVolumeKey);
            UnityEngine.PlayerPrefs.Save();
        }
    }
}