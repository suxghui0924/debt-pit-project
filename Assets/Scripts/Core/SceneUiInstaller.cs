using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneUiInstaller
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneCallbacks()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Title")
            TitleMenuController.EnsureInstalled();
        else if (scene.name == "Loading")
            LoadingScreenController.EnsureInstalled();
        else if (scene.name == "Map")
        {
            StoryIntroController.EnsureInstalled();
            GameDayClock.EnsureInstalled();
            GameplayUiController.EnsureInstalled();
        }
    }
}
