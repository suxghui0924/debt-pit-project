using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameDayClock : MonoBehaviour
{
    public const float DayDurationSeconds = 300f;
    public static float SecondsUntilMidnight { get; private set; } = DayDurationSeconds;
    public static int DailyLaborPayment => GameEconomy.DailyPayment;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void EnsureInstalled()
    {
        if (SceneManager.GetActiveScene().name != "Map") return;
        if (FindFirstObjectByType<GameDayClock>() != null) return;
        new GameObject("Game Day Clock").AddComponent<GameDayClock>();
    }

    private void Start()
    {
        SecondsUntilMidnight = DayDurationSeconds;
    }

    private void Update()
    {
        if (StoryIntroController.IsPlaying || DailyStoryController.IsPlaying || GameplayTutorialController.IsBlockingGameplay) return;

        SecondsUntilMidnight -= Time.deltaTime;
        if (SecondsUntilMidnight > 0f) return;
        SecondsUntilMidnight = 0f;
        DailyStoryController.BeginEndOfDay(GameSaveService.DailyPaymentPaid);
    }

    public static void CompletePaidDay()
    {
        GameSaveService.SaveProgress(GameSaveService.Day + 1, GameSaveService.Labor, GameSaveService.Debt);
        SecondsUntilMidnight = DayDurationSeconds;
    }
}
