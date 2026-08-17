using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameDayClock : MonoBehaviour
{
    public const float DayDurationSeconds = 300f;
    private const string SavedDayKey = "Save.Clock.Day";
    private const string RemainingTimeKey = "Save.Clock.Remaining";
    public static float SecondsUntilMidnight { get; private set; } = DayDurationSeconds;
    public static int DailyLaborPayment => GameEconomy.DailyPayment;
    private float nextSaveAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void EnsureInstalled()
    {
        if (SceneManager.GetActiveScene().name != "Map") return;
        if (FindFirstObjectByType<GameDayClock>() != null) return;
        new GameObject("Game Day Clock").AddComponent<GameDayClock>();
    }

    private void Start()
    {
        int savedDay = PlayerPrefs.GetInt(SavedDayKey, -1);
        SecondsUntilMidnight = savedDay == GameSaveService.Day
            ? Mathf.Clamp(PlayerPrefs.GetFloat(RemainingTimeKey, DayDurationSeconds), 0f, DayDurationSeconds)
            : DayDurationSeconds;
        SaveClock();
    }

    private void Update()
    {
        if (StoryIntroController.IsPlaying || DailyStoryController.IsPlaying || GameplayTutorialController.IsBlockingGameplay) return;

        SecondsUntilMidnight -= Time.deltaTime;
        if (Time.unscaledTime >= nextSaveAt)
        {
            nextSaveAt = Time.unscaledTime + 5f;
            SaveClock();
        }
        if (SecondsUntilMidnight > 0f) return;
        SecondsUntilMidnight = 0f;
        DailyStoryController.BeginEndOfDay(GameSaveService.DailyPaymentPaid);
    }

    public static void CompletePaidDay()
    {
        GameSaveService.SaveProgress(GameSaveService.Day + 1, GameSaveService.Labor, GameSaveService.Debt);
        SecondsUntilMidnight = DayDurationSeconds;
        SaveClock();
    }

    public static void ResetSavedClock()
    {
        SecondsUntilMidnight = DayDurationSeconds;
        PlayerPrefs.SetInt(SavedDayKey, 1);
        PlayerPrefs.SetFloat(RemainingTimeKey, DayDurationSeconds);
    }

    public static void SaveClock()
    {
        if (!GameSaveService.HasSave) return;
        PlayerPrefs.SetInt(SavedDayKey, GameSaveService.Day);
        PlayerPrefs.SetFloat(RemainingTimeKey, Mathf.Clamp(SecondsUntilMidnight, 0f, DayDurationSeconds));
        PlayerPrefs.Save();
    }

    private void OnDisable() => SaveClock();
    private void OnApplicationQuit() => SaveClock();
}
