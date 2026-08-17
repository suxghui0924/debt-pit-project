using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class StoryIntroController : MonoBehaviour
{
    public static bool IsPlaying { get; private set; }

    private static readonly string[] KoreanPages =
    {
        "…기록을 불러옵니다.\n\n피수용자 식별명: 삭제됨.",
        "당신은 이름을 잃었다.\n사회에 남긴 죄의 대가로, 기억 제거 처분을 받았다.",
        "눈을 뜬 곳은 DEBT PIT.\n창문도, 시계도, 출구 표지판도 없는 지하 수용 구역이다.",
        "여기서 당신은 죄수가 아니다.\n매일 살아 있을 권리를 구매해야 하는 채무자다.",
        "매일 자정, 시설은 당신에게 ‘노동값’을 청구한다.\n납부하지 못한 채무자는 다음 날을 맞이하지 못한다.",
        "노동은 이곳의 유일한 화폐다.\n상점에서 팔고, 컴퓨터로 송금하고, 필요하다면 운에 맡겨라.",
        "복도 끝의 물건 투입구를 확인하라.\n승인된 물품과 매일의 공급품은 그곳으로 도착한다.",
        "그리고 기억해라. 시설은 거짓말을 하지 않는다.\n‘자유 기금’을 모두 납부하면, 당신은 이곳을 나갈 수 있다.",
        "…단, 이전 수용자들도 같은 문장을 들었다.\n그들의 이름은 모두 삭제되어 있다.",
        "첫 번째 청구까지 남은 시간: 23:59:59\n\nDAY 1이 시작된다."
    };

    private static readonly string[] EnglishPages =
    {
        "…LOADING RECORD.\n\nDETAINEE IDENTIFIER: DELETED.",
        "You have lost your name.\nFor the crime you left upon society, you were sentenced to memory removal.",
        "You open your eyes in DEBT PIT.\nAn underground detention block with no windows, clocks, or exit signs.",
        "You are not a prisoner here.\nYou are a debtor who must purchase the right to remain alive each day.",
        "At midnight, the facility charges your daily labor fee.\nDebtors who fail to pay do not see the next day.",
        "Labor is the only currency here.\nSell at the shop, transfer it by computer, or leave it to chance if you must.",
        "Check the delivery chute at the end of the corridor.\nApproved purchases and daily supplies arrive there.",
        "And remember: the facility does not lie.\nComplete the Freedom Fund, and you may leave this place.",
        "…But every prisoner before you heard the same sentence.\nAll of their names have been deleted.",
        "TIME UNTIL FIRST PAYMENT: 23:59:59\n\nDAY 1 BEGINS."
    };

    private static string[] Pages => GameLanguage.IsEnglish ? EnglishPages : KoreanPages;

    private int pageIndex;
    private TextMeshProUGUI body;
    private TextMeshProUGUI continueLabel;
    private Image sceneImage;
    private Sprite runtimeSceneSprite;
    private Coroutine typingRoutine;
    private bool isTyping;

    private void Awake() => IsPlaying = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void EnsureInstalled()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Map") return;
        if (GameSaveService.IntroSeen || FindFirstObjectByType<StoryIntroController>() != null) return;
        new GameObject("Story Intro Controller").AddComponent<StoryIntroController>();
    }

    private void Start()
    {
        Build();
        ShowPage();
    }

    private void OnDestroy()
    {
        if (runtimeSceneSprite != null) Destroy(runtimeSceneSprite);
        IsPlaying = false;
        GameObject gameplayHud = GameObject.Find("Gameplay HUD");
        if (gameplayHud != null) gameplayHud.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            NextPage();
    }

    private void Build()
    {
        GameObject canvasObject = new("Story Intro Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject shade = ImageObject("Shade", canvasObject.transform, new Color(0, 0, 0, .62f));
        Stretch(shade.GetComponent<RectTransform>());
        GameObject topBar = ImageObject("Top Bar", canvasObject.transform, new Color(0, 0, 0, .94f));
        SetRect(topBar.GetComponent<RectTransform>(), new Vector2(0, 480), new Vector2(1920, 120));
        CreateText("DEBT PIT // PENAL LABOR FACILITY", canvasObject.transform, new Vector2(0, 480), new Vector2(1100, 60), 25, new Color(.84f, .08f, .06f, 1));

        GameObject sceneFrame = ImageObject("Intro Image Frame", canvasObject.transform, new Color(.025f, .022f, .018f, .98f));
        SetRect(sceneFrame.GetComponent<RectTransform>(), new Vector2(0, 95), new Vector2(820, 430));
        Outline frameOutline = sceneFrame.AddComponent<Outline>();
        frameOutline.effectColor = new Color(.34f, .12f, .07f, .9f);
        frameOutline.effectDistance = new Vector2(4, -4);
        GameObject sceneObject = ImageObject("Intro Image", sceneFrame.transform, Color.white);
        SetRect(sceneObject.GetComponent<RectTransform>(), Vector2.zero, new Vector2(792, 402));
        sceneImage = sceneObject.GetComponent<Image>();
        sceneImage.preserveAspect = true;
        sceneImage.raycastTarget = false;

        GameObject panel = ImageObject("Briefing Panel", canvasObject.transform, new Color(.92f, .91f, .86f, .98f));
        SetRect(panel.GetComponent<RectTransform>(), new Vector2(0, -365), new Vector2(1340, 235));
        GameObject accent = ImageObject("Accent", panel.transform, new Color(.84f, .08f, .06f, 1));
        SetRect(accent.GetComponent<RectTransform>(), new Vector2(-652, 0), new Vector2(8, 235));
        GameObject nameTag = ImageObject("Name Tag", panel.transform, new Color(.84f, .08f, .06f, 1));
        SetRect(nameTag.GetComponent<RectTransform>(), new Vector2(-500, 88), new Vector2(270, 50));
        CreateText("시설 안내 시스템", nameTag.transform, Vector2.zero, new Vector2(250, 40), 20, Color.white);
        body = CreateText(string.Empty, panel.transform, new Vector2(0, -20), new Vector2(1130, 92), 27, new Color(.08f, .07f, .06f, 1));
        body.alignment = TextAlignmentOptions.TopLeft;
        body.textWrappingMode = TextWrappingModes.Normal;
        body.overflowMode = TextOverflowModes.Ellipsis;
        body.margin = new Vector4(0, 0, 0, 0);
        continueLabel = CreateText(string.Empty, panel.transform, new Vector2(480, -84), new Vector2(320, 28), 16, new Color(.38f, .1f, .08f, 1));
    }

    private void ShowPage()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        SoundManager.Instance?.PlayDialoguePage();
        UpdateSceneImage();
        typingRoutine = StartCoroutine(TypePage());
    }

    private void UpdateSceneImage()
    {
        if (sceneImage == null) return;
        string resourceName = pageIndex switch
        {
            <= 1 => "intro_memory_erasure",
            <= 3 => "intro_debt_pit_cell",
            <= 5 => "intro_labor_payment",
            6 => "day_02_delivery_chute",
            <= 8 => "intro_freedom_door",
            _ => "intro_labor_payment"
        };
        Texture2D texture = Resources.Load<Texture2D>("Story/" + resourceName);
        if (texture == null) return;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        if (runtimeSceneSprite != null) Destroy(runtimeSceneSprite);
        runtimeSceneSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100f);
        sceneImage.sprite = runtimeSceneSprite;
    }

    private void NextPage()
    {
        SoundManager.Instance?.PlayDialogueAdvance();
        if (isTyping)
        {
            StopCoroutine(typingRoutine);
            body.text = Pages[pageIndex];
            isTyping = false;
            continueLabel.text = GameLanguage.IsEnglish ? "[ CLICK OR SPACE ]" : "[ 클릭 또는 SPACE ]";
            return;
        }

        pageIndex++;
        if (pageIndex < Pages.Length)
        {
            ShowPage();
            return;
        }

        GameSaveService.MarkIntroSeen();
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator TypePage()
    {
        isTyping = true;
        body.text = string.Empty;
        continueLabel.text = string.Empty;
        string line = Pages[pageIndex];
        int visibleGlyphs = 0;
        for (int index = 0; index < line.Length; index++)
        {
            body.text += line[index];
            if (!char.IsWhiteSpace(line[index]))
            {
                visibleGlyphs++;
                if (visibleGlyphs % 3 == 0)
                    SoundManager.Instance?.PlayDialogueType();
            }
            yield return new WaitForSecondsRealtime(line[index] == '\n' ? .08f : .025f);
        }

        isTyping = false;
        continueLabel.text = GameLanguage.IsEnglish
            ? pageIndex == Pages.Length - 1 ? "[ CLICK OR SPACE : BEGIN ]" : "[ CLICK OR SPACE : CONTINUE ]"
            : pageIndex == Pages.Length - 1 ? "[ 클릭 또는 SPACE : 시작 ]" : "[ 클릭 또는 SPACE : 계속 ]";
    }

    private static GameObject ImageObject(string name, Transform parent, Color color)
    {
        GameObject image = new(name, typeof(RectTransform), typeof(Image));
        image.transform.SetParent(parent, false);
        image.GetComponent<Image>().color = color;
        return image;
    }

    private static TextMeshProUGUI CreateText(string text, Transform parent, Vector2 position, Vector2 size, float fontSize, Color color)
    {
        GameObject textObject = new("Text", typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = GameLanguage.Runtime(text);
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        return label;
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
