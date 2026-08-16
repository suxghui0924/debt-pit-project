using UnityEngine;

public static class GameSettings
{
    public const float MaxVolume = 2f;
    public static float MasterVolume { get => Mathf.Clamp(PlayerPrefs.GetFloat("Settings.MasterVolume", 1f), 0f, MaxVolume); set { float volume = Mathf.Clamp(value, 0f, MaxVolume); PlayerPrefs.SetFloat("Settings.MasterVolume", volume); AudioListener.volume = 1f; SoundManager.Instance?.SetMasterVolume(volume); } }
    public static float BgmVolume { get => Mathf.Clamp(PlayerPrefs.GetFloat("Settings.BgmVolume", 1f), 0f, MaxVolume); set { float volume = Mathf.Clamp(value, 0f, MaxVolume); PlayerPrefs.SetFloat("Settings.BgmVolume", volume); SoundManager.Instance?.SetMusicVolume(volume); } }
    public static float SfxVolume { get => Mathf.Clamp(PlayerPrefs.GetFloat("Settings.SfxVolume", 1f), 0f, MaxVolume); set { float volume = Mathf.Clamp(value, 0f, MaxVolume); PlayerPrefs.SetFloat("Settings.SfxVolume", volume); SoundManager.Instance?.SetSfxVolume(volume); } }
    public static float MouseSensitivity { get => PlayerPrefs.GetFloat("Settings.MouseSensitivity", 2.2f); set => PlayerPrefs.SetFloat("Settings.MouseSensitivity", Mathf.Clamp(value, 0.2f, 10f)); }
    public static void Apply()
    {
        AudioListener.volume = 1f;
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.SetMasterVolume(MasterVolume);
        SoundManager.Instance.SetMusicVolume(BgmVolume);
        SoundManager.Instance.SetSfxVolume(SfxVolume);
    }
    public static void Save() => PlayerPrefs.Save();
}
