using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameNotificationType
{
    Default,
    Success,
    Error
}

public sealed class GameNotificationCenter : MonoBehaviour
{
    private const int MaxVisible = 3;
    private static readonly Queue<Notice> Pending = new();
    private static GameNotificationCenter instance;

    private readonly List<GameObject> active = new();
    private Transform container;
    private Sprite roundedSprite;

    private readonly struct Notice
    {
        public readonly string Message;
        public readonly GameNotificationType Type;

        public Notice(string message, GameNotificationType type)
        {
            Message = message;
            Type = type;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstalled()
    {
        if (SceneManager.GetActiveScene().name != "Map") return;
        if (instance != null) return;
        instance = new GameObject("Game Notification Center").AddComponent<GameNotificationCenter>();
    }

    public static void Show(string message, GameNotificationType type = GameNotificationType.Default)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        if (instance == null && SceneManager.GetActiveScene().name == "Map")
            EnsureInstalled();
        if (instance == null) return;
        Pending.Enqueue(new Notice(message, type));
        instance.Pump();
    }

    public static void Success(string message) => Show(message, GameNotificationType.Success);
    public static void Error(string message) => Show(message, GameNotificationType.Error);

    private void Awake()
    {
        instance = this;
        BuildCanvas();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
        Pending.Clear();
    }

    private void BuildCanvas()
    {
        GameObject canvasObject = new("Notification Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 900;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = .5f;

        GameObject root = new("Notification Stack", typeof(RectTransform));
        root.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        container = root.transform;
    }

    private void Pump()
    {
        while (active.Count < MaxVisible && Pending.Count > 0)
            Spawn(Pending.Dequeue());
    }

    private void Spawn(Notice notice)
    {
        if (notice.Type == GameNotificationType.Success)
            SoundManager.Instance?.PlaySuccess();
        else if (notice.Type == GameNotificationType.Error)
            SoundManager.Instance?.PlayError();

        GameObject toast = new("Notification", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        toast.transform.SetParent(container, false);
        RectTransform rect = toast.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.sizeDelta = new Vector2(570, 68);
        Image background = toast.GetComponent<Image>();
        background.color = new Color(.105f, .11f, .12f, .97f);
        background.sprite = roundedSprite ??= CreateRoundedSprite();
        background.type = Image.Type.Sliced;
        background.raycastTarget = false;

        Shadow shadow = toast.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, .55f);
        shadow.effectDistance = new Vector2(5, -5);

        GameObject textObject = new("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(toast.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24, 10);
        textRect.offsetMax = new Vector2(-24, -10);
        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = notice.Message;
        label.fontSize = 19;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;
        label.color = notice.Type switch
        {
            GameNotificationType.Success => new Color(.34f, .92f, .58f, 1),
            GameNotificationType.Error => new Color(1f, .28f, .25f, 1),
            _ => Color.white
        };

        active.Add(toast);
        Relayout();
        StartCoroutine(AnimateToast(toast));
    }

    private IEnumerator AnimateToast(GameObject toast)
    {
        CanvasGroup group = toast.GetComponent<CanvasGroup>();
        RectTransform rect = toast.GetComponent<RectTransform>();
        group.alpha = 0f;
        rect.localScale = Vector3.one * .68f;

        yield return Animate(group, rect, .68f, 1.08f, 0f, 1f, .2f);
        yield return Animate(group, rect, 1.08f, 1f, 1f, 1f, .12f);
        yield return new WaitForSecondsRealtime(2.4f);
        yield return Animate(group, rect, 1f, .88f, 1f, 0f, .22f);

        active.Remove(toast);
        Destroy(toast);
        Relayout();
        Pump();
    }

    private static IEnumerator Animate(CanvasGroup group, RectTransform rect, float fromScale, float toScale, float fromAlpha, float toAlpha, float duration)
    {
        for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
        {
            float t = Mathf.Clamp01(time / duration);
            t = 1f - Mathf.Pow(1f - t, 3f);
            rect.localScale = Vector3.one * Mathf.LerpUnclamped(fromScale, toScale, t);
            group.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            yield return null;
        }
        rect.localScale = Vector3.one * toScale;
        group.alpha = toAlpha;
    }

    private void Relayout()
    {
        for (int index = 0; index < active.Count; index++)
            active[index].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 405 - index * 80);
    }

    private static Sprite CreateRoundedSprite()
    {
        const int size = 64;
        const int radius = 14;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        Color32[] pixels = new Color32[size * size];
        float half = (size - 1) * .5f;
        float inner = half - radius;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = Mathf.Max(Mathf.Abs(x - half) - inner, 0f);
            float dy = Mathf.Max(Mathf.Abs(y - half) - inner, 0f);
            pixels[y * size + x] = dx * dx + dy * dy <= radius * radius
                ? new Color32(255, 255, 255, 255)
                : new Color32(255, 255, 255, 0);
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(14, 14, 14, 14));
    }
}
