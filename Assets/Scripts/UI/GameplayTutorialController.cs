using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class GameplayTutorialController : MonoBehaviour
{
    private const string CompletedKey = "Tutorial.Gameplay.Completed";
    public static bool IsPlaying { get; private set; }
    public static bool IsBlockingGameplay =>
        IsPlaying && activeInstance != null && activeInstance.overlay != null && activeInstance.overlay.activeInHierarchy;

    private static GameplayTutorialController activeInstance;

    private string[] pages;
    private string topic;
    private int pageIndex;
    private GameObject overlay;
    private TextMeshProUGUI stepLabel;
    private TextMeshProUGUI titleLabel;
    private TextMeshProUGUI bodyLabel;
    private TextMeshProUGUI continueLabel;
    private CursorLockMode previousLock;
    private bool previousCursorVisible;
    private bool ownsPlayback;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        IsPlaying = false;
        activeInstance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallForNewGame()
    {
        if (SceneManager.GetActiveScene().name != "Map" || PlayerPrefs.GetInt(CompletedKey, 0) == 1) return;
        GameObject host = new("Gameplay Tutorial Controller");
        GameplayTutorialController controller = host.AddComponent<GameplayTutorialController>();
        controller.StartCoroutine(controller.WaitForIntro());
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(CompletedKey);
        string[] topics = { "computer", "night_market", "card_pack", "lockpick", "drill", "cutter" };
        foreach (string item in topics) PlayerPrefs.DeleteKey("Tutorial.Topic." + item);
        PlayerPrefs.Save();
    }

    public static void ShowContext(string contextTopic)
    {
        if (IsPlaying || PlayerPrefs.GetInt("Tutorial.Topic." + contextTopic, 0) == 1) return;
        string[] contextPages = ContextPages(contextTopic);
        if (contextPages == null || contextPages.Length == 0) return;
        GameObject host = new("Context Tutorial " + contextTopic);
        GameplayTutorialController controller = host.AddComponent<GameplayTutorialController>();
        controller.topic = contextTopic;
        controller.pages = contextPages;
        controller.Begin();
    }

    public static void ShowMainTutorial()
    {
        if (IsPlaying) return;
        GameObject host = new("Gameplay Tutorial Replay");
        GameplayTutorialController controller = host.AddComponent<GameplayTutorialController>();
        controller.topic = "gameplay";
        controller.pages = MainPages();
        controller.Begin();
    }

    private System.Collections.IEnumerator WaitForIntro()
    {
        // 모든 AfterSceneLoad 설치 함수가 실행되고 스토리 컨트롤러가 Awake될 시간을 준다.
        yield return null;
        yield return null;
        while (StoryIntroController.IsPlaying || DailyStoryController.IsPlaying) yield return null;
        yield return new WaitForSecondsRealtime(.25f);
        if (PlayerPrefs.GetInt(CompletedKey, 0) == 1)
        {
            Destroy(gameObject);
            yield break;
        }

        topic = "gameplay";
        pages = MainPages();
        Begin();
    }

    private void Begin()
    {
        if (IsPlaying && activeInstance != this) { Destroy(gameObject); return; }
        activeInstance = this;
        ownsPlayback = true;
        IsPlaying = true;
        previousLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        BuildUi();

        if (overlay == null)
        {
            Debug.LogError("Gameplay tutorial UI could not be created.");
            CancelPlayback();
            return;
        }

        ShowPage();
        StartCoroutine(UiOpenAnimator.Play(overlay));
    }

    private void Update()
    {
        if (!IsPlaying || activeInstance != this) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) Next();
    }

    private void BuildUi()
    {
        GameObject canvasObject = new("Gameplay Tutorial Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 32000;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = .5f;

        overlay = ImageObject("Tutorial Overlay", canvasObject.transform, new Color(0, 0, 0, .72f));
        Stretch(overlay.GetComponent<RectTransform>());
        Button advance = overlay.AddComponent<Button>();
        advance.transition = Selectable.Transition.None;
        advance.onClick.AddListener(Next);

        GameObject panel = ImageObject("Tutorial Panel", overlay.transform, new Color(.055f, .05f, .045f, .995f));
        SetRect(panel.GetComponent<RectTransform>(), new Vector2(0, -255), new Vector2(1320, 360));
        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(.58f, .18f, .07f, 1);
        outline.effectDistance = new Vector2(4, -4);
        GameObject header = ImageObject("Header", panel.transform, new Color(.48f, .13f, .045f, 1));
        SetRect(header.GetComponent<RectTransform>(), new Vector2(0, 142), new Vector2(1280, 58));

        stepLabel = Text(string.Empty, header.transform, new Vector2(485, 0), new Vector2(250, 28), 16, new Color(.9f, .75f, .65f, 1));
        stepLabel.alignment = TextAlignmentOptions.Right;
        titleLabel = Text(string.Empty, header.transform, new Vector2(-380, 0), new Vector2(470, 34), 23, Color.white);
        titleLabel.alignment = TextAlignmentOptions.Left;
        bodyLabel = Text(string.Empty, panel.transform, new Vector2(0, 20), new Vector2(1170, 160), 23, new Color(.92f, .89f, .82f, 1));
        bodyLabel.alignment = TextAlignmentOptions.TopLeft;
        bodyLabel.textWrappingMode = TextWrappingModes.Normal;
        continueLabel = Text("[ 클릭 / SPACE / ENTER : 다음 ]", panel.transform, new Vector2(415, -135), new Vector2(390, 28), 15, new Color(.72f, .4f, .28f, 1));

        if (topic == "gameplay")
        {
            Button skip = CreateButton("튜토리얼 건너뛰기", panel.transform, new Vector2(-475, -135), new Vector2(220, 38));
            skip.onClick.AddListener(Finish);
        }
        EnsureEventSystem();
    }

    private void ShowPage()
    {
        string page = pages[pageIndex];
        int split = page.IndexOf('\n');
        titleLabel.text = split >= 0 ? page[..split] : "시설 안내";
        bodyLabel.text = split >= 0 ? page[(split + 1)..] : page;
        stepLabel.text = topic == "gameplay" ? $"기본 교육  {pageIndex + 1} / {pages.Length}" : "신규 시스템 안내";
        continueLabel.text = pageIndex == pages.Length - 1 ? "[ 확인 : 닫기 ]" : "[ 클릭 / SPACE / ENTER : 다음 ]";
    }

    private void Next()
    {
        if (!IsPlaying) return;
        pageIndex++;
        if (pageIndex >= pages.Length) Finish();
        else ShowPage();
    }

    private void Finish()
    {
        if (!IsPlaying || activeInstance != this) return;
        if (topic == "gameplay") PlayerPrefs.SetInt(CompletedKey, 1);
        else PlayerPrefs.SetInt("Tutorial.Topic." + topic, 1);
        PlayerPrefs.Save();
        EndPlayback();
        Destroy(gameObject);
    }

    private void CancelPlayback()
    {
        EndPlayback();
        Destroy(gameObject);
    }

    private void EndPlayback()
    {
        if (activeInstance == this) activeInstance = null;
        IsPlaying = false;
        if (!ownsPlayback) return;
        ownsPlayback = false;

        // 컴퓨터/상점 UI 위에서 열린 안내라면 튜토리얼 이전 상태가 Locked였더라도
        // 창을 조작할 수 있도록 커서를 계속 해제해 둔다.
        if (GameplayUiController.IsTerminalOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = previousLock;
        Cursor.visible = previousCursorVisible;
    }

    private void OnDestroy()
    {
        if (activeInstance == this || ownsPlayback) EndPlayback();
    }

    private static string[] MainPages() => new[]
    {
        "DEBT PIT의 목적\n당신은 매일 노동값을 벌어 생존 청구액을 내야 합니다. 남은 돈을 자유 기금 100,000,000까지 채우면 시설을 나갈 수 있습니다.",
        "하루와 사형 규칙\n하루는 현실 시간 약 5분입니다. 자정까지 일일 노동값을 내지 않으면 처형되고 현재 저장은 무효화됩니다.",
        "반드시 컴퓨터에서 납부\n노동값을 보유하고 있는 것만으로는 납부되지 않습니다. 컴퓨터를 열고 ‘일일 납부’ 앱에서 직접 납부 실행을 눌러야 합니다.",
        "컴퓨터 단말기\n일일 납부, 자유 기금, 데일리 보상, 카드 상점, 도구 상점, 업그레이드, 위험 게임과 하루 넘기기를 사용할 수 있습니다.",
        "물건 투입구와 인벤토리\n컴퓨터와 야시장에서 구매한 물건은 투입구로 배송됩니다. E로 열고 원하는 물건만 수령하거나 모두 수령할 수 있습니다.",
        "작업대와 미니게임\n카드팩과 가챠 상자는 작업대에서 개봉합니다. 팩 레벨마다 해제 프로토콜과 난이도가 달라지며 정확한 플레이는 보상을 높입니다.",
        "상점과 야시장\n상점에서는 물건을 개별 흥정하거나 기본가로 전체 판매합니다. 야시장 가챠 상자는 1분마다 바뀌며 유료 리롤 비용은 같은 날 계속 증가합니다.",
        "카드 레벨과 성장\n카드팩을 열고 물건을 판매하면 경험치를 얻습니다. 레벨이 오르면 더 비싸고 운이 좋은 카드팩과 고수익 카드가 해금됩니다.",
        "조작 안내\nE: 상호작용 · 1~0/휠: 인벤토리 선택 · ESC: UI 닫기 · SPACE: 미니게임 판정. 새로운 시스템은 처음 만날 때 추가 설명이 표시됩니다."
    };

    private static string[] ContextPages(string context) => context switch
    {
        "computer" => new[] { "수용자 컴퓨터\n오늘 납부액은 반드시 ‘일일 납부’ 앱에서 직접 처리하십시오. 자유 기금 송금과 상품 구매, 하루 넘기기도 이 단말기에서 실행합니다." },
        "night_market" => new[] { "야시장 재고\n가챠 상자 3종이 60초마다 자동 갱신됩니다. 즉시 리롤도 가능하지만 같은 날 반복할수록 비용이 빠르게 증가합니다." },
        "card_pack" => new[] { "카드팩 보안 프로토콜\n카드팩 등급마다 미니게임 규칙과 속도, 성공 범위가 달라집니다. 정확도가 높으면 상위 카드 확률이 추가로 상승합니다." },
        "lockpick" => new[] { "락픽 타이밍\n표식이 초록 범위 안에 있을 때 SPACE를 누른 순간만 성공합니다. 희귀한 상자일수록 초록 범위가 좁아집니다." },
        "drill" => new[] { "드릴 과열 관리\nSPACE를 누르면 천공과 열이 오르고, 놓으면 냉각됩니다. 과열 전에 진행도 100%를 채우십시오." },
        "cutter" => new[] { "유압 절단기\n미니게임 없이 상자를 확정 해제하는 고가 일회용 도구입니다. 희귀 상자에 아껴 사용하는 것이 좋습니다." },
        _ => null
    };

    private static GameObject ImageObject(string name, Transform parent, Color color)
    {
        GameObject item = new(name, typeof(RectTransform), typeof(Image));
        item.transform.SetParent(parent, false);
        item.GetComponent<Image>().color = color;
        return item;
    }

    private static TextMeshProUGUI Text(string value, Transform parent, Vector2 position, Vector2 size, float fontSize, Color color)
    {
        GameObject item = new(value, typeof(RectTransform));
        item.transform.SetParent(parent, false);
        SetRect(item.GetComponent<RectTransform>(), position, size);
        TextMeshProUGUI text = item.AddComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(string label, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject item = ImageObject(label, parent, new Color(.25f, .12f, .07f, 1));
        SetRect(item.GetComponent<RectTransform>(), position, size);
        Button button = item.AddComponent<Button>();
        Text(label, item.transform, Vector2.zero, size - new Vector2(10, 6), 15, Color.white);
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
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
