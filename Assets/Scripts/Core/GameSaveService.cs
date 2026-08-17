using UnityEngine;

public static class GameSaveService
{
    private const string SaveExistsKey = "Save.Exists";
    private const string DayKey = "Save.Day";
    private const string LaborKey = "Save.Labor";
    private const string DebtKey = "Save.Debt";
    private const string FreedomFundKey = "Save.FreedomFund";
    private const string DailyPaymentDayKey = "Save.DailyPaymentDay";
    private const string DailyRewardDayKey = "Save.DailyRewardDay";
    private const string IntroSeenKey = "Save.IntroSeen";

    public static bool HasSave => PlayerPrefs.GetInt(SaveExistsKey, 0) == 1;
    public static int Day => PlayerPrefs.GetInt(DayKey, 1);
    public static int Labor => PlayerPrefs.GetInt(LaborKey, 0);
    public static int Debt => PlayerPrefs.GetInt(DebtKey, 0);
    public static int FreedomFund => PlayerPrefs.GetInt(FreedomFundKey, 0);
    public static bool DailyPaymentPaid => PlayerPrefs.GetInt(DailyPaymentDayKey, -1) == Day;
    public static bool DailyRewardClaimed => PlayerPrefs.GetInt(DailyRewardDayKey, -1) == Day;
    public static bool IntroSeen => PlayerPrefs.GetInt(IntroSeenKey, 0) == 1;

    public static void StartNewGame()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt(SaveExistsKey, 1);
        PlayerPrefs.SetInt(DayKey, 1);
        PlayerPrefs.SetInt(LaborKey, 0);
        PlayerPrefs.SetInt(DebtKey, 0);
        PlayerPrefs.SetInt(FreedomFundKey, 0);
        PlayerPrefs.SetInt(DailyPaymentDayKey, -1);
        PlayerPrefs.SetInt(DailyRewardDayKey, -1);
        PlayerPrefs.SetInt("Inventory.Capacity", 4);
        PlayerPrefs.SetInt("Upgrade.HaggleChance", 0);
        PlayerPrefs.SetInt("Upgrade.HaggleMargin", 0);
        PlayerPrefs.SetInt("Upgrade.PackLuck", 0);
        PlayerPrefs.SetInt("Upgrade.RiskPayout", 0);
        PlayerPrefs.SetInt("Upgrade.ToolDiscount", 0);
        PlayerPrefs.SetInt("Upgrade.SkillWindow", 0);
        PlayerPrefs.SetInt("Upgrade.Experience", 0);
        GameplayTutorialController.ResetProgress();
        CardProgressionService.Reset();
        for (int index = 0; index < ItemInventoryService.MaxSlots; index++) PlayerPrefs.DeleteKey("Inventory.Slot." + index);
        ItemInventoryService.AddStarterDeliveries();
        PlayerPrefs.SetInt(IntroSeenKey, 0);
        GameDayClock.ResetSavedClock();
        GameplayUiController.ResetSavedNightMarket();
        ComputerRadioPlayer.ResetSavedPlayback();
        PlayerPrefs.Save();
    }

    public static void SaveProgress(int day, int labor, int debt)
    {
        PlayerPrefs.SetInt(SaveExistsKey, 1);
        PlayerPrefs.SetInt(DayKey, Mathf.Max(1, day));
        PlayerPrefs.SetInt(LaborKey, Mathf.Max(0, labor));
        PlayerPrefs.SetInt(DebtKey, Mathf.Max(0, debt));
        GameDayClock.SaveClock();
        PlayerPrefs.Save();
    }

    public static void MarkIntroSeen()
    {
        PlayerPrefs.SetInt(IntroSeenKey, 1);
        PlayerPrefs.Save();
    }

    public static void InvalidateSave()
    {
        PlayerPrefs.SetInt(SaveExistsKey, 0);
        PlayerPrefs.Save();
    }

    public static void SetFreedomFund(int amount)
    {
        PlayerPrefs.SetInt(FreedomFundKey, Mathf.Max(0, amount));
        PlayerPrefs.Save();
    }

    public static void MarkDailyPaymentPaid()
    {
        PlayerPrefs.SetInt(DailyPaymentDayKey, Day);
        PlayerPrefs.Save();
    }

    public static void MarkDailyRewardClaimed()
    {
        PlayerPrefs.SetInt(DailyRewardDayKey, Day);
        PlayerPrefs.Save();
    }
}
