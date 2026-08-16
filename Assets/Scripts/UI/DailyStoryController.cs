using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class DailyStoryController : MonoBehaviour
{
    public static bool IsPlaying { get; private set; }
    private static DailyStoryController instance;

    private Canvas canvas;
    private Image fade;
    private TextMeshProUGUI body;
    private TextMeshProUGUI continueLabel;
    private GameObject sceneImageFrame;
    private Image sceneImage;
    private Sprite runtimeSceneSprite;
    private Button yesButton;
    private Button noButton;
    private string[] pages;
    private int pageIndex;
    private bool failure;
    private bool isTyping;
    private bool awaitingChoice;
    private Coroutine typingRoutine;

    public static void BeginEndOfDay(bool paymentPaid)
    {
        if (IsPlaying) return;
        instance = new GameObject("Daily Story Controller").AddComponent<DailyStoryController>();
        instance.failure = !paymentPaid;
        instance.StartCoroutine(instance.BeginRoutine());
    }

    private void Awake()
    {
        instance = this;
        IsPlaying = true;
    }

    private void OnDestroy()
    {
        if (runtimeSceneSprite != null) Destroy(runtimeSceneSprite);
        if (instance == this) instance = null;
        IsPlaying = false;
        GameObject gameplayHud = GameObject.Find("Gameplay HUD");
        if (gameplayHud != null) gameplayHud.SetActive(true);
    }

    private IEnumerator BeginRoutine()
    {
        GameplayUiController gameplayUi = FindFirstObjectByType<GameplayUiController>();
        if (gameplayUi != null) gameplayUi.CloseForSystemTransition();
        BuildCanvas();
        yield return Fade(0f, 1f, .45f);

        if (failure)
        {
            pages = FailurePages();
        }
        else
        {
            GameDayClock.CompletePaidDay();
            pages = StoryForDay(GameSaveService.Day);
        }

        BuildDialog();
        yield return Fade(1f, 0f, .5f);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ShowPage();
    }

    private void Update()
    {
        if (body == null || awaitingChoice) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            NextPage();
    }

    private void BuildCanvas()
    {
        GameObject canvasObject = new("Daily Story Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 850;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = .5f;

        GameObject fadeObject = ImageObject("Day Fade", canvasObject.transform, Color.black);
        Stretch(fadeObject.GetComponent<RectTransform>());
        fade = fadeObject.GetComponent<Image>();
        fade.raycastTarget = false;
        fade.color = new Color(0, 0, 0, 0);
    }

    private void BuildDialog()
    {
        GameObject shade = ImageObject("Story Shade", canvas.transform, new Color(0, 0, 0, .55f));
        Stretch(shade.GetComponent<RectTransform>());

        GameObject topBar = ImageObject("Top Cinema Bar", canvas.transform, new Color(0, 0, 0, .94f));
        SetRect(topBar.GetComponent<RectTransform>(), new Vector2(0, 490), new Vector2(1920, 100));
        Text(failure ? "DEBT PIT // PAYMENT DEFAULT" : $"DEBT PIT // DAY {GameSaveService.Day:00}", topBar.transform, Vector2.zero, new Vector2(1100, 50), 23, failure ? new Color(1f, .18f, .15f, 1) : new Color(.82f, .12f, .08f, 1));

        sceneImageFrame = ImageObject("Story Image Frame", canvas.transform, new Color(.025f, .022f, .018f, .98f));
        SetRect(sceneImageFrame.GetComponent<RectTransform>(), new Vector2(0, 95), new Vector2(820, 430));
        Outline imageOutline = sceneImageFrame.AddComponent<Outline>();
        imageOutline.effectColor = new Color(.34f, .12f, .07f, .9f);
        imageOutline.effectDistance = new Vector2(4, -4);
        GameObject imageObject = ImageObject("Story Image", sceneImageFrame.transform, Color.white);
        SetRect(imageObject.GetComponent<RectTransform>(), Vector2.zero, new Vector2(792, 402));
        sceneImage = imageObject.GetComponent<Image>();
        sceneImage.preserveAspect = true;
        sceneImage.raycastTarget = false;

        GameObject panel = ImageObject("Daily Briefing", canvas.transform, new Color(.92f, .91f, .86f, .99f));
        SetRect(panel.GetComponent<RectTransform>(), new Vector2(0, -360), new Vector2(1340, 260));
        Shadow shadow = panel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, .65f);
        shadow.effectDistance = new Vector2(8, -8);
        GameObject accent = ImageObject("Accent", panel.transform, failure ? new Color(.65f, .015f, .01f, 1) : new Color(.82f, .08f, .05f, 1));
        SetRect(accent.GetComponent<RectTransform>(), new Vector2(-652, 0), new Vector2(10, 260));
        GameObject tag = ImageObject("Name Tag", panel.transform, failure ? new Color(.65f, .015f, .01f, 1) : new Color(.82f, .08f, .05f, 1));
        SetRect(tag.GetComponent<RectTransform>(), new Vector2(-500, 98), new Vector2(290, 50));
        Text(failure ? "처분 관리 시스템" : "시설 안내 시스템", tag.transform, Vector2.zero, new Vector2(270, 38), 20, Color.white);

        body = Text(string.Empty, panel.transform, new Vector2(0, -15), new Vector2(1120, 105), 27, new Color(.075f, .065f, .055f, 1));
        body.alignment = TextAlignmentOptions.TopLeft;
        body.textWrappingMode = TextWrappingModes.Normal;
        body.overflowMode = TextOverflowModes.Ellipsis;
        continueLabel = Text(string.Empty, panel.transform, new Vector2(480, -100), new Vector2(330, 28), 15, new Color(.38f, .1f, .08f, 1));

        yesButton = CreateButton("예 · 다시 시작", panel.transform, new Vector2(-130, -83), new Vector2(220, 48), new Color(.13f, .35f, .2f, 1));
        noButton = CreateButton("아니오 · 타이틀", panel.transform, new Vector2(130, -83), new Vector2(220, 48), new Color(.35f, .08f, .06f, 1));
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        yesButton.onClick.AddListener(RestartGame);
        noButton.onClick.AddListener(ReturnToTitle);

        fade.transform.SetAsLastSibling();
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
        if (sceneImage == null || sceneImageFrame == null) return;
        string resourceName = StoryImageForPage(failure, GameSaveService.Day, pageIndex);
        Texture2D texture = string.IsNullOrEmpty(resourceName) ? null : Resources.Load<Texture2D>("Story/" + resourceName);
        sceneImageFrame.SetActive(texture != null);
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
            body.text = pages[pageIndex];
            isTyping = false;
            continueLabel.text = "[ 클릭 또는 SPACE : 계속 ]";
            return;
        }

        pageIndex++;
        if (pageIndex < pages.Length)
        {
            ShowPage();
            return;
        }

        if (failure) ShowRestartChoice();
        else FinishDayStory();
    }

    private IEnumerator TypePage()
    {
        isTyping = true;
        body.text = string.Empty;
        continueLabel.text = string.Empty;
        string page = pages[pageIndex];
        int visibleGlyphs = 0;
        for (int index = 0; index < page.Length; index++)
        {
            body.text += page[index];
            if (!char.IsWhiteSpace(page[index]))
            {
                visibleGlyphs++;
                if (visibleGlyphs % 3 == 0)
                    SoundManager.Instance?.PlayDialogueType();
            }
            yield return new WaitForSecondsRealtime(page[index] == '\n' ? .07f : .022f);
        }
        isTyping = false;
        continueLabel.text = "[ 클릭 또는 SPACE : 계속 ]";
    }

    private void ShowRestartChoice()
    {
        GameSaveService.InvalidateSave();
        awaitingChoice = true;
        body.text = "지급 능력이 없는 수용자의 기록은 여기서 종료된다.\n\n다시 시작하시겠습니까?";
        continueLabel.text = string.Empty;
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        StartCoroutine(UiOpenAnimator.Play(yesButton.transform.parent.gameObject));
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void FinishDayStory()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Destroy(gameObject);
    }

    private void RestartGame()
    {
        yesButton.interactable = false;
        noButton.interactable = false;
        GameSaveService.StartNewGame();
        SceneFade.Load("Loading");
    }

    private void ReturnToTitle()
    {
        yesButton.interactable = false;
        noButton.interactable = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneFade.Load("Title");
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        fade.transform.SetAsLastSibling();
        for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
        {
            float t = Mathf.Clamp01(time / duration);
            t = t * t * (3f - 2f * t);
            fade.color = new Color(0, 0, 0, Mathf.Lerp(from, to, t));
            yield return null;
        }
        fade.color = new Color(0, 0, 0, to);
    }

    private static string[] FailurePages() => new[]
    {
        "00:00. 시설 시계가 멈췄다.\n단말기에는 단 한 줄만 남았다.  ‘일일 노동값 미납.’",
        "잠금 장치가 순서대로 닫힌다. 환풍기 소리가 멎고, 복도 끝에서 무거운 발소리가 가까워진다.",
        "시설은 거짓말을 하지 않는다.\n살아 있을 권리를 구매하지 못한 채무자는 다음 날을 맞이하지 못한다."
    };

    private static string[] StoryForDay(int day) => day switch
    {
        2 => new[] { "두 번째 아침이다. 투입구 안쪽에서 누군가 세 번 두드렸다. 확인했을 때는 아무것도 없었다.", "단말기는 오늘의 청구액을 표시했다. 어제보다 숫자가 커졌지만, 설명은 없었다." },
        3 => new[] { "세 번째 아침, 물건 투입구에서 배급 캔 하나가 굴러 나왔다. 주문한 적 없는 물건이었다.", "캔의 이중 바닥을 뜯자 접힌 편지가 나왔다.  ‘자유 기금은 출구가 아니라 카운터다.’" },
        4 => new[] { "새벽 동안 다른 단말기에서 접속 신호가 들어왔다. 사용자 이름은 당신과 같은 ‘익명’이었다.", "메시지는 한 글자도 오지 않았다. 대신 자유 기금 창의 숫자가 1만큼 올라가 있었다." },
        5 => new[] { "캔 식량의 바닥에서 사진 조각이 나왔다. 얼굴은 찢겨 있었지만, 뒷면의 필체는 이상하게 익숙했다.", "‘이번에는 돈을 내지 마.’ 사진 아래에는 오늘 날짜가 적혀 있었다." },
        6 => new[] { "야시장 상인은 당신을 보자 이미 거래한 사람처럼 고개를 끄덕였다.", "처음 보는 얼굴이라고 말하자 상인은 웃었다.  ‘다들 처음에는 그렇게 말하지.’" },
        7 => new[] { "자유 기금 서버가 잠시 오류를 냈다. 화면에는 목표 금액 대신 ‘회수 예정 기억: 100%’가 표시됐다.", "오류 보고서를 열기 전에 시스템이 스스로 재부팅됐다." },
        8 => new[] { "처분 기록에서 당신의 수용자 번호를 발견했다. 같은 번호가 이미 열세 번 사형 처리되어 있었다.", "각 기록의 마지막 문장은 같았다.  ‘대상 기억 초기화 후 재배치.’" },
        9 => new[] { "물건 투입구로 녹음기가 배송됐다. 재생 버튼을 누르자 당신의 목소리가 흘러나왔다.", "‘이걸 듣고 있다면 또 실패한 거야. 출구에 도착해도 절대 눈을 감지 마.’" },
        10 => new[] { "중앙 감사 시스템이 접속했다. 노동 실적은 우수, 순응도는 안정, 기억 잔존율은 위험 수준이라고 적혀 있었다.", "마지막 항목만 붉었다.  ‘진실 인지 시 즉시 회수.’" },
        11 => new[] { "오늘부터 거래 가격이 폭발적으로 오르기 시작했다. 시설은 이를 ‘성실 노동 보너스’라고 불렀다.", "하지만 청구액도 같은 속도로 오른다. 보상과 빚이 함께 커지도록 설계된 것이다." },
        12 => new[] { "봉인된 상자에서 오래된 출구 카드가 나왔다. 카드에는 당신의 지문이 이미 등록되어 있었다.", "등록 일자는 수감되기 전이었다." },
        13 => new[] { "시설 방송이 처음으로 당신의 이름을 부르려 했다. 잡음 뒤에 들린 것은 짧은 기계음뿐이었다.", "단말기 기록에는 ‘삭제된 이름 복구 시도 47회’라고 남았다." },
        14 => new[] { "출구 엘리베이터가 잠시 열렸다. 안에는 밖으로 향하는 버튼이 없고, 지하로 내려가는 버튼만 있었다.", "버튼 옆에는 작은 문구가 적혀 있었다.  ‘신규 수용 절차.’" },
        15 => new[] { "자유 기금 목표가 가까워지자 시스템이 축하 메시지를 보냈다. 그 아래 아주 작은 글씨가 깜빡였다.", "‘지급 완료 시 대상 기억 제거 및 DAY 1 재배치.’ 이제 출구가 무엇인지 알 것 같다." },
        _ => new[] { $"DAY {day:00}. 시설은 여전히 정상이라고 주장한다. 하지만 밤마다 벽 너머에서 같은 작업음이 반복된다.", "누군가가 당신보다 먼저 이 하루를 살았고, 어쩌면 그 누군가도 당신이었을 것이다." }
    };

    private static string StoryImageForPage(bool isFailure, int day, int page)
    {
        if (isFailure) return "failure_execution_corridor";
        if (day == 3) return page == 0 ? "day_03_ration_can" : "day_03_hidden_letter";
        return day switch
        {
            2 => "day_02_delivery_chute",
            4 => "day_04_terminal_signal",
            5 => "day_05_torn_photo",
            6 => "day_06_night_merchant",
            7 => "day_07_memory_error",
            8 => "day_08_execution_records",
            9 => "day_09_voice_recorder",
            10 => "day_10_audit_warning",
            11 => "day_11_price_spike",
            12 => "day_12_exit_card",
            13 => "day_13_deleted_name",
            14 => "day_14_elevator",
            15 => "day_15_reset_contract",
            _ => "day_default_cell_wall"
        };
    }

    private static GameObject ImageObject(string name, Transform parent, Color color)
    {
        GameObject item = new(name, typeof(RectTransform), typeof(Image));
        item.transform.SetParent(parent, false);
        item.GetComponent<Image>().color = color;
        return item;
    }

    private static TextMeshProUGUI Text(string value, Transform parent, Vector2 position, Vector2 size, float fontSize, Color color)
    {
        GameObject item = new("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        item.transform.SetParent(parent, false);
        SetRect(item.GetComponent<RectTransform>(), position, size);
        TextMeshProUGUI text = item.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(string label, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject item = ImageObject(label + " Button", parent, color);
        SetRect(item.GetComponent<RectTransform>(), position, size);
        Button button = item.AddComponent<Button>();
        Text(label, item.transform, Vector2.zero, size - new Vector2(12, 8), 17, Color.white);
        return button;
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
