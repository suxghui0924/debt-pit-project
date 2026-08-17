using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public sealed class GameBootstrap : Singleton<GameBootstrap>
{
    private const string TitleSceneName = "Title";

    private VideoPlayer introVideoPlayer;
    private bool isTransitioningToTitle;

    protected override void Awake()
    {
        base.Awake();
        if (!IsPrimaryInstance) return;
        EnsureManager<SoundManager>("Sound Manager");
        EnsureManager<PoolManager>("Pool Manager");

        introVideoPlayer = FindFirstObjectByType<VideoPlayer>();
        if (introVideoPlayer != null)
            introVideoPlayer.loopPointReached += LoadTitleScene;
    }

    protected override void OnDestroy()
    {
        if (introVideoPlayer != null)
            introVideoPlayer.loopPointReached -= LoadTitleScene;

        base.OnDestroy();
    }

    private void LoadTitleScene(VideoPlayer source)
    {
        if (isTransitioningToTitle) return;

        isTransitioningToTitle = true;
        ReleaseIntroVideo(source);
        SceneFade.Load(TitleSceneName);
    }

    private void ReleaseIntroVideo(VideoPlayer source)
    {
        if (source == null) return;
        source.loopPointReached -= LoadTitleScene;
        source.Stop();
        source.targetTexture = null;
        source.clip = null;
        source.enabled = false;
        introVideoPlayer = null;
    }

    private static void EnsureManager<T>(string objectName) where T : Component
    {
        if (FindFirstObjectByType<T>() == null) new GameObject(objectName).AddComponent<T>();
    }
}
