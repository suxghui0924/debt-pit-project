using UnityEngine;

public static class GameEconomy
{
    public const int FreedomGoal = 100_000_000;

    private static readonly int[] DayMultipliers =
    {
        1, 2, 5, 15, 50, 200, 800, 3_000, 10_000, 30_000,
        100_000, 300_000, 1_000_000, 3_000_000, 10_000_000
    };

    public static int Multiplier
    {
        get
        {
            int index = Mathf.Clamp(GameSaveService.Day - 1, 0, DayMultipliers.Length - 1);
            return DayMultipliers[index];
        }
    }

    public static int Scale(int baseValue)
    {
        long value = (long)Mathf.Max(1, baseValue) * Multiplier;
        return (int)Mathf.Min(value, 1_500_000_000L);
    }

    public static int DailyPayment => Scale(GameSaveService.Day <= 2 ? 1 : 2);
}
