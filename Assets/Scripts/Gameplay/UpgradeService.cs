using UnityEngine;

public static class UpgradeService
{
    private const string InventoryKey = "Inventory.Capacity";
    private const string HaggleChanceKey = "Upgrade.HaggleChance";
    private const string HaggleMarginKey = "Upgrade.HaggleMargin";
    private const string PackLuckKey = "Upgrade.PackLuck";
    private const string RiskPayoutKey = "Upgrade.RiskPayout";
    private const string ToolDiscountKey = "Upgrade.ToolDiscount";
    private const string SkillWindowKey = "Upgrade.SkillWindow";
    private const string ExperienceKey = "Upgrade.Experience";

    public static int InventoryCapacity => Mathf.Clamp(PlayerPrefs.GetInt(InventoryKey, 4), 4, 10);
    public static int HaggleChanceLevel => Mathf.Clamp(PlayerPrefs.GetInt(HaggleChanceKey, 0), 0, 4);
    public static int HaggleMarginLevel => Mathf.Clamp(PlayerPrefs.GetInt(HaggleMarginKey, 0), 0, 3);
    public static int PackLuckLevel => Mathf.Clamp(PlayerPrefs.GetInt(PackLuckKey, 0), 0, 3);
    public static int RiskPayoutLevel => Mathf.Clamp(PlayerPrefs.GetInt(RiskPayoutKey, 0), 0, 3);
    public static int ToolDiscountLevel => Mathf.Clamp(PlayerPrefs.GetInt(ToolDiscountKey, 0), 0, 3);
    public static int SkillWindowLevel => Mathf.Clamp(PlayerPrefs.GetInt(SkillWindowKey, 0), 0, 3);
    public static int ExperienceLevel => Mathf.Clamp(PlayerPrefs.GetInt(ExperienceKey, 0), 0, 3);

    public static float HaggleChance => .56f + HaggleChanceLevel * .06f;
    public static float HaggleMinIncrease => .05f + HaggleMarginLevel * .02f;
    public static float HaggleMaxIncrease => .13f + HaggleMarginLevel * .025f;
    public static float RiskPayoutMultiplier => 2.4f + RiskPayoutLevel * .3f;
    public static float ToolDiscount => ToolDiscountLevel * .08f;
    public static float SkillZoneWidth => .12f + SkillWindowLevel * .035f;
    public static float ExperienceMultiplier => 1f + ExperienceLevel * .2f;

    public static int GetLevel(string type) => type switch
    {
        "inventory" => InventoryCapacity - 4,
        "chance" => HaggleChanceLevel,
        "margin" => HaggleMarginLevel,
        "luck" => PackLuckLevel,
        "risk" => RiskPayoutLevel,
        "discount" => ToolDiscountLevel,
        "skill" => SkillWindowLevel,
        "experience" => ExperienceLevel,
        _ => 0
    };

    public static int GetMaxLevel(string type) => type == "inventory" ? 6 : type == "chance" ? 4 : 3;

    public static int GetCost(string type)
    {
        int level = GetLevel(type);
        int[] costs = type switch
        {
            "inventory" => new[] { 3, 25, 250, 5_000, 250_000, 10_000_000 },
            "chance" => new[] { 5, 100, 5_000, 500_000 },
            "margin" => new[] { 10, 500, 50_000 },
            "luck" => new[] { 15, 1_000, 100_000 },
            "risk" => new[] { 20, 2_000, 250_000 },
            "discount" => new[] { 12, 1_200, 120_000 },
            "skill" => new[] { 8, 800, 80_000 },
            _ => new[] { 10, 1_000, 100_000 }
        };
        return level >= costs.Length ? 0 : costs[level];
    }

    public static bool Purchase(string type)
    {
        int level = GetLevel(type);
        if (level >= GetMaxLevel(type)) return false;
        int cost = GetCost(type);
        if (GameSaveService.Labor < cost) return false;

        GameSaveService.SaveProgress(GameSaveService.Day, GameSaveService.Labor - cost, GameSaveService.Debt);
        if (type == "inventory") PlayerPrefs.SetInt(InventoryKey, InventoryCapacity + 1);
        else if (type == "chance") PlayerPrefs.SetInt(HaggleChanceKey, level + 1);
        else if (type == "margin") PlayerPrefs.SetInt(HaggleMarginKey, level + 1);
        else if (type == "luck") PlayerPrefs.SetInt(PackLuckKey, level + 1);
        else if (type == "risk") PlayerPrefs.SetInt(RiskPayoutKey, level + 1);
        else if (type == "discount") PlayerPrefs.SetInt(ToolDiscountKey, level + 1);
        else if (type == "skill") PlayerPrefs.SetInt(SkillWindowKey, level + 1);
        else PlayerPrefs.SetInt(ExperienceKey, level + 1);
        PlayerPrefs.Save();
        return true;
    }
}
