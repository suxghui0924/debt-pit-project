using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class LoadingScreenController : MonoBehaviour
{
    private static readonly string[] Tips =
    {
        "TIP: RUNNING MAKES NOISE.",
        "TIP: CHECK EVERY CORNER.",
        "TIP: KEEP YOUR DEBT UNDER CONTROL.",
        "TIP: LISTEN BEFORE YOU MOVE."
    };

    private TextMeshProUGUI progressLabel;
    private Image progressFill;
    private RectTransform spinner;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void EnsureInstalled()
    {
        if (SceneManager.GetActiveScene().name != "Loading") return;
        if (FindFirstObjectByType<LoadingScreenController>() != null) return;
        new GameObject("Loading Screen Controller").AddComponent<LoadingScreenController>();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        BuildScreen();
        StartCoroutine(LoadMap());
    }

    private void Update()
    {
        if (spinner != null)
            spinner.Rotate(0f, 0f, -220f * Time.unscaledDeltaTime);
    }

    private IEnumerator LoadMap()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("Map");
        operation.allowSceneActivation = false;
        const float fakeLoadingDuration = 3f;
        float elapsed = 0f;
        bool mapActivated = false;

        while (elapsed < fakeLoadingDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / fakeLoadingDuration);
            progressFill.fillAmount = progress;
            progressLabel.text = $"LOADING {Mathf.RoundToInt(progress * 100f)}%";

            yield return null;
        }

        progressFill.fillAmount = 1f;
        progressLabel.text = "LOADING 100%";
        yield return new WaitForSecondsRealtime(0.35f);

        while (!mapActivated && operation.progress < .9f)
            yield return null;

        if (!mapActivated)
        {
            mapActivated = true;
            operation.allowSceneActivation = true;
        }

        while (!operation.isDone)
            yield return null;

        Destroy(gameObject);
    }

    private void BuildScreen()
    {
        GameObject canvasObject = new("Loading Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        CreateLetterboxBar("Top Letterbox", canvasObject.transform, new Vector2(0, 480));
        CreateLetterboxBar("Bottom Letterbox", canvasObject.transform, new Vector2(0, -480));
        CreateLogo(canvasObject.transform);
        CreateText(Tips[Random.Range(0, Tips.Length)], canvasObject.transform, new Vector2(0, -330), 23, new Color(0.94f, 0.92f, 0.86f, 1f));
        progressLabel = CreateText("LOADING 0%", canvasObject.transform, new Vector2(0, -395), 22, new Color(0.94f, 0.92f, 0.86f, 1f));

        GameObject track = CreateImage("Progress Track", canvasObject.transform, new Color(0.16f, 0.13f, 0.11f, 1f));
        RectTransform trackRect = track.GetComponent<RectTransform>();
        trackRect.anchorMin = trackRect.anchorMax = new Vector2(.5f, .5f);
        trackRect.sizeDelta = new Vector2(620, 14);
        trackRect.anchoredPosition = new Vector2(0, -435);
        GameObject fill = CreateImage("Progress Fill", track.transform, new Color(0.84f, 0.08f, 0.06f, 1f));
        progressFill = fill.GetComponent<Image>();
        progressFill.type = Image.Type.Filled;
        progressFill.fillMethod = Image.FillMethod.Horizontal;
        progressFill.fillOrigin = 0;
        progressFill.fillAmount = 0f;
        Stretch(fill.GetComponent<RectTransform>());
        CreateSpinner(canvasObject.transform);
    }

    private static void CreateLetterboxBar(string name, Transform parent, Vector2 position)
    {
        GameObject bar = CreateImage(name, parent, new Color(0f, 0f, 0f, 0.96f));
        RectTransform rect = bar.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.sizeDelta = new Vector2(1920, 120);
        rect.anchoredPosition = position;
    }

    private static void CreateLogo(Transform parent)
    {
        Texture2D logoTexture = Resources.Load<Texture2D>("Loading/debtpit");
        if (logoTexture == null) return;

        GameObject logoObject = new("DEBT PIT Logo", typeof(RectTransform), typeof(RawImage));
        logoObject.transform.SetParent(parent, false);
        RectTransform rect = logoObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.sizeDelta = new Vector2(470, 90);
        rect.anchoredPosition = new Vector2(0, 480);
        RawImage logo = logoObject.GetComponent<RawImage>();
        logo.texture = logoTexture;
        logo.color = Color.white;
        logo.raycastTarget = false;
    }

    private void CreateSpinner(Transform parent)
    {
        GameObject spinnerObject = new("Loading Spinner", typeof(RectTransform));
        spinnerObject.transform.SetParent(parent, false);
        spinner = spinnerObject.GetComponent<RectTransform>();
        spinner.anchorMin = spinner.anchorMax = new Vector2(1f, 0f);
        spinner.pivot = new Vector2(.5f, .5f);
        spinner.sizeDelta = new Vector2(54, 54);
        spinner.anchoredPosition = new Vector2(-84, 84);

        for (int index = 0; index < 8; index++)
        {
            GameObject tick = CreateImage("Tick", spinner, new Color(0.94f, 0.92f, 0.86f, .25f + index * .09f));
            RectTransform tickRect = tick.GetComponent<RectTransform>();
            tickRect.anchorMin = tickRect.anchorMax = new Vector2(.5f, .5f);
            tickRect.sizeDelta = new Vector2(5, 14);
            tickRect.anchoredPosition = Quaternion.Euler(0, 0, index * 45) * Vector2.up * 21;
            tickRect.localRotation = Quaternion.Euler(0, 0, -index * 45);
        }
    }

    private static GameObject CreateImage(string name, Transform parent, Color color)
    {
        GameObject imageObject = new(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        imageObject.GetComponent<Image>().color = color;
        return imageObject;
    }

    private static TextMeshProUGUI CreateText(string text, Transform parent, Vector2 position, float size, Color color)
    {
        GameObject textObject = new("Text", typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.sizeDelta = new Vector2(900, 100);
        rect.anchoredPosition = position;
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = text;
        label.fontSize = size;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        return label;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
