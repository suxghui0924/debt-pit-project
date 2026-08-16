using UnityEngine;

public static class CardProgressionService
{
    private const string ExperienceKey = "Progress.CardExperience";
    public const int MaxLevel = 30;
    public const int CardsPerPack = 30;

    private static readonly string[] CardNames =
    {
        "구겨진 시간표", "녹슨 출입증", "빈 배급표", "파손된 코일", "작업반 명찰",
        "감시 기록 조각", "봉인된 영수증", "정비용 회로", "야간 통행증", "폐기 승인서",
        "검열된 편지", "비상 전력 셀", "암호화된 장부", "관리자 서명", "검은 거래표",
        "기억 복구 조각", "격리구역 열쇠", "시설 설계도", "위조된 판결문", "탈출 경로 지도",
        "감독관 인장", "삭제된 신원표", "중앙 서버 키", "자유 채권", "사형 집행 유예장",
        "기억 원본 백업", "정부 비밀 계정", "시설 소유 증서", "무기한 면책 계약", "황금 자유 계약"
    };

    private static readonly string[] PackNames =
    {
        "폐기장 잔해 팩", "녹슨 철문 팩", "야간 배급 팩", "작업반 교대 팩", "정비구역 부품 팩",
        "감시망 기록 팩", "밀봉 영수증 팩", "비상 전력 팩", "검열 우편 팩", "폐기 승인 팩",
        "지하 거래 팩", "격리구역 보급 팩", "암호 장부 팩", "관리국 인가 팩", "검은 시장 팩",
        "기억 파편 팩", "보안구역 열쇠 팩", "시설 설계도 팩", "위조 판결 팩", "탈출 경로 팩",
        "감독관 금고 팩", "삭제 신원 팩", "중앙 서버 팩", "자유 채권 팩", "집행 유예 팩",
        "기억 원본 팩", "정부 비밀계정 팩", "시설 소유권 팩", "무기한 면책 팩", "황금 자유계약 팩"
    };

    public static int Experience => PlayerPrefs.GetInt(ExperienceKey, 0);
    public static int Level
    {
        get
        {
            int level = 1;
            while (level < MaxLevel && Experience >= TotalExperienceForLevel(level + 1)) level++;
            return level;
        }
    }

    public static int UnlockedCardCount => Mathf.Clamp(Level + 2, 3, CardsPerPack);
    public static int CurrentLevelExperience => Experience - TotalExperienceForLevel(Level);
    public static int ExperienceForNextLevel => Level >= MaxLevel ? 0 : TotalExperienceForLevel(Level + 1) - TotalExperienceForLevel(Level);
    public static int PackCount => PackNames.Length;

    public static CardPackDefinition GetPack(int index)
    {
        index = Mathf.Clamp(index, 0, PackNames.Length - 1);
        int level = index + 1;
        int basePrice = Mathf.Max(1, Mathf.RoundToInt(Mathf.Pow(1.55f, index)));
        int valueMultiplier = Mathf.Max(1, Mathf.RoundToInt(Mathf.Pow(1.38f, index)));
        float luck = Mathf.Clamp01(.04f + index * .03f);
        return new CardPackDefinition(PackNames[index], level, basePrice, luck, valueMultiplier, 10 + index * 5);
    }

    public static bool TryGetPack(string packName, out CardPackDefinition definition)
    {
        int index = System.Array.IndexOf(PackNames, packName);
        if (index >= 0)
        {
            definition = GetPack(index);
            return true;
        }
        definition = default;
        return false;
    }

    public static int GetPackPrice(string packName)
    {
        return TryGetPack(packName, out CardPackDefinition pack) ? GameEconomy.Scale(pack.BasePrice) : 0;
    }

    public static bool IsCardPack(string item)
    {
        return TryGetPack(item, out _) || item == "무료 카드팩" || item == "낡은 카드팩" || item == "보급 카드팩" || item == "고급 카드팩";
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(ExperienceKey);
        PlayerPrefs.Save();
    }

