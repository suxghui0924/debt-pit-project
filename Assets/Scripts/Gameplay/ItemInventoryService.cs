using UnityEngine;

public static class ItemInventoryService
{
    public const int MaxSlots = 10;
    private const string SlotKey = "Inventory.Slot.";
    private const string DeliveryKey = "Delivery.Item.";
    private const string DeliveryCountKey = "Delivery.Count";
    private const string InitialDeliveryKey = "Delivery.Initialized";

    public static string GetItem(int slot) => PlayerPrefs.GetString(SlotKey + slot, string.Empty);
    public static int GetValue(string item)
    {
        if (CardProgressionService.TryGetCardValue(item, out int cardValue)) return cardValue;
        int packPrice = CardProgressionService.GetPackPrice(item);
        if (packPrice > 0) return packPrice;
        return GameEconomy.Scale(item switch
        {
            "작업 장갑" => 1,
            "캔 식량" => 2,
            "잠금 상자" => 4,
            "봉인된 상자" => 8,
            "녹슨 가챠 상자" => 3,
            "보급 가챠 상자" => 9,
            "군수 가챠 상자" => 25,
            "검은 금고" => 70,
            "락핀" => 3,
            "휴대용 드릴" => 14,
            "유압 절단기" => 35,
            "미니 노트북" => 24,
            "신호 복호기" => 45,
            "냉각 스프레이" => 8,
            "무료 카드팩" => 1,
            "낡은 카드팩" => 2,
            "보급 카드팩" => 5,
            "고급 카드팩" => 12,
            "녹슨 톱니 카드" => 3,
            "작업 속도 카드" => 8,
            "황금 계약 카드" => 25,
            _ => 1
        });
    }

    public static bool IsCardPack(string item) => CardProgressionService.IsCardPack(item);

    public static bool IsLootBox(string item)
    {
        return item == "봉인된 상자" || item == "녹슨 가챠 상자" || item == "보급 가챠 상자" || item == "군수 가챠 상자" || item == "검은 금고";
    }

    public static int GetLootBoxRarity(string item) => item switch
    {
        "검은 금고" => 5,
        "군수 가챠 상자" => 4,
        "봉인된 상자" => 3,
        "보급 가챠 상자" => 2,
        _ => 1
    };

    public static string RollCard(string pack, float minigameLuckBonus = 0f)
    {
        return CardProgressionService.RollCard(pack, minigameLuckBonus);
    }

    public static void SetItem(int slot, string item)
    {
        PlayerPrefs.SetString(SlotKey + slot, item ?? string.Empty);
        PlayerPrefs.Save();
    }

    public static bool TryAdd(string item, int capacity)
    {
        for (int index = 0; index < capacity; index++)
        {
            if (!string.IsNullOrEmpty(GetItem(index))) continue;
            SetItem(index, item);
            return true;
        }
        return false;
    }

    public static void AddStarterDeliveries()
    {
        PlayerPrefs.SetInt(DeliveryCountKey, 2);
        PlayerPrefs.SetString(DeliveryKey + 0, "작업 장갑");
        PlayerPrefs.SetString(DeliveryKey + 1, "캔 식량");
        PlayerPrefs.SetInt(InitialDeliveryKey, 1);
        PlayerPrefs.Save();
    }

    public static void EnsureInitialDelivery()
    {
        if (PlayerPrefs.GetInt(InitialDeliveryKey, 0) == 1) return;
        AddStarterDeliveries();
    }

    public static int DeliveryCount => PlayerPrefs.GetInt(DeliveryCountKey, 0);
    public static string GetDelivery(int index) => PlayerPrefs.GetString(DeliveryKey + index, string.Empty);

    public static void QueueDelivery(string item)
    {
        int index = DeliveryCount;
        PlayerPrefs.SetString(DeliveryKey + index, item);
        PlayerPrefs.SetInt(DeliveryCountKey, index + 1);
        PlayerPrefs.Save();
    }

    public static int UsedSlots(int capacity)
    {
        int used = 0;
        for (int index = 0; index < capacity; index++)
            if (!string.IsNullOrEmpty(GetItem(index))) used++;
        return used;
    }

    public static int ClaimAll(int capacity)
    {
        int claimed = 0;
        int count = DeliveryCount;
        string[] remaining = new string[count];
        int remainingCount = 0;
        for (int index = 0; index < count; index++)
        {
            string item = GetDelivery(index);
            if (string.IsNullOrEmpty(item)) continue;
            if (TryAdd(item, capacity)) claimed++;
            else remaining[remainingCount++] = item;
        }
        for (int index = 0; index < count; index++) PlayerPrefs.DeleteKey(DeliveryKey + index);
        for (int index = 0; index < remainingCount; index++) PlayerPrefs.SetString(DeliveryKey + index, remaining[index]);
        PlayerPrefs.SetInt(DeliveryCountKey, remainingCount);
        PlayerPrefs.Save();
        return claimed;
    }

    public static bool ClaimAt(int deliveryIndex, int capacity)
    {
        int count = DeliveryCount;
        if (deliveryIndex < 0 || deliveryIndex >= count) return false;
        string item = GetDelivery(deliveryIndex);
        if (string.IsNullOrEmpty(item) || !TryAdd(item, capacity)) return false;

        for (int index = deliveryIndex; index < count - 1; index++)
            PlayerPrefs.SetString(DeliveryKey + index, GetDelivery(index + 1));
        PlayerPrefs.DeleteKey(DeliveryKey + (count - 1));
        PlayerPrefs.SetInt(DeliveryCountKey, count - 1);
        PlayerPrefs.Save();
        return true;
    }
}