    public static bool AddExperience(int baseAmount)
    {
        int previousLevel = Level;
        int adjusted = Mathf.Max(1, Mathf.RoundToInt(baseAmount * UpgradeService.ExperienceMultiplier));
        PlayerPrefs.SetInt(ExperienceKey, Mathf.Max(0, Experience + adjusted));
        PlayerPrefs.Save();
        return Level > previousLevel;
    }

    public static int PackExperience(string pack)
    {
        if (TryGetPack(pack, out CardPackDefinition definition)) return definition.Experience;
        return pack == "고급 카드팩" ? 35 : pack == "보급 카드팩" ? 20 : 10;
    }

    public static string RollCard(string pack, float minigameLuckBonus = 0f)
    {
        if (TryGetPack(pack, out CardPackDefinition definition))
        {
            float packExponent = Mathf.Lerp(1.8f, .25f, definition.LuckBonus);
            packExponent = Mathf.Max(.14f, packExponent - UpgradeService.PackLuckLevel * .08f - Mathf.Clamp01(minigameLuckBonus));
            int cardIndex = Mathf.Clamp(Mathf.FloorToInt(Mathf.Pow(Random.value, packExponent) * UnlockedCardCount), 0, UnlockedCardCount - 1);
            return $"{definition.RequiredLevel:00}급·{CardNames[cardIndex]}";
        }

        int quality = PackQuality(pack);
        float exponent = quality == 2 ? .42f : quality == 1 ? .85f : 1.75f;
        exponent = Mathf.Max(.16f, exponent - UpgradeService.PackLuckLevel * .08f - Mathf.Clamp01(minigameLuckBonus));
        int index = Mathf.Clamp(Mathf.FloorToInt(Mathf.Pow(Random.value, exponent) * UnlockedCardCount), 0, UnlockedCardCount - 1);
        return PackPrefix(quality) + CardNames[index];
    }

    public static bool TryGetCardValue(string item, out int value)
    {
        if (string.IsNullOrEmpty(item))
        {
            value = 0;
            return false;
        }
        int separator = item.IndexOf('·');
        if (separator <= 0)
        {
            value = 0;
            return false;
        }

        string prefix = item[..separator];
        int qualityMultiplier;
        if (prefix.EndsWith("급") && int.TryParse(prefix[..^1], out int packLevel) && packLevel >= 1 && packLevel <= PackCount)
            qualityMultiplier = GetPack(packLevel - 1).ValueMultiplier;
        else if (prefix == "특급") qualityMultiplier = 8;
        else if (prefix == "배급") qualityMultiplier = 3;
        else if (prefix == "폐기") qualityMultiplier = 1;
        else
        {
            value = 0;
            return false;
        }

        string cardName = item[(separator + 1)..];
        int index = System.Array.IndexOf(CardNames, cardName);
        if (index < 0)
        {
            value = 0;
            return false;
        }

        int baseValue = Mathf.Max(1, (index + 1) * (index + 2) / 2 * qualityMultiplier);
        value = GameEconomy.Scale(baseValue);
        return true;
    }

    public static bool IsGeneratedCard(string item) => TryGetCardValue(item, out _);

    private static int TotalExperienceForLevel(int level)
    {
        int completedLevels = Mathf.Max(0, level - 1);
        return 25 * completedLevels * (completedLevels + 1);
    }

    private static int PackQuality(string pack) => pack == "고급 카드팩" ? 2 : pack == "보급 카드팩" ? 1 : 0;
    private static string PackPrefix(int quality) => quality == 2 ? "특급·" : quality == 1 ? "배급·" : "폐기·";
}

public readonly struct CardPackDefinition
{
    public readonly string Name;
    public readonly int RequiredLevel;
    public readonly int BasePrice;
    public readonly float LuckBonus;
    public readonly int ValueMultiplier;
    public readonly int Experience;

    public CardPackDefinition(string name, int requiredLevel, int basePrice, float luckBonus, int valueMultiplier, int experience)
    {
        Name = name;
        RequiredLevel = requiredLevel;
        BasePrice = basePrice;
        LuckBonus = luckBonus;
        ValueMultiplier = valueMultiplier;
        Experience = experience;
    }
}
