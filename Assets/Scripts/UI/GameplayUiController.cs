using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class GameplayUiController : MonoBehaviour
{
    public static bool IsTerminalOpen { get; private set; }
    private const int DefaultSlots = 4;
    private const int MaxSlots = 10;
    private const float InteractionDistance = 2.5f;
    private static Sprite roundedPanelSprite;
    private static Sprite roundedSlotSprite;

    private readonly Image[] slotFrames = new Image[MaxSlots];
    private Transform player;
    private Transform computer;
    private ComputerRadioPlayer computerRadio;
    private Transform shop;
    private Transform chute;
    private Transform workbench;
    private WorldInteractionTrigger computerTrigger;
    private WorldInteractionTrigger shopTrigger;
    private WorldInteractionTrigger chuteTrigger;
    private WorldInteractionTrigger workbenchTrigger;
    private readonly TextMeshProUGUI[] slotLabels = new TextMeshProUGUI[MaxSlots];
    private TextMeshProUGUI prompt;
    private TextMeshProUGUI dayLabel;
    private TextMeshProUGUI levelLabel;
    private TextMeshProUGUI timeLabel;
    private TextMeshProUGUI laborLabel;
    private TextMeshProUGUI paymentLabel;
    private TextMeshProUGUI freedomLabel;
    private TextMeshProUGUI computerTimeLabel;
    private int selectedSlot;
    private int slotCapacity;
    private GameObject terminalPanel;
    private Transform terminalContent;
    private GameObject hudRoot;
    private Transform activeTerminalTarget;
    private int haggleSlot = -1;
    private int haggleAttempts;
    private int hagglePrice;
    private string haggleMessage;
    private string riskMessage = "승률은 공개되지 않습니다.";
    private const string DeveloperKey = "suxghui";
    private string developerInput = string.Empty;
    private bool developerMode;
    private GameObject developerPanel;
    private GameObject developerBadge;
    private GameObject pausePanel;
    private float pauseRestoreTimeScale = 1f;
    private static readonly int[] RiskBetMultipliers = { 1, 2, 5, 10, 25, 50, 100 };
    private static readonly float[] DeveloperGameSpeeds = { 1f, 2f, 5f, 10f };
    private int riskBetIndex = 1;
    private bool skillCheckActive;
    private GameObject skillCheckOverlay;
    private RectTransform skillMarker;
    private float skillPhase;
    private int skillBoxSlot = -1;
    private int skillLockpinSlot = -1;
    private float skillZoneWidth;
    private bool packOpeningActive;
    private GameObject packOpeningOverlay;
    private RectTransform packOpeningMarker;
    private float packOpeningPhase;
    private float packOpeningTarget;
    private int packOpeningSlot = -1;
    private string packOpeningName;
    private int packOpeningProtocol;
    private float packOpeningSpeed;
    private float packOpeningZoneWidth;
    private bool packChargeStarted;
    private string packHackCode;
    private int packHackIndex;
    private TextMeshProUGUI packOpeningStatus;
    private bool drillCheckActive;
    private GameObject drillCheckOverlay;
    private RectTransform drillProgressFill;
    private RectTransform drillHeatFill;
    private float drillProgress;
    private float drillHeat;
    private int drillBoxSlot = -1;
    private int drillToolSlot = -1;
    private int drillCoolantSlot = -1;
    private TextMeshProUGUI nightMarketTimerLabel;
    private float nightMarketRefreshAt;
    private int nightMarketSeed;
    private int nightMarketRerolls;
    private int nightMarketDay;
    private float nextNightMarketSaveAt;
    private const string NightMarketDayKey = "Save.NightMarket.Day";
    private const string NightMarketSeedKey = "Save.NightMarket.Seed";
    private const string NightMarketRerollsKey = "Save.NightMarket.Rerolls";
    private const string NightMarketRemainingKey = "Save.NightMarket.Remaining";
    private static readonly string[] NightMarketBoxes =
    {
        "녹슨 가챠 상자", "보급 가챠 상자", "봉인된 상자", "군수 가챠 상자", "검은 금고"
    };
    private static readonly string[] PackProtocolNames =
    {
        "폐기물 자기띠 정렬", "철문 수직 스캐너", "야간 배급 회전 봉인", "교대 전력 안정화", "정비 단말기 침투",
        "감시 기록 파형 동기화", "영수증 광학 추적", "비상 전력 위상 조정", "검열 우편 축전", "폐기 승인망 우회",
        "지하 거래 신호 포착", "격리구역 레이저 정렬", "암호 장부 다이얼", "관리국 전압 인증", "검은 시장 포트 해킹",
        "기억 파편 공명", "보안 열쇠 수직 해독", "설계도 회전 복호", "판결문 잉크 충전", "탈출 경로 노트북 침투",
        "감독관 금고 주파수", "삭제 신원 생체 스캔", "중앙 서버 위상 잠금", "자유 채권 전하 서명", "집행 유예망 관리자 해킹",
        "기억 원본 신경 동기화", "정부 계정 심층 스캔", "시설 소유권 양자 다이얼", "면책 계약 코어 충전", "황금 자유계약 루트 해킹"
    };
    private static readonly string[] PackProtocolNamesEnglish =
    {
        "WASTE MAGSTRIPE ALIGNMENT", "IRON GATE VERTICAL SCAN", "NIGHT RATION ROTARY SEAL", "SHIFT POWER STABILIZATION", "MAINTENANCE TERMINAL BREACH",
        "SURVEILLANCE WAVE SYNC", "RECEIPT OPTICAL TRACE", "EMERGENCY POWER PHASE", "CENSORED MAIL CHARGE", "DISPOSAL NETWORK BYPASS",
        "UNDERGROUND SIGNAL TRACE", "QUARANTINE LASER ALIGNMENT", "CIPHER LEDGER DIAL", "AUTHORITY VOLTAGE CHECK", "BLACK MARKET PORT HACK",
        "MEMORY FRAGMENT RESONANCE", "SECURITY KEY DECODE", "BLUEPRINT ROTARY DECRYPTION", "VERDICT INK CHARGE", "ESCAPE ROUTE TERMINAL BREACH",
        "WARDEN VAULT FREQUENCY", "DELETED IDENTITY BIOMETRIC SCAN", "CENTRAL SERVER PHASE LOCK", "FREEDOM BOND CHARGE SIGNATURE", "STAY-OF-EXECUTION ADMIN HACK",
        "ORIGINAL MEMORY NEURAL SYNC", "GOVERNMENT ACCOUNT DEEP SCAN", "FACILITY OWNERSHIP QUANTUM DIAL", "IMMUNITY CONTRACT CORE CHARGE", "GOLDEN FREEDOM ROUTE HACK"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        IsTerminalOpen = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void EnsureInstalled()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Map") return;
        if (FindFirstObjectByType<GameplayUiController>() != null) return;
        new GameObject("Gameplay UI Controller").AddComponent<GameplayUiController>();
    }

    private void Start()
    {
        // Domain Reload를 끈 에디터에서도 이전 플레이의 열린 상태를 물려받지 않는다.
        IsTerminalOpen = false;
        player = Camera.main != null ? Camera.main.transform : null;
        computer = GameObject.Find("Computer")?.transform;
        if (computer != null) computerRadio = ComputerRadioPlayer.GetOrCreate(computer);
        shop = GameObject.Find("Shop")?.transform;
        chute = GameObject.Find("Object")?.transform;
        workbench = GameObject.Find("Task")?.transform;
        if (computer != null) computerTrigger = WorldInteractionTrigger.Create(computer, "Computer Interaction Trigger");
        if (shop != null) shopTrigger = WorldInteractionTrigger.Create(shop, "Shop Interaction Trigger");
        if (chute != null) chuteTrigger = WorldInteractionTrigger.Create(chute, "Chute Interaction Trigger");
        if (workbench != null) workbenchTrigger = WorldInteractionTrigger.Create(workbench, "Workbench Interaction Trigger");
        slotCapacity = Mathf.Clamp(PlayerPrefs.GetInt("Inventory.Capacity", DefaultSlots), DefaultSlots, MaxSlots);
        BuildUi();
        EnsureEventSystem();
        SelectSlot(0);
        RefreshInventoryUi();
    }

    private void Update()
    {
        if (nightMarketDay > 0 && Time.unscaledTime >= nextNightMarketSaveAt)
        {
            nextNightMarketSaveAt = Time.unscaledTime + 5f;
            SaveNightMarketState();
        }
        if (pausePanel != null)
        {
            HandlePauseMenuInput();
            return;
        }

        HandleDeveloperInput();
        if (developerPanel != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) CloseDeveloperPanel();
            return;
        }
        if (player == null) return;

        bool systemStoryPlaying = StoryIntroController.IsPlaying || DailyStoryController.IsPlaying || GameplayTutorialController.IsBlockingGameplay;
        if (hudRoot != null && hudRoot.activeSelf == systemStoryPlaying)
            hudRoot.SetActive(!systemStoryPlaying);
        if (systemStoryPlaying) return;

        RefreshStatusHud();
        if (computerTimeLabel != null) computerTimeLabel.text = FormatTime(GameDayClock.SecondsUntilMidnight);
        if (nightMarketTimerLabel != null)
        {
            float marketSeconds = Mathf.Max(0f, nightMarketRefreshAt - Time.unscaledTime);
            nightMarketTimerLabel.text = GameLanguage.IsEnglish ? $"AUTO REFRESH {FormatTime(marketSeconds)}" : $"자동 갱신 {FormatTime(marketSeconds)}";
            if (marketSeconds <= 0f)
            {
                AdvanceNightMarketStock();
                ShowNightMarket();
            }
        }

        if (packOpeningActive)
        {
            UpdateCardPackOpening();
            if (terminalPanel != null && !IsActiveTriggerInside()) CloseTerminal();
            return;
        }

        if (drillCheckActive)
        {
            UpdateDrillCheck();
            if (terminalPanel != null && !IsActiveTriggerInside()) CloseTerminal();
            return;
        }

        if (skillCheckActive)
        {
            UpdateSkillCheck();
            if (terminalPanel != null && !IsActiveTriggerInside()) CloseTerminal();
            return;
        }

        HandleSlotInput();
        Transform target = NearestInteractable(out string action);
        prompt.gameObject.SetActive(target != null && terminalPanel == null);
        if (target != null) prompt.text = $"[ E ]  {action}";
        if (target != null && terminalPanel == null && developerPanel == null && !IsTerminalOpen && Input.GetKeyDown(KeyCode.E))
            OpenTerminal(action, target);
        if (terminalPanel != null && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTerminal();
            return;
        }
        if (terminalPanel != null && !IsActiveTriggerInside())
            CloseTerminal();
        if (terminalPanel == null && Input.GetKeyDown(KeyCode.Escape))
            OpenPauseMenu();
    }

    private void HandleDeveloperInput()
    {
        if (developerMode && Input.GetKeyDown(KeyCode.F1))
        {
            if (developerPanel == null) OpenDeveloperPanel();
            else CloseDeveloperPanel();
            return;
        }

        foreach (char character in Input.inputString)
        {
            if (character == '\b')
            {
                if (developerInput.Length > 0) developerInput = developerInput[..^1];
                continue;
            }

            if (character == '\n' || character == '\r')
            {
                if (developerInput.EndsWith(DeveloperKey, System.StringComparison.OrdinalIgnoreCase))
                    ActivateDeveloperMode();
                developerInput = string.Empty;
                continue;
            }

            if (!char.IsLetter(character)) continue;
            developerInput += char.ToLowerInvariant(character);
            if (developerInput.Length > DeveloperKey.Length)
                developerInput = developerInput[^DeveloperKey.Length..];
        }
    }

    private void ActivateDeveloperMode()
    {
        developerMode = true;
        BuildDeveloperBadge();
        OpenDeveloperPanel();
        GameNotificationCenter.Success("개발자 모드가 활성화되었습니다.  F1로 다시 열 수 있습니다.");
    }

    private void HandleSlotInput()
    {
        float wheel = Input.mouseScrollDelta.y;
        if (wheel > 0) SelectSlot((selectedSlot - 1 + slotCapacity) % slotCapacity);
        if (wheel < 0) SelectSlot((selectedSlot + 1) % slotCapacity);

        for (int key = 0; key < slotCapacity; key++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + key))) SelectSlot(key);
        }
    }

    private Transform NearestInteractable(out string action)
    {
        action = string.Empty;
        Transform target = null;
        if (computer != null && computerTrigger != null && computerTrigger.PlayerInside)
        {
            target = computer;
            action = GameLanguage.IsEnglish ? "OPEN COMPUTER" : "컴퓨터 열기";
        }

        if (shop != null && shopTrigger != null && shopTrigger.PlayerInside)
        {
            target = shop;
            action = GameLanguage.IsEnglish ? "OPEN SHOP" : "상점 열기";
        }

        if (chute != null && chuteTrigger != null && chuteTrigger.PlayerInside)
        {
            target = chute;
            action = GameLanguage.IsEnglish ? "OPEN DELIVERY CHUTE" : "물건 투입구 열기";
        }

        if (workbench != null && workbenchTrigger != null && workbenchTrigger.PlayerInside)
        {
            target = workbench;
            action = GameLanguage.IsEnglish ? "OPEN WORKBENCH" : "작업대 열기";
        }

        return target;
    }

    private bool IsActiveTriggerInside()
    {
        WorldInteractionTrigger trigger = activeTerminalTarget == computer ? computerTrigger
            : activeTerminalTarget == shop ? shopTrigger
            : activeTerminalTarget == chute ? chuteTrigger
            : activeTerminalTarget == workbench ? workbenchTrigger
            : null;
        if (trigger == null || !trigger.PlayerInside) return false;
        return player == null || Vector3.Distance(player.position, trigger.transform.position) <= 4f;
    }

    private void BuildUi()
    {
        GameObject canvasObject = new("Gameplay HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        hudRoot = canvasObject;
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = .5f;

        prompt = Text("Prompt", canvasObject.transform, new Vector2(0, -420), new Vector2(560, 46), 22, Color.white);
        prompt.alignment = TextAlignmentOptions.Center;

        BuildStatusHud(canvasObject.transform);

        GameObject bar = ImageObject("Inventory Bar", canvasObject.transform, new Color(.03f, .025f, .02f, .88f));
        SetRoundedSprite(bar.GetComponent<Image>(), ref roundedPanelSprite, 12, 12);
        SetRect(bar.GetComponent<RectTransform>(), new Vector2(0, -485), new Vector2(820, 92));

        for (int index = 0; index < MaxSlots; index++)
        {
            bool unlocked = index < slotCapacity;
            GameObject slot = ImageObject("Slot " + (index + 1), bar.transform,
                unlocked ? new Color(.16f, .13f, .11f, 1) : new Color(.06f, .05f, .045f, 1));
            SetRoundedSprite(slot.GetComponent<Image>(), ref roundedSlotSprite, 15, 15);
            SetRect(slot.GetComponent<RectTransform>(), new Vector2(-342 + index * 76, 0), new Vector2(64, 64));
            slotFrames[index] = slot.GetComponent<Image>();
            Text((index + 1).ToString(), slot.transform, new Vector2(-22, 20), new Vector2(22, 20), 14, new Color(.7f, .65f, .58f, 1));
            slotLabels[index] = Text(unlocked ? "-" : "LOCK", slot.transform, new Vector2(0, -4), new Vector2(54, 24), unlocked ? 16 : 11, new Color(.9f, .87f, .8f, .7f));
        }
    }

    private void BuildStatusHud(Transform parent)
    {
        GameObject status = ImageObject("Status HUD", parent, new Color(.025f, .022f, .018f, .92f));
        SetRect(status.GetComponent<RectTransform>(), new Vector2(-680, 405), new Vector2(500, 190));

        GameObject accent = ImageObject("Status Accent", status.transform, new Color(.84f, .08f, .06f, 1));
        SetRect(accent.GetComponent<RectTransform>(), new Vector2(-242, 0), new Vector2(8, 190));

        dayLabel = Text("Day", status.transform, new Vector2(-155, 62), new Vector2(130, 38), 28, new Color(.94f, .92f, .86f, 1));
        dayLabel.alignment = TextAlignmentOptions.Left;
        levelLabel = Text("Level", status.transform, new Vector2(-28, 62), new Vector2(110, 32), 20, new Color(.42f, .82f, .55f, 1));
        levelLabel.alignment = TextAlignmentOptions.Left;
        timeLabel = Text("Time", status.transform, new Vector2(125, 62), new Vector2(190, 38), 21, new Color(.84f, .08f, .06f, 1));
        timeLabel.alignment = TextAlignmentOptions.Right;
        laborLabel = Text("Labor", status.transform, new Vector2(0, 13), new Vector2(420, 31), 20, new Color(.94f, .92f, .86f, 1));
        laborLabel.alignment = TextAlignmentOptions.Left;
        paymentLabel = Text("Payment", status.transform, new Vector2(0, -28), new Vector2(420, 31), 20, new Color(.94f, .92f, .86f, 1));
        paymentLabel.alignment = TextAlignmentOptions.Left;
        freedomLabel = Text("Freedom", status.transform, new Vector2(0, -69), new Vector2(420, 31), 20, new Color(.94f, .92f, .86f, 1));
        freedomLabel.alignment = TextAlignmentOptions.Left;
        RefreshStatusHud();
    }

    private void RefreshStatusHud()
    {
        if (dayLabel == null) return;
        float seconds = Mathf.Max(0f, GameDayClock.SecondsUntilMidnight);
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60f);
        dayLabel.text = $"DAY {GameSaveService.Day:00}";
        levelLabel.text = $"LV {CardProgressionService.Level:00}";
        timeLabel.text = GameLanguage.IsEnglish ? $"UNTIL MIDNIGHT   {minutes:00}:{remainingSeconds:00}" : $"자정까지 {minutes:00}:{remainingSeconds:00}";
        laborLabel.text = GameLanguage.IsEnglish ? $"LABOR BALANCE   {GameSaveService.Labor:N0}" : $"보유 노동값   {GameSaveService.Labor:N0}";
        paymentLabel.text = GameLanguage.IsEnglish ? $"TODAY'S PAYMENT   {GameDayClock.DailyLaborPayment:N0}" : $"오늘 납부액   {GameDayClock.DailyLaborPayment:N0}";
        freedomLabel.text = GameLanguage.IsEnglish ? $"FREEDOM FUND   {GameSaveService.FreedomFund:N0} / {GameEconomy.FreedomGoal:N0}" : $"자유 기금     {GameSaveService.FreedomFund:N0} / {GameEconomy.FreedomGoal:N0}";
    }

    private void SelectSlot(int index)
    {
        selectedSlot = index;
        for (int i = 0; i < slotFrames.Length; i++)
        {
            if (slotFrames[i] != null && i < slotCapacity)
                slotFrames[i].color = i == index ? new Color(.75f, .08f, .06f, 1) : new Color(.16f, .13f, .11f, 1);
        }
    }

    private void OpenTerminal(string title, Transform target)
    {
        if (IsTerminalOpen && terminalPanel == null && developerPanel == null)
            IsTerminalOpen = false;
        if (terminalPanel != null || developerPanel != null || IsTerminalOpen || DailyStoryController.IsPlaying || StoryIntroController.IsPlaying)
            return;
        activeTerminalTarget = target;
        IsTerminalOpen = true;
        // 창을 만드는 도중 상황별 튜토리얼이 시작될 수 있으므로 먼저 커서를 해제한다.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // The displayed prompt is localized, so it must never be used to decide
        // which world object was opened. In English "OPEN COMPUTER" did not
        // contain the Korean discriminator and incorrectly fell through to Shop.
        if (target == computer) BuildComputerWindow();
        else if (target == chute) BuildChuteWindow();
        else if (target == workbench) BuildWorkbenchWindow();
        else BuildShopWindow();
        if (terminalPanel != null) StartCoroutine(UiOpenAnimator.Play(terminalPanel, target == computer));
    }

    public void CloseForSystemTransition()
    {
        if (pausePanel != null) ClosePauseMenu();
        if (developerPanel != null) CloseDeveloperPanel();
        if (terminalPanel != null) CloseTerminal();
        if (prompt != null) prompt.gameObject.SetActive(false);
    }

    private void OpenPauseMenu()
    {
        if (pausePanel != null || terminalPanel != null || developerPanel != null || hudRoot == null) return;

        pauseRestoreTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;
        IsTerminalOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pausePanel = ImageObject("Pause Menu Overlay", hudRoot.transform, new Color(0f, 0f, 0f, .72f));
        Stretch(pausePanel.GetComponent<RectTransform>());

        GameObject window = CreateRoundedPanel("Pause Menu", pausePanel.transform, Vector2.zero, new Vector2(560, 610), new Color(.055f, .052f, .05f, .99f), 16);
        Shadow shadow = window.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, .75f);
        shadow.effectDistance = new Vector2(12, -12);

        GameObject header = CreateRoundedPanel("Pause Header", window.transform, new Vector2(0, 248), new Vector2(520, 86), new Color(.48f, .055f, .035f, 1f), 12);
        Text(GameLanguage.Text("pause"), header.transform, new Vector2(0, 8), new Vector2(440, 38), 31, Color.white);
        Text("DEBT PIT  //  SYSTEM MENU", header.transform, new Vector2(0, -23), new Vector2(440, 22), 13, new Color(1f, 1f, 1f, .62f));

        Text(GameLanguage.IsEnglish
                ? $"DAY {GameSaveService.Day:00}   ·   LABOR {GameSaveService.Labor:N0}"
                : $"DAY {GameSaveService.Day:00}   ·   보유 노동값 {GameSaveService.Labor:N0}",
            window.transform, new Vector2(0, 152), new Vector2(460, 32), 18, new Color(.72f, .7f, .66f, 1f));

        Button resume = CreateActionButton(GameLanguage.Text("resume"), window.transform, new Vector2(0, 66), new Vector2(390, 64), new Color(.45f, .09f, .055f, 1f));
        resume.onClick.AddListener(ClosePauseMenu);
        Button settings = CreateActionButton(GameLanguage.Text("settings"), window.transform, new Vector2(0, -18), new Vector2(390, 64), new Color(.18f, .17f, .155f, 1f));
        settings.onClick.AddListener(OpenPauseSettings);
        Button lobby = CreateActionButton(GameLanguage.Text("return_lobby"), window.transform, new Vector2(0, -102), new Vector2(390, 64), new Color(.28f, .075f, .06f, 1f));
        lobby.onClick.AddListener(ReturnToLobbyFromPause);

        Text(GameLanguage.IsEnglish ? "ESC  RESUME" : "ESC  계속하기", window.transform, new Vector2(0, -222), new Vector2(400, 24), 14, new Color(.48f, .47f, .44f, 1f));
        StartCoroutine(UiOpenAnimator.Play(window));
    }

    private void HandlePauseMenuInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        TitleSettingsPanel settings = FindFirstObjectByType<TitleSettingsPanel>();
        if (settings != null)
        {
            GameSettings.Save();
            SoundManager.Instance?.PlayWindowClose();
            Destroy(settings.gameObject);
            return;
        }

        ClosePauseMenu();
    }

    private void OpenPauseSettings()
    {
        if (hudRoot == null || FindFirstObjectByType<TitleSettingsPanel>() != null) return;
        TitleSettingsPanel.Show(hudRoot.GetComponent<Canvas>(), TMP_Settings.defaultFontAsset);
    }

    private void ClosePauseMenu()
    {
        TitleSettingsPanel settings = FindFirstObjectByType<TitleSettingsPanel>();
        if (settings != null) Destroy(settings.gameObject);
        if (pausePanel != null) Destroy(pausePanel);
        pausePanel = null;
        Time.timeScale = Mathf.Max(.01f, pauseRestoreTimeScale);
        IsTerminalOpen = terminalPanel != null || developerPanel != null;
        if (!IsTerminalOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void ReturnToLobbyFromPause()
    {
        GameSettings.Save();
        Time.timeScale = 1f;
        IsTerminalOpen = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneFade.Load("Title");
    }

    private void RefreshInventoryUi()
    {
        for (int index = 0; index < slotCapacity; index++)
        {
            if (slotLabels[index] == null) continue;
            string item = ItemInventoryService.GetItem(index);
            slotLabels[index].text = string.IsNullOrEmpty(item) ? "-" : GameLanguage.Item(item);
            slotLabels[index].fontSize = string.IsNullOrEmpty(item) ? 16 : 10;
        }
    }

    private void BuildComputerWindow()
    {
        terminalPanel = ImageObject("DEBT PIT Desktop", transform.GetComponentInChildren<Canvas>().transform, new Color(0f, .5f, .5f, 1));
        Stretch(terminalPanel.GetComponent<RectTransform>());

        CreateDesktopIcon(L("내 컴퓨터", "MY COMPUTER"), terminalPanel.transform, new Vector2(-835, 360), "PC", () => OpenAppWindow("내 컴퓨터"));
        CreateDesktopIcon(L("채무 계정", "DEBT ACCOUNT"), terminalPanel.transform, new Vector2(-835, 225), "AC", () => OpenAppWindow("채무 계정"));
        CreateDesktopIcon(L("물건 투입구", "DELIVERY CHUTE"), terminalPanel.transform, new Vector2(-835, 90), "IN", () => OpenAppWindow("물건 투입구"));
        CreateDesktopIcon(L("도박 앱", "RISK GAME"), terminalPanel.transform, new Vector2(-835, -45), "GM", () => OpenAppWindow("위험 게임"));
        CreateDesktopIcon(L("도움 앱", "HELP"), terminalPanel.transform, new Vector2(-835, -180), "?", () => OpenAppWindow("도움말"));
        CreateDesktopIcon(L("데일리 보상", "DAILY REWARD"), terminalPanel.transform, new Vector2(-690, 360), "DR", () => OpenAppWindow("데일리 보상"));
        CreateDesktopIcon(L("카드 상점", "CARD SHOP"), terminalPanel.transform, new Vector2(-690, 225), "CS", () => OpenAppWindow("카드 상점"));
        CreateDesktopIcon(L("업그레이드", "UPGRADES"), terminalPanel.transform, new Vector2(-690, 90), "UP", () => OpenAppWindow("업그레이드 상점"));
        CreateDesktopIcon(L("도구 상점", "TOOL SHOP"), terminalPanel.transform, new Vector2(-690, -45), "TL", () => OpenAppWindow("도구 상점"));
        CreateDesktopIcon(L("하루 종료", "END DAY"), terminalPanel.transform, new Vector2(-690, -180), "SK", () => OpenAppWindow("하루 넘기기"));
        CreateDesktopIcon(L("라디오", "RADIO"), terminalPanel.transform, new Vector2(-545, 360), "RD", () => OpenAppWindow("라디오"));

        GameObject window = ImageObject("Prisoner Terminal Window", terminalPanel.transform, new Color(.75f, .75f, .75f, 1));
        SetRect(window.GetComponent<RectTransform>(), new Vector2(0, 25), new Vector2(1110, 680));
        Outline outerOutline = window.AddComponent<Outline>();
        outerOutline.effectColor = new Color(.05f, .05f, .05f, 1);
        outerOutline.effectDistance = new Vector2(3, -3);

        GameObject titleBar = ImageObject("Title Bar", window.transform, new Color(0f, .08f, .48f, 1));
        SetRect(titleBar.GetComponent<RectTransform>(), new Vector2(0, 310), new Vector2(1080, 42));
        WindowDragHandler dragHandler = titleBar.AddComponent<WindowDragHandler>();
        dragHandler.Configure(window.GetComponent<RectTransform>(), terminalPanel.GetComponentInParent<Canvas>());
        Text("[PC]  DEBT PIT OS - PRISONER TERMINAL", titleBar.transform, new Vector2(-12, 0), new Vector2(900, 34), 20, Color.white).alignment = TextAlignmentOptions.Left;
        Button close = CreateWindowButton("X", titleBar.transform, new Vector2(505, 0), new Vector2(31, 28));
        close.onClick.AddListener(() => window.SetActive(false));

        GameObject menu = ImageObject("Menu", window.transform, new Color(.75f, .75f, .75f, 1));
        SetRect(menu.GetComponent<RectTransform>(), new Vector2(0, 275), new Vector2(1080, 27));
        Text(L("파일(F)     편집(E)     계정(A)     결제(P)     도움말(H)", "FILE(F)     EDIT(E)     ACCOUNT(A)     PAYMENT(P)     HELP(H)"), menu.transform, new Vector2(-170, 0), new Vector2(700, 24), 16, Color.black).alignment = TextAlignmentOptions.Left;

        GameObject body = ImageObject("Window Body", window.transform, new Color(.75f, .75f, .75f, 1));
        SetRect(body.GetComponent<RectTransform>(), new Vector2(0, -12), new Vector2(1080, 542));
        GameObject content = ImageObject("System Properties", body.transform, Color.white);
        SetRect(content.GetComponent<RectTransform>(), new Vector2(0, 28), new Vector2(960, 400));
        Outline insetOutline = content.AddComponent<Outline>();
        insetOutline.effectColor = new Color(.25f, .25f, .25f, 1);
        insetOutline.effectDistance = new Vector2(2, -2);
        terminalContent = content.transform;
        CreateComputerNavButton("일일 납부", body.transform, new Vector2(-320, -215), () => OpenAppWindow("일일 납부"));
        CreateComputerNavButton("자유 기금", body.transform, new Vector2(-105, -215), () => OpenAppWindow("자유 기금"));
        CreateComputerNavButton("데일리 상품", body.transform, new Vector2(110, -215), () => OpenAppWindow("데일리 상품"));
        CreateComputerNavButton("위험 게임", body.transform, new Vector2(325, -215), () => OpenAppWindow("위험 게임"));
        ShowComputerPage("현황");

        GameObject taskbar = ImageObject("Taskbar", terminalPanel.transform, new Color(.75f, .75f, .75f, 1));
        SetRect(taskbar.GetComponent<RectTransform>(), new Vector2(0, -512), new Vector2(1920, 56));
        Button start = CreateWindowButton("컴퓨터 끄기", taskbar.transform, new Vector2(-830, 0), new Vector2(175, 42));
        start.onClick.AddListener(CloseTerminal);
        Button terminalTask = CreateWindowButton("[PC]  Prisoner Terminal", taskbar.transform, new Vector2(-610, 0), new Vector2(250, 40));
        terminalTask.onClick.AddListener(() => { window.SetActive(true); window.transform.SetAsLastSibling(); });
        Text("SOUND   DAY " + GameSaveService.Day.ToString("00"), taskbar.transform, new Vector2(805, 0), new Vector2(190, 34), 16, Color.black);
        GameplayTutorialController.ShowContext("computer");
    }

    private void CreateDesktopIcon(string label, Transform parent, Vector2 position, string symbol, UnityEngine.Events.UnityAction callback)
    {
        GameObject icon = new(label, typeof(RectTransform), typeof(Image), typeof(Button));
        icon.transform.SetParent(parent, false);
        SetRect(icon.GetComponent<RectTransform>(), position, new Vector2(120, 112));
        icon.GetComponent<Image>().color = Color.clear;
        icon.GetComponent<Button>().onClick.AddListener(callback);
        GameObject picture = ImageObject("Icon", icon.transform, new Color(.75f, .75f, .75f, 1));
        picture.GetComponent<Image>().raycastTarget = false;
        SetRect(picture.GetComponent<RectTransform>(), new Vector2(0, 22), new Vector2(48, 48));
        Text(symbol, picture.transform, Vector2.zero, new Vector2(42, 42), 28, new Color(0f, .08f, .48f, 1));
        Text(label, icon.transform, new Vector2(0, -40), new Vector2(116, 38), 16, Color.white);
    }

    private void OpenAppWindow(string app)
    {
        Vector2 size = app == "위험 게임" ? new Vector2(720, 510)
            : app == "업그레이드 상점" ? new Vector2(820, 680)
            : app == "카드 상점" ? new Vector2(960, 720)
            : app == "도구 상점" ? new Vector2(960, 720)
            : new Vector2(760, 500);
        Vector2 position = app == "도움말" ? new Vector2(120, -25) : new Vector2(90, 5);
        Color frame = app == "위험 게임" ? new Color(.12f, .025f, .025f, 1) : new Color(.75f, .75f, .75f, 1);
        Color titleColor = app == "위험 게임" ? new Color(.42f, .02f, .02f, 1) : app == "물건 투입구" ? new Color(.08f, .20f, .32f, 1) : new Color(0f, .08f, .48f, 1);

        GameObject window = ImageObject(app + " Window", terminalPanel.transform, frame);
        SetRect(window.GetComponent<RectTransform>(), position, size);
        Outline outline = window.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(3, -3);

        GameObject titleBar = ImageObject("Title Bar", window.transform, titleColor);
        SetRect(titleBar.GetComponent<RectTransform>(), new Vector2(0, size.y * .5f - 25), new Vector2(size.x - 28, 38));
        WindowDragHandler drag = titleBar.AddComponent<WindowDragHandler>();
        drag.Configure(window.GetComponent<RectTransform>(), terminalPanel.GetComponentInParent<Canvas>());
        Text(AppTitle(app), titleBar.transform, new Vector2(-8, 0), new Vector2(size.x - 100, 30), 19, Color.white).alignment = TextAlignmentOptions.Left;
        Button close = CreateWindowButton("X", titleBar.transform, new Vector2(size.x * .5f - 38, 0), new Vector2(28, 26));
        close.onClick.AddListener(() => Destroy(window));

        GameObject page = ImageObject("Page", window.transform, app == "위험 게임" ? new Color(.025f, .012f, .012f, 1) : Color.white);
        SetRect(page.GetComponent<RectTransform>(), new Vector2(0, -10), new Vector2(size.x - 58, size.y - 94));
        Outline pageOutline = page.AddComponent<Outline>();
        pageOutline.effectColor = app == "위험 게임" ? new Color(.75f, .08f, .06f, 1) : new Color(.28f, .28f, .28f, 1);
        pageOutline.effectDistance = new Vector2(2, -2);
        BuildAppPage(app, page.transform, size);
        StartCoroutine(UiOpenAnimator.Play(window));
    }

    private void BuildAppPage(string app, Transform page, Vector2 windowSize)
    {
        Color ink = app == "위험 게임" ? new Color(1f, .22f, .18f, 1) : Color.black;
        if (app == "라디오")
        {
            BuildRadioPage(page);
            return;
        }
        if (app == "물건 투입구")
        {
            Text("DELIVERY CHUTE MONITOR", page, new Vector2(0, 135), new Vector2(570, 34), 25, new Color(.05f, .19f, .34f, 1));
            Text(L("수령 대기 물품", "PENDING DELIVERIES"), page, new Vector2(-205, 72), new Vector2(260, 28), 20, Color.black).alignment = TextAlignmentOptions.Left;
            GameObject status = ImageObject("Delivery Status", page, new Color(.84f, .9f, .92f, 1));
            SetRect(status.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(570, 104));
            string deliveryStatus = ItemInventoryService.DeliveryCount == 0
                ? L("대기 중인 배송이 없습니다.\n데일리 상품과 구매 물품은 이곳으로 도착합니다.", "NO DELIVERIES ARE PENDING.\nDAILY AND PURCHASED ITEMS ARRIVE HERE.")
                : L($"현재 {ItemInventoryService.DeliveryCount}개의 물품이 도착해 있습니다.\n복도 끝 물건 투입구에서 직접 수령하십시오.", $"{ItemInventoryService.DeliveryCount} ITEMS HAVE ARRIVED.\nCOLLECT THEM AT THE CHUTE IN THE CORRIDOR.");
            Text(deliveryStatus, status.transform, Vector2.zero, new Vector2(520, 80), 19, new Color(.05f, .12f, .18f, 1));
            CreateWindowButton(L("수령함 확인", "VIEW DELIVERY QUEUE"), page, new Vector2(175, -135), new Vector2(190, 40));
            return;
        }

        if (app == "위험 게임")
        {
            int bet = CurrentRiskBet;
            Text("DEBT PIT // RISK GAME", page, new Vector2(0, 142), new Vector2(560, 36), 28, ink);
            Text(GameLanguage.Runtime(riskMessage), page, new Vector2(0, 85), new Vector2(560, 30), 20, new Color(.9f, .8f, .78f, 1));
            Text(L($"베팅 금액  {bet:N0} 노동값", $"BET  {bet:N0} LABOR"), page, new Vector2(0, 28), new Vector2(360, 34), 22, Color.white);
            CreateWindowButton("-", page, new Vector2(-225, 28), new Vector2(54, 42)).onClick.AddListener(() => AdjustRiskBet(-1, page.parent.gameObject));
            CreateWindowButton("+", page, new Vector2(225, 28), new Vector2(54, 42)).onClick.AddListener(() => AdjustRiskBet(1, page.parent.gameObject));
            Text(L($"당첨 배율  x{UpgradeService.RiskPayoutMultiplier:0.0}", $"WIN MULTIPLIER  x{UpgradeService.RiskPayoutMultiplier:0.0}"), page, new Vector2(0, -12), new Vector2(360, 24), 16, new Color(.82f, .52f, .48f, 1));
            Button play = CreateWindowButton(L("게임 시작", "START GAME"), page, new Vector2(0, -72), new Vector2(190, 52));
            play.image.color = new Color(.75f, .08f, .06f, 1);
            play.onClick.AddListener(() => PlayRiskGame(page.parent.gameObject));
            Text(L("주의: 손실은 복구할 수 없습니다.", "WARNING: LOSSES CANNOT BE RECOVERED."), page, new Vector2(0, -145), new Vector2(520, 30), 17, new Color(.9f, .4f, .35f, 1));
            return;
        }

        if (app == "도움말")
        {
            Text(L("도움말", "HELP"), page, new Vector2(-100, 140), new Vector2(450, 36), 27, new Color(0f, .08f, .48f, 1)).alignment = TextAlignmentOptions.Left;
            Text(L("DEBT PIT 생활 안내", "DEBT PIT SURVIVAL GUIDE"), page, new Vector2(-100, 88), new Vector2(450, 30), 21, Color.black).alignment = TextAlignmentOptions.Left;
            Text(L("1. 자정 전까지 컴퓨터에서 일일 노동값을 납부합니다.\n2. 구매 물품은 물건 투입구에서 수령합니다.\n3. 작업대에서 카드팩과 봉인 상자를 개봉합니다.\n4. 잉여 노동값은 자유 기금에 납부할 수 있습니다.", "1. PAY THE DAILY LABOR CHARGE BEFORE MIDNIGHT.\n2. COLLECT PURCHASES FROM THE DELIVERY CHUTE.\n3. OPEN CARD PACKS AND SEALED BOXES AT THE WORKBENCH.\n4. DEPOSIT SURPLUS LABOR INTO THE FREEDOM FUND."), page, new Vector2(-100, 2), new Vector2(540, 145), 18, Color.black).alignment = TextAlignmentOptions.TopLeft;
            Button replay = CreateWindowButton(L("기본 튜토리얼 다시 보기", "REPLAY TUTORIAL"), page, new Vector2(-55, -150), new Vector2(250, 38));
            replay.onClick.AddListener(() =>
            {
                Destroy(page.parent.gameObject);
                GameplayTutorialController.ShowMainTutorial();
            });
            CreateWindowButton("닫기", page, new Vector2(215, -150), new Vector2(110, 38)).onClick.AddListener(() => Destroy(page.parent.gameObject));
            return;
        }

        if (app == "자유 기금")
        {
            int remainingFund = Mathf.Max(0, GameEconomy.FreedomGoal - GameSaveService.FreedomFund);
            int maxPayment = Mathf.Min(GameSaveService.Labor, remainingFund);
            Text("FREEDOM FUND", page, new Vector2(0, 142), new Vector2(560, 38), 28, new Color(0f, .08f, .48f, 1));
            Text(L($"현재 적립금  {GameSaveService.FreedomFund:N0} / {GameEconomy.FreedomGoal:N0}", $"CURRENT FUND  {GameSaveService.FreedomFund:N0} / {GameEconomy.FreedomGoal:N0}"), page, new Vector2(0, 99), new Vector2(560, 30), 21, Color.black);
            GameObject track = ImageObject("Fund Progress", page, new Color(.24f, .24f, .24f, 1));
            SetRect(track.GetComponent<RectTransform>(), new Vector2(0, 58), new Vector2(560, 26));
            float ratio = Mathf.Clamp01((float)GameSaveService.FreedomFund / GameEconomy.FreedomGoal);
            GameObject fill = ImageObject("Fund Fill", track.transform, new Color(0f, .08f, .48f, 1));
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, .5f);
            fillRect.anchorMax = new Vector2(0, .5f);
            fillRect.pivot = new Vector2(0, .5f);
            fillRect.sizeDelta = new Vector2(ratio > 0f ? Mathf.Max(4f, 552 * ratio) : 0f, 18);
            fillRect.anchoredPosition = new Vector2(4, 0);
            Text(L($"보유 노동값  {GameSaveService.Labor:N0}     ·     남은 목표  {remainingFund:N0}", $"LABOR BALANCE  {GameSaveService.Labor:N0}     ·     REMAINING  {remainingFund:N0}"), page, new Vector2(0, 22), new Vector2(560, 26), 17, Color.black);

            TMP_InputField amountInput = CreateNumericInput(L("납부 금액", "PAYMENT AMOUNT"), page, new Vector2(0, -25), new Vector2(350, 42));
            CreateWindowButton("25%", page, new Vector2(-170, -76), new Vector2(96, 34)).onClick.AddListener(() => SetFundInputFraction(amountInput, maxPayment, .25f));
            CreateWindowButton("50%", page, new Vector2(-57, -76), new Vector2(96, 34)).onClick.AddListener(() => SetFundInputFraction(amountInput, maxPayment, .5f));
            CreateWindowButton(L("전액", "MAX"), page, new Vector2(57, -76), new Vector2(96, 34)).onClick.AddListener(() => amountInput.text = maxPayment.ToString());
            CreateWindowButton(L("초기화", "CLEAR"), page, new Vector2(170, -76), new Vector2(96, 34)).onClick.AddListener(() => amountInput.text = string.Empty);

            Button pay = CreateWindowButton(L("입력 금액 납부", "SUBMIT PAYMENT"), page, new Vector2(0, -132), new Vector2(210, 43));
            pay.onClick.AddListener(() => PayFreedomFundFromInput(amountInput.text, page.parent.gameObject));
            return;
        }

        if (app == "일일 납부")
        {
            Text("DAILY PAYMENT NOTICE", page, new Vector2(0, 140), new Vector2(560, 36), 27, new Color(.42f, .02f, .02f, 1));
            Text(L("오늘의 생존 청구서", "TODAY'S SURVIVAL INVOICE"), page, new Vector2(-185, 80), new Vector2(290, 28), 20, Color.black).alignment = TextAlignmentOptions.Left;
            string invoice = GameLanguage.IsEnglish
                ? $"AMOUNT DUE                         {GameDayClock.DailyLaborPayment:N0}\nLABOR BALANCE                      {GameSaveService.Labor:N0}\nSTATUS                             {(GameSaveService.DailyPaymentPaid ? "PAID" : "UNPAID")}"
                : $"청구 금액                         {GameDayClock.DailyLaborPayment:N0}\n보유 노동값                       {GameSaveService.Labor:N0}\n상태                              {(GameSaveService.DailyPaymentPaid ? "납부 완료" : "미납")}";
            Text(invoice, page, new Vector2(0, 5), new Vector2(540, 110), 20, Color.black).alignment = TextAlignmentOptions.TopLeft;
            Button pay = CreateWindowButton(GameSaveService.DailyPaymentPaid ? L("납부 완료", "PAID") : L("납부 실행", "PAY NOW"), page, new Vector2(180, -132), new Vector2(150, 42));
            pay.onClick.AddListener(() => PayDailyLaborInApp(page.parent.gameObject));
            return;
        }

        if (app == "데일리 상품")
        {
            Text("DAILY DELIVERY", page, new Vector2(0, 140), new Vector2(560, 36), 27, new Color(.05f, .19f, .34f, 1));
            Text(L("오늘의 공급품", "TODAY'S SUPPLIES"), page, new Vector2(-205, 82), new Vector2(260, 28), 20, Color.black).alignment = TextAlignmentOptions.Left;
            for (int i = 0; i < 3; i++)
            {
                GameObject card = ImageObject("Daily Item", page, new Color(.84f, .9f, .92f, 1));
                SetRect(card.GetComponent<RectTransform>(), new Vector2(-190 + i * 190, -2), new Vector2(160, 115));
                Text(i == 0 ? "작업 장갑" : i == 1 ? "캔 식량" : "잠금 상자", card.transform, new Vector2(0, 20), new Vector2(145, 26), 18, new Color(.05f, .12f, .18f, 1));
                string itemName = i == 0 ? "작업 장갑" : i == 1 ? "캔 식량" : "잠금 상자";
                int price = ItemInventoryService.GetValue(itemName);
                Button buy = CreateWindowButton(L($"{price:N0} 구매", $"BUY  {price:N0}"), card.transform, new Vector2(0, -27), new Vector2(120, 30));
                buy.onClick.AddListener(() => BuyComputerItem(itemName, price, page.parent.gameObject));
            }
            Text(L("구매한 물품은 물건 투입구에 도착합니다.", "PURCHASED ITEMS ARE SENT TO THE DELIVERY CHUTE."), page, new Vector2(0, -145), new Vector2(560, 26), 17, Color.black);
            return;
        }

        if (app == "데일리 보상")
        {
            Text("DAILY SUPPLY", page, new Vector2(0, 140), new Vector2(560, 36), 27, new Color(0f, .08f, .48f, 1));
            Text(L($"DAY {GameSaveService.Day:00} 무료 보급", $"DAY {GameSaveService.Day:00} FREE SUPPLY"), page, new Vector2(0, 78), new Vector2(520, 30), 21, Color.black);
            GameObject reward = ImageObject("Reward", page, new Color(.88f, .91f, .95f, 1));
            SetRect(reward.GetComponent<RectTransform>(), new Vector2(0, 5), new Vector2(500, 100));
            Text(L("무료 카드팩  x1\n작업대에서 개봉할 수 있습니다.", "FREE CARD PACK  x1\nOPEN IT AT THE WORKBENCH."), reward.transform, Vector2.zero, new Vector2(450, 70), 19, new Color(.04f, .12f, .25f, 1));
            Button claim = CreateWindowButton(GameSaveService.DailyRewardClaimed ? L("오늘 수령 완료", "CLAIMED TODAY") : L("투입구로 보내기", "SEND TO CHUTE"), page, new Vector2(0, -115), new Vector2(190, 44));
            claim.interactable = !GameSaveService.DailyRewardClaimed;
            claim.onClick.AddListener(() => ClaimDailyReward(page.parent.gameObject));
            return;
        }

        if (app == "카드 상점")
        {
            BuildCardPackShop(page, page.parent.gameObject);
            return;
        }

        if (app == "도구 상점")
        {
            Text("FACILITY TOOL STORE", page, new Vector2(0, 260), new Vector2(700, 36), 27, new Color(.16f, .27f, .18f, 1));
            Text(L("상자와 카드팩 보안 규격에 맞는 도구를 준비하십시오.", "EQUIP TOOLS MATCHED TO BOX AND CARD-PACK SECURITY."), page, new Vector2(0, 225), new Vector2(720, 26), 16, Color.black);
            CreateToolStoreCard("락핀", L("타이밍 링\n정밀 해제", "TIMING RING\nPRECISION ENTRY"), 3, new Vector2(-225, 100), page, page.parent.gameObject);
            CreateToolStoreCard("휴대용 드릴", L("과열 관리\n고속 해제", "HEAT CONTROL\nFAST ENTRY"), 14, new Vector2(0, 100), page, page.parent.gameObject);
            CreateToolStoreCard("유압 절단기", L("즉시 절단\n확정 해제", "INSTANT CUT\nGUARANTEED ENTRY"), 35, new Vector2(225, 100), page, page.parent.gameObject);
            CreateToolStoreCard("미니 노트북", L("해킹 팩 전용\n재사용 가능", "HACKING PACKS\nREUSABLE"), 24, new Vector2(-225, -125), page, page.parent.gameObject);
            CreateToolStoreCard("신호 복호기", L("스캔 판정 폭\n보조 장비", "WIDER SCAN ZONE\nSUPPORT TOOL"), 45, new Vector2(0, -125), page, page.parent.gameObject);
            CreateToolStoreCard("냉각 스프레이", L("드릴 과열 시\n예비 소모품", "DRILL COOLANT\nCONSUMABLE"), 8, new Vector2(225, -125), page, page.parent.gameObject);
            return;
        }

        if (app == "하루 넘기기")
        {
            bool paid = GameSaveService.DailyPaymentPaid;
            Text("END OF DAY CONTROL", page, new Vector2(0, 140), new Vector2(600, 36), 27, new Color(.4f, .08f, .04f, 1));
            Text(L($"DAY {GameSaveService.Day:00}을 즉시 종료합니다.", $"END DAY {GameSaveService.Day:00} IMMEDIATELY."), page, new Vector2(0, 86), new Vector2(560, 30), 21, Color.black);
            GameObject warning = ImageObject("Skip Warning", page, paid ? new Color(.86f, .92f, .87f, 1) : new Color(.94f, .84f, .82f, 1));
            SetRect(warning.GetComponent<RectTransform>(), new Vector2(0, 12), new Vector2(540, 105));
            string endWarning = paid
                ? L("오늘의 노동값 납부 완료\n안전하게 다음 날로 이동할 수 있습니다.", "TODAY'S PAYMENT IS COMPLETE.\nYOU MAY SAFELY PROCEED TO THE NEXT DAY.")
                : L($"경고: 오늘의 노동값 {GameDayClock.DailyLaborPayment:N0} 미납\n지금 하루를 종료하면 처형 절차가 시작됩니다.", $"WARNING: {GameDayClock.DailyLaborPayment:N0} LABOR REMAINS UNPAID.\nENDING THE DAY WILL BEGIN EXECUTION PROCEDURES.");
            Text(endWarning, warning.transform, Vector2.zero, new Vector2(500, 76), 18, paid ? new Color(.05f, .28f, .12f, 1) : new Color(.52f, .05f, .03f, 1));
            Button endDay = CreateWindowButton(paid ? L("하루 종료", "END DAY") : L("미납 상태로 종료", "END DAY UNPAID"), page, new Vector2(0, -105), new Vector2(210, 46));
            endDay.onClick.AddListener(EndDayFromComputer);
            return;
        }

        if (app == "업그레이드 상점")
        {
            Text("SYSTEM UPGRADES", page, new Vector2(0, 250), new Vector2(650, 36), 27, new Color(.12f, .22f, .4f, 1));
            Text(L("영구 업그레이드 · 구매 즉시 적용", "PERMANENT UPGRADES · APPLIED IMMEDIATELY"), page, new Vector2(0, 219), new Vector2(650, 22), 15, new Color(.25f, .25f, .28f, 1));
            CreateUpgradeRow("inventory", L("인벤토리 확장", "INVENTORY EXPANSION"), L($"현재 {UpgradeService.InventoryCapacity}칸 · 최대 10칸", $"CURRENT {UpgradeService.InventoryCapacity} SLOTS · MAX 10"), new Vector2(0, 178), page);
            CreateUpgradeRow("chance", L("흥정 기술", "NEGOTIATION SKILL"), L($"성공률 {UpgradeService.HaggleChance * 100f:0}% · 최대 80%", $"SUCCESS {UpgradeService.HaggleChance * 100f:0}% · MAX 80%"), new Vector2(0, 120), page);
            CreateUpgradeRow("margin", L("협상 수익", "NEGOTIATION PROFIT"), L($"성공 시 {UpgradeService.HaggleMinIncrease * 100f:0}~{UpgradeService.HaggleMaxIncrease * 100f:0}% 상승", $"SUCCESS INCREASE {UpgradeService.HaggleMinIncrease * 100f:0}~{UpgradeService.HaggleMaxIncrease * 100f:0}%"), new Vector2(0, 62), page);
            CreateUpgradeRow("luck", L("카드팩 행운", "CARD-PACK LUCK"), L($"상위 카드 출현 보정 +{UpgradeService.PackLuckLevel * 8}%", $"HIGH-TIER CARD CHANCE +{UpgradeService.PackLuckLevel * 8}%"), new Vector2(0, 4), page);
            CreateUpgradeRow("risk", L("위험 보상", "RISK REWARD"), L($"당첨 배율 x{UpgradeService.RiskPayoutMultiplier:0.0}", $"WIN MULTIPLIER x{UpgradeService.RiskPayoutMultiplier:0.0}"), new Vector2(0, -54), page);
            CreateUpgradeRow("discount", L("도구 할인", "TOOL DISCOUNT"), L($"락핀 가격 {UpgradeService.ToolDiscount * 100f:0}% 할인", $"LOCKPICK PRICE -{UpgradeService.ToolDiscount * 100f:0}%"), new Vector2(0, -112), page);
            CreateUpgradeRow("skill", L("해제 숙련", "UNSEALING MASTERY"), L($"스킬 체크 성공 구간 {UpgradeService.SkillZoneWidth * 100f:0}%", $"SKILL-CHECK ZONE {UpgradeService.SkillZoneWidth * 100f:0}%"), new Vector2(0, -170), page);
            CreateUpgradeRow("experience", L("학습 속도", "LEARNING SPEED"), L($"경험치 획득 x{UpgradeService.ExperienceMultiplier:0.0}", $"EXP GAIN x{UpgradeService.ExperienceMultiplier:0.0}"), new Vector2(0, -228), page);
            return;
        }

        string body = app == "일일 납부"
            ? $"오늘의 생존 청구액: {GameDayClock.DailyLaborPayment:N0} 노동값\n보유 노동값: {GameSaveService.Labor:N0}"
            : app == "자유 기금"
                ? $"자유 기금: {GameSaveService.FreedomFund:N0} / {GameEconomy.FreedomGoal:N0}\n보유 노동값: {GameSaveService.Labor:N0}"
                : app == "채무 계정"
                    ? $"DAY {GameSaveService.Day:00}\n오늘의 청구액: {GameDayClock.DailyLaborPayment:N0}\n자유 기금: {GameSaveService.FreedomFund:N0} / {GameEconomy.FreedomGoal:N0}"
                    : app == "내 컴퓨터"
                        ? "시설 네트워크에 연결된 개인 단말기입니다.\n왼쪽의 앱 아이콘을 선택하십시오."
                        : "오늘의 공급품은 자정에 갱신됩니다.\n구매 물품은 물건 투입구로 배송됩니다.";
        Text(app, page, new Vector2(-100, 135), new Vector2(450, 36), 27, ink).alignment = TextAlignmentOptions.Left;
        Text(body, page, new Vector2(-100, 50), new Vector2(500, 150), 20, ink).alignment = TextAlignmentOptions.TopLeft;
    }

    private static string AppTitle(string app)
    {
        return app switch
        {
            "물건 투입구" => "[IN]  DELIVERY CHUTE",
            "위험 게임" => "[GM]  RISK GAME",
            "도움말" => "[?]  HELP",
            "채무 계정" => "[AC]  DEBT ACCOUNT",
            "내 컴퓨터" => "[PC]  MY COMPUTER",
            "데일리 보상" => "[DR]  DAILY SUPPLY",
            "카드 상점" => "[CS]  CARD SHOP",
            "업그레이드 상점" => "[UP]  UPGRADE SHOP",
            "도구 상점" => "[TL]  TOOL STORE",
            "하루 넘기기" => "[SK]  END DAY",
            "라디오" => "[RD]  FACILITY RADIO",
            _ => "[PC]  " + app
        };
    }

    private void BuildRadioPage(Transform page)
    {
        computerRadio ??= computer != null ? ComputerRadioPlayer.GetOrCreate(computer) : null;
        Text("FACILITY RADIO", page, new Vector2(0, 145), new Vector2(600, 38), 28, new Color(0f, .08f, .48f, 1));
        Text(L("컴퓨터에서 재생되는 시설 라디오입니다. 멀어질수록 소리가 작아집니다.", "SPATIAL FACILITY RADIO PLAYED FROM THIS COMPUTER. VOLUME FADES WITH DISTANCE."), page,
            new Vector2(0, 108), new Vector2(620, 26), 16, Color.black);

        TMP_InputField urlInput = CreateTextInput(L("MP3 / OGG / WAV 주소 또는 file:/// 로컬 경로", "MP3 / OGG / WAV URL OR file:/// LOCAL PATH"), page, new Vector2(0, 55), new Vector2(570, 42));
        if (computerRadio != null) urlInput.text = computerRadio.SavedUrl;
        TextMeshProUGUI status = Text(computerRadio != null ? GameLanguage.Runtime(computerRadio.Status) : L("컴퓨터 오디오 장치를 찾지 못했습니다.", "COMPUTER AUDIO DEVICE NOT FOUND."),
            page, new Vector2(0, 14), new Vector2(590, 24), 15,
            computerRadio != null && computerRadio.HasError ? new Color(.65f, .04f, .04f, 1) : new Color(.05f, .35f, .18f, 1));
        status.textWrappingMode = TextWrappingModes.NoWrap;
        TextMeshProUGUI positionLabel = Text(computerRadio != null ? computerRadio.PlaybackPosition : "00:00 / 00:00",
            page, new Vector2(0, -12), new Vector2(240, 24), 16, Color.black);
        StartCoroutine(UpdateRadioStatus(status, positionLabel));

        Button play = CreateWindowButton(L("재생", "PLAY"), page, new Vector2(-185, -45), new Vector2(110, 36));
        Button stop = CreateWindowButton(L("정지", "STOP"), page, new Vector2(-65, -45), new Vector2(110, 36));
        CreateWindowButton(L("-10초", "-10 SEC"), page, new Vector2(65, -45), new Vector2(110, 36)).onClick.AddListener(() => computerRadio?.Seek(-10f));
        CreateWindowButton(L("+10초", "+10 SEC"), page, new Vector2(185, -45), new Vector2(110, 36)).onClick.AddListener(() => computerRadio?.Seek(10f));
        play.onClick.AddListener(() =>
        {
            if (computerRadio == null) return;
            computerRadio.Play(urlInput.text);
        });
        stop.onClick.AddListener(() =>
        {
            computerRadio?.Stop();
        });

        float volume = computerRadio != null ? computerRadio.VolumePercent : 100f;
        Text(L($"라디오 볼륨  {volume:0}%", $"RADIO VOLUME  {volume:0}%"), page, new Vector2(0, -88), new Vector2(240, 28), 18, Color.black);
        CreateWindowButton("-500", page, new Vector2(-250, -88), new Vector2(68, 32)).onClick.AddListener(() => AdjustRadioVolume(-500, page.parent.gameObject));
        CreateWindowButton("-10", page, new Vector2(-175, -88), new Vector2(62, 32)).onClick.AddListener(() => AdjustRadioVolume(-10, page.parent.gameObject));
        CreateWindowButton("+10", page, new Vector2(175, -88), new Vector2(62, 32)).onClick.AddListener(() => AdjustRadioVolume(10, page.parent.gameObject));
        CreateWindowButton("+500", page, new Vector2(250, -88), new Vector2(68, 32)).onClick.AddListener(() => AdjustRadioVolume(500, page.parent.gameObject));

        Button loop = CreateWindowButton(computerRadio != null && computerRadio.LoopEnabled ? L("[X] 반복 재생", "[X] LOOP") : L("[ ] 반복 재생", "[ ] LOOP"),
            page, new Vector2(0, -123), new Vector2(180, 30));
        loop.onClick.AddListener(() =>
        {
            computerRadio?.ToggleLoop();
            RefreshRadioWindow(page.parent.gameObject);
        });

        Text(L("YouTube 주소는 공식 임베디드 플레이어가 필요합니다.\n직접 MP3/OGG/WAV 주소와 file:/// 로컬 파일을 재생할 수 있습니다.", "YOUTUBE URLS REQUIRE THE OFFICIAL EMBEDDED PLAYER.\nDIRECT MP3/OGG/WAV URLS AND file:/// LOCAL FILES ARE SUPPORTED."),
            page, new Vector2(0, -158), new Vector2(610, 34), 12, new Color(.32f, .32f, .32f, 1));
    }

    private void AdjustRadioVolume(int delta, GameObject window)
    {
        if (computerRadio == null) return;
        computerRadio.SetVolumePercent(computerRadio.VolumePercent + delta);
        RefreshRadioWindow(window);
    }

    private System.Collections.IEnumerator UpdateRadioStatus(TextMeshProUGUI label, TextMeshProUGUI positionLabel)
    {
        while (label != null && computerRadio != null)
        {
            label.text = GameLanguage.Runtime(computerRadio.Status);
            label.color = computerRadio.HasError
                ? new Color(.65f, .04f, .04f, 1)
                : new Color(.05f, .35f, .18f, 1);
            if (positionLabel != null) positionLabel.text = computerRadio.PlaybackPosition;
            yield return null;
        }
    }

    private void RefreshRadioWindow(GameObject window)
    {
        if (window == null) return;
        Destroy(window);
        OpenAppWindow("라디오");
    }

    private void ShowComputerPage(string page)
    {
        if (terminalContent == null) return;
        for (int i = terminalContent.childCount - 1; i >= 0; i--)
        {
            Transform child = terminalContent.GetChild(i);
            if (child.name != "Bevel") Destroy(child.gameObject);
        }

        if (page == "현황")
        {
            RenderComputerHome();
            return;
        }

        Text(page.ToUpperInvariant(), terminalContent, new Vector2(-130, 168), new Vector2(650, 34), 26, new Color(0f, .08f, .36f, 1)).alignment = TextAlignmentOptions.Left;
        string copy;
        if (page == "일일 납부")
        {
            copy = GameSaveService.DailyPaymentPaid
                ? $"오늘의 생존 청구액: 납부 완료\n보유 노동값: {GameSaveService.Labor:N0}\n\n다음 청구는 자정 이후에 갱신됩니다."
                : $"오늘의 생존 청구액: {GameDayClock.DailyLaborPayment:N0} 노동값\n보유 노동값: {GameSaveService.Labor:N0}\n\n자정 전까지 납부하지 않으면 처분 절차가 시작됩니다.";
            if (!GameSaveService.DailyPaymentPaid)
                CreateComputerAction("납부 실행", new Vector2(225, -155), () => PayDailyLabor());
        }
        else if (page == "자유 기금")
        {
            int payment = GameEconomy.Scale(10);
            copy = $"현재 자유 기금: {GameSaveService.FreedomFund:N0} / {GameEconomy.FreedomGoal:N0}\n보유 노동값: {GameSaveService.Labor:N0}\n\n기금은 환불되지 않으며, 목표 달성 후에만 출구가 열립니다.";
            CreateComputerAction($"{payment:N0} 납부", new Vector2(210, -155), () => PayFreedomFund());
        }
        else if (page == "데일리 상품")
        {
            copy = "오늘의 공급품은 물건 투입구로 배송됩니다.\n재고 갱신까지 남은 시간은 자정까지의 시간과 같습니다.\n\n[ 준비 중 ] 물건 목록을 불러오는 중입니다.";
        }
        else if (page == "위험 게임")
        {
            copy = "승률은 공개되지 않습니다.\n시설은 결과의 공정성을 보증하지 않습니다.\n\n[ 준비 중 ] 위험 게임 서버에 연결할 수 없습니다.";
        }
        else if (page == "내 컴퓨터")
        {
            copy = "내 컴퓨터\n\n시설 네트워크에 연결된 개인 단말기입니다.\n계정, 결제, 공급품 및 기록을 여기서 확인할 수 있습니다.";
        }
        else if (page == "채무 계정")
        {
            copy = $"채무 계정\n\n보유 노동값: {GameSaveService.Labor:N0}\n오늘의 생존 청구액: {GameDayClock.DailyLaborPayment:N0}\n자유 기금: {GameSaveService.FreedomFund:N0} / {GameEconomy.FreedomGoal:N0}";
        }
        else if (page == "물건 투입구")
        {
            copy = "물건 투입구\n\n구매한 물품과 매일의 공급품은 복도 끝 투입구로 배송됩니다.\n현재 수령 대기 물품: 0개";
        }
        else if (page == "도움말")
        {
            copy = "도움말\n\n1. 노동값을 모아 자정 전 일일 납부를 완료하십시오.\n2. 잉여 노동값은 자유 기금에 납부할 수 있습니다.\n3. 투입구에서 배송된 물건을 수령하십시오.";
        }
        else
        {
            copy = $"수용자 계정: 익명\nDAY {GameSaveService.Day:00}  |  자정까지 {FormatTime(GameDayClock.SecondsUntilMidnight)}\n\n오늘의 청구액을 확인하고, 필요한 업무를 선택하십시오.";
        }

        if (GameLanguage.IsEnglish)
        {
            copy = page switch
            {
                "일일 납부" => GameSaveService.DailyPaymentPaid
                    ? $"TODAY'S SURVIVAL CHARGE: PAID\nLABOR BALANCE: {GameSaveService.Labor:N0}\n\nTHE NEXT CHARGE IS ISSUED AFTER MIDNIGHT."
                    : $"TODAY'S SURVIVAL CHARGE: {GameDayClock.DailyLaborPayment:N0} LABOR\nLABOR BALANCE: {GameSaveService.Labor:N0}\n\nFAILURE TO PAY BEFORE MIDNIGHT BEGINS DISPOSAL PROCEDURES.",
                "자유 기금" => $"CURRENT FREEDOM FUND: {GameSaveService.FreedomFund:N0} / {GameEconomy.FreedomGoal:N0}\nLABOR BALANCE: {GameSaveService.Labor:N0}\n\nDEPOSITS ARE NON-REFUNDABLE. THE EXIT OPENS ONLY AFTER THE GOAL IS MET.",
                "데일리 상품" => "TODAY'S SUPPLIES ARE SENT TO THE DELIVERY CHUTE.\nSTOCK REFRESHES AT MIDNIGHT.\n\n[ STANDBY ] LOADING ITEM LIST.",
                "위험 게임" => "WIN ODDS ARE NOT DISCLOSED.\nTHE FACILITY DOES NOT GUARANTEE FAIR RESULTS.\n\n[ STANDBY ] RISK SERVER UNAVAILABLE.",
                "내 컴퓨터" => "MY COMPUTER\n\nA PERSONAL TERMINAL CONNECTED TO THE FACILITY NETWORK.\nACCESS ACCOUNTS, PAYMENTS, SUPPLIES, AND RECORDS HERE.",
                "채무 계정" => $"DEBT ACCOUNT\n\nLABOR BALANCE: {GameSaveService.Labor:N0}\nTODAY'S SURVIVAL CHARGE: {GameDayClock.DailyLaborPayment:N0}\nFREEDOM FUND: {GameSaveService.FreedomFund:N0} / {GameEconomy.FreedomGoal:N0}",
                "물건 투입구" => $"DELIVERY CHUTE\n\nPURCHASES AND DAILY SUPPLIES ARE DELIVERED TO THE CHUTE.\nPENDING ITEMS: {ItemInventoryService.DeliveryCount}",
                "도움말" => "HELP\n\n1. COMPLETE THE DAILY PAYMENT BEFORE MIDNIGHT.\n2. DEPOSIT SURPLUS LABOR INTO THE FREEDOM FUND.\n3. COLLECT DELIVERED ITEMS FROM THE CHUTE.",
                _ => $"PRISONER ACCOUNT: ANONYMOUS\nDAY {GameSaveService.Day:00}  |  UNTIL MIDNIGHT {FormatTime(GameDayClock.SecondsUntilMidnight)}\n\nREVIEW TODAY'S CHARGE AND SELECT A REQUIRED TASK."
            };
        }

        Text(copy, terminalContent, new Vector2(-130, 48), new Vector2(650, 185), 21, Color.black).alignment = TextAlignmentOptions.TopLeft;
        Text("SYSTEM READY", terminalContent, new Vector2(-130, -175), new Vector2(650, 24), 15, new Color(0f, .36f, .2f, 1)).alignment = TextAlignmentOptions.Left;
    }

    private void RenderComputerHome()
    {
        Text(L("수용자 단말기", "PRISONER TERMINAL"), terminalContent, new Vector2(-330, 168), new Vector2(260, 34), 27, new Color(0f, .08f, .36f, 1)).alignment = TextAlignmentOptions.Left;
        Text("SYSTEM OVERVIEW", terminalContent, new Vector2(250, 168), new Vector2(210, 24), 14, new Color(.35f, .35f, .35f, 1)).alignment = TextAlignmentOptions.Right;

        CreateComputerStatCard("DAY", GameSaveService.Day.ToString("00"), new Vector2(-315, 70), new Color(0f, .08f, .48f, 1));
        CreateComputerStatCard("LEVEL", CardProgressionService.Level.ToString("00"), new Vector2(-105, 70), new Color(.28f, .08f, .42f, 1));
        computerTimeLabel = CreateComputerStatCard("자정까지", FormatTime(GameDayClock.SecondsUntilMidnight), new Vector2(105, 70), new Color(.45f, .08f, .04f, 1));
        CreateComputerStatCard("보유 노동값", GameSaveService.Labor.ToString("N0"), new Vector2(315, 70), new Color(.08f, .32f, .18f, 1));

        GameObject notice = ImageObject("Notice", terminalContent, new Color(.91f, .91f, .91f, 1));
        SetRect(notice.GetComponent<RectTransform>(), new Vector2(0, -58), new Vector2(850, 84));
        Outline outline = notice.AddComponent<Outline>();
        outline.effectColor = new Color(.35f, .35f, .35f, 1);
        outline.effectDistance = new Vector2(1, -1);
        string paymentState = GameSaveService.DailyPaymentPaid ? "오늘 납부 완료" : $"오늘 미납 · {GameDayClock.DailyLaborPayment:N0} 노동값 필요";
        Text(paymentState, notice.transform, new Vector2(-245, 15), new Vector2(340, 26), 19, GameSaveService.DailyPaymentPaid ? new Color(.05f, .35f, .15f, 1) : new Color(.5f, .06f, .03f, 1)).alignment = TextAlignmentOptions.Left;
        Text($"자유 기금  {GameSaveService.FreedomFund:N0} / {GameEconomy.FreedomGoal:N0}", notice.transform, new Vector2(180, -17), new Vector2(430, 24), 16, Color.black).alignment = TextAlignmentOptions.Right;
        Text(L("아래 업무 버튼 또는 데스크톱 아이콘을 선택하십시오.", "SELECT A TASK BUTTON BELOW OR AN APPLICATION ON THE DESKTOP."), terminalContent, new Vector2(0, -145), new Vector2(760, 26), 16, new Color(.25f, .25f, .25f, 1));
    }

    private TextMeshProUGUI CreateComputerStatCard(string label, string value, Vector2 position, Color accent)
    {
        GameObject card = ImageObject(label, terminalContent, new Color(.9f, .9f, .9f, 1));
        SetRect(card.GetComponent<RectTransform>(), position, new Vector2(190, 100));
        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = new Color(.32f, .32f, .32f, 1);
        outline.effectDistance = new Vector2(1, -1);
        GameObject stripe = ImageObject("Accent", card.transform, accent);
        SetRect(stripe.GetComponent<RectTransform>(), new Vector2(-90, 0), new Vector2(8, 94));
        Text(label, card.transform, new Vector2(18, 24), new Vector2(130, 22), 14, new Color(.28f, .28f, .28f, 1)).alignment = TextAlignmentOptions.Left;
        TextMeshProUGUI valueLabel = Text(value, card.transform, new Vector2(18, -16), new Vector2(130, 36), value.Length > 8 ? 19 : 24, Color.black);
        valueLabel.alignment = TextAlignmentOptions.Left;
        return valueLabel;
    }

    private void CreateComputerNavButton(string label, Transform parent, Vector2 position, UnityEngine.Events.UnityAction callback)
    {
        Button button = CreateWindowButton(label, parent, position, new Vector2(185, 53));
        button.onClick.AddListener(callback);
    }

    private void CreateComputerAction(string label, Vector2 position, UnityEngine.Events.UnityAction callback)
    {
        Button button = CreateWindowButton(label, terminalContent, position, new Vector2(170, 42));
        button.onClick.AddListener(callback);
    }

    private void PayDailyLabor()
    {
        int payment = GameDayClock.DailyLaborPayment;
        if (GameSaveService.DailyPaymentPaid) GameNotificationCenter.Show("오늘의 노동값은 이미 납부했습니다.");
        else if (GameSaveService.Labor < payment) GameNotificationCenter.Error($"노동값이 {payment - GameSaveService.Labor:N0} 부족합니다.");
        else
        {
            GameSaveService.SaveProgress(GameSaveService.Day, GameSaveService.Labor - payment, GameSaveService.Debt);
            GameSaveService.MarkDailyPaymentPaid();
            GameNotificationCenter.Success($"오늘의 노동값 {payment:N0}을 납부했습니다.");
        }
        ShowComputerPage("일일 납부");
        RefreshStatusHud();
    }

    private void PayDailyLaborInApp(GameObject window)
    {
        int payment = GameDayClock.DailyLaborPayment;
        if (GameSaveService.DailyPaymentPaid) GameNotificationCenter.Show("오늘의 노동값은 이미 납부했습니다.");
        else if (GameSaveService.Labor < payment) GameNotificationCenter.Error($"노동값이 {payment - GameSaveService.Labor:N0} 부족합니다.");
        else
        {
            GameSaveService.SaveProgress(GameSaveService.Day, GameSaveService.Labor - payment, GameSaveService.Debt);
            GameSaveService.MarkDailyPaymentPaid();
            GameNotificationCenter.Success($"오늘의 노동값 {payment:N0}을 납부했습니다.");
        }
        RefreshStatusHud();
        Destroy(window);
        OpenAppWindow("일일 납부");
    }

    private void PayFreedomFund()
    {
        int remaining = Mathf.Max(0, GameEconomy.FreedomGoal - GameSaveService.FreedomFund);
        if (remaining == 0)
        {
            TryBeginFreedomEnding();
            return;
        }
        int payment = Mathf.Min(GameEconomy.Scale(10), remaining);
        if (GameSaveService.Labor < payment) GameNotificationCenter.Error($"노동값이 {payment - GameSaveService.Labor:N0} 부족합니다.");
        else
        {
            GameSaveService.SaveProgress(GameSaveService.Day, GameSaveService.Labor - payment, GameSaveService.Debt);
            GameSaveService.SetFreedomFund(Mathf.Min(GameEconomy.FreedomGoal, GameSaveService.FreedomFund + payment));
            GameNotificationCenter.Success($"자유 기금에 {payment:N0}을 납부했습니다.");
            if (TryBeginFreedomEnding()) return;
        }
        ShowComputerPage("자유 기금");
        RefreshStatusHud();
    }

    private void PayFreedomFundInApp(int amount, GameObject window)
    {
        if (GameSaveService.Labor < amount)
        {
            GameNotificationCenter.Error($"노동값이 {amount - GameSaveService.Labor:N0} 부족합니다.");
            return;
        }
        GameSaveService.SaveProgress(GameSaveService.Day, GameSaveService.Labor - amount, GameSaveService.Debt);
        GameSaveService.SetFreedomFund(Mathf.Min(GameEconomy.FreedomGoal, GameSaveService.FreedomFund + amount));
        GameNotificationCenter.Success($"자유 기금에 {amount:N0}을 납부했습니다.");
        RefreshStatusHud();
        if (TryBeginFreedomEnding(window)) return;
        Destroy(window);
        OpenAppWindow("자유 기금");
    }

    private static void SetFundInputFraction(TMP_InputField input, int maximum, float fraction)
    {
        if (input == null) return;
        input.text = maximum <= 0 ? "0" : Mathf.Max(1, Mathf.FloorToInt(maximum * fraction)).ToString();
    }

    private bool TryBeginFreedomEnding(GameObject window = null)
    {
        if (GameSaveService.FreedomFund < GameEconomy.FreedomGoal) return false;
        if (window != null) Destroy(window);
        GameNotificationCenter.Success("자유 기금 완납 · 석방 절차를 시작합니다.");
        DailyStoryController.BeginFreedomEnding();
        return true;
    }

    private void PayFreedomFundFromInput(string rawAmount, GameObject window)
    {
        string sanitized = string.IsNullOrWhiteSpace(rawAmount) ? string.Empty : rawAmount.Replace(",", string.Empty).Trim();
        if (!long.TryParse(sanitized, out long parsedAmount) || parsedAmount <= 0 || parsedAmount > int.MaxValue)
        {
            GameNotificationCenter.Error("납부할 금액을 올바르게 입력하십시오.");
            return;
        }

        int amount = (int)parsedAmount;
        int remainingFund = Mathf.Max(0, GameEconomy.FreedomGoal - GameSaveService.FreedomFund);
        if (remainingFund <= 0)
        {
            GameNotificationCenter.Show("자유 기금 목표를 이미 달성했습니다.");
            return;
        }
        if (amount > remainingFund)
        {
            GameNotificationCenter.Error($"남은 목표액은 {remainingFund:N0}입니다.");
            return;
        }
        if (amount > GameSaveService.Labor)
        {
            GameNotificationCenter.Error($"노동값이 {amount - GameSaveService.Labor:N0} 부족합니다.");
            return;
        }

        PayFreedomFundInApp(amount, window);
    }

    private void BuyComputerItem(string item, int price, GameObject window)
    {
        if (GameSaveService.Labor < price)
        {
            GameNotificationCenter.Error($"구매 실패: 노동값이 {price - GameSaveService.Labor:N0} 부족합니다.");
            return;
        }
        GameSaveService.SaveProgress(GameSaveService.Day, GameSaveService.Labor - price, GameSaveService.Debt);
        ItemInventoryService.QueueDelivery(item);
        GameNotificationCenter.Success($"{item} 구매 완료 · 물건 투입구로 배송했습니다.");
        RefreshStatusHud();
        Destroy(window);
        OpenAppWindow("데일리 상품");
    }

    private int CurrentRiskBet => GameEconomy.Scale(RiskBetMultipliers[riskBetIndex]);

    private void AdjustRiskBet(int direction, GameObject window)
    {
        riskBetIndex = Mathf.Clamp(riskBetIndex + direction, 0, RiskBetMultipliers.Length - 1);
        Destroy(window);
        OpenAppWindow("위험 게임");
    }

    private void PlayRiskGame(GameObject window)
    {
        int bet = CurrentRiskBet;
        if (GameSaveService.Labor < bet)
        {
            GameNotificationCenter.Error($"게임 실패: 노동값이 {bet - GameSaveService.Labor:N0} 부족합니다.");
            return;
        }
        int reward = Random.value < .42f
            ? (int)System.Math.Min(1_500_000_000L, System.Math.Round((double)bet * UpgradeService.RiskPayoutMultiplier))
            : 0;
        riskMessage = reward > 0
            ? L($"당첨. {reward:N0} 노동값이 지급되었습니다.", $"WIN. {reward:N0} LABOR AWARDED.")
            : L("실패. 베팅 노동값을 잃었습니다.", "FAILED. THE WAGERED LABOR WAS LOST.");
        if (reward > 0) GameNotificationCenter.Success($"당첨! +{reward:N0} 노동값");
        else GameNotificationCenter.Error($"실패 · {bet:N0} 노동값을 잃었습니다.");
        int resultingLabor = (int)System.Math.Clamp((long)GameSaveService.Labor - bet + reward, 0L, 1_500_000_000L);
        GameSaveService.SaveProgress(GameSaveService.Day, resultingLabor, GameSaveService.Debt);
        RefreshStatusHud();
        Destroy(window);
        OpenAppWindow("위험 게임");
    }

    private void BuyToolItem(string item, int price, GameObject window)
    {
        if (GameSaveService.Labor < price)
        {
            GameNotificationCenter.Error($"구매 실패: 노동값이 {price - GameSaveService.Labor:N0} 부족합니다.");
            return;
        }
        GameSaveService.SaveProgress(GameSaveService.Day, GameSaveService.Labor - price, GameSaveService.Debt);
        ItemInventoryService.QueueDelivery(item);
        RefreshStatusHud();
        GameNotificationCenter.Success($"{item} 구매 완료 · 물건 투입구로 배송했습니다.");
        Destroy(window);
        OpenAppWindow("도구 상점");
    }

    private void CreateToolStoreCard(string item, string description, int basePrice, Vector2 position, Transform parent, GameObject window)
    {
        int price = Mathf.Max(1, Mathf.RoundToInt(GameEconomy.Scale(basePrice) * (1f - UpgradeService.ToolDiscount)));
        GameObject card = ImageObject(item + " Card", parent, new Color(.88f, .9f, .87f, 1));
        SetRect(card.GetComponent<RectTransform>(), position, new Vector2(200, 190));
        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = new Color(.2f, .28f, .22f, 1);
        outline.effectDistance = new Vector2(1, -1);
        Text(item, card.transform, new Vector2(0, 60), new Vector2(180, 28), 19, new Color(.08f, .22f, .12f, 1));
        Text(description, card.transform, new Vector2(0, 15), new Vector2(175, 48), 14, new Color(.24f, .28f, .25f, 1));
        Text(L($"{price:N0} 노동값", $"{price:N0} LABOR"), card.transform, new Vector2(0, -30), new Vector2(180, 22), 15, Color.black);
        CreateWindowButton(L("구매", "BUY"), card.transform, new Vector2(0, -68), new Vector2(126, 34)).onClick.AddListener(() => BuyToolItem(item, price, window));
    }

    private void ClaimDailyReward(GameObject window)
    {
        if (GameSaveService.DailyRewardClaimed)
        {
            GameNotificationCenter.Show("오늘의 무료 보상은 이미 받았습니다.");
            return;
        }
        ItemInventoryService.QueueDelivery("무료 카드팩");
        GameSaveService.MarkDailyRewardClaimed();
        GameNotificationCenter.Success("무료 카드팩을 물건 투입구로 보냈습니다.");
        Destroy(window);
        OpenAppWindow("데일리 보상");
    }

    private void BuildCardPackShop(Transform page, GameObject window)
    {
        Text("CARD SUPPLY SHOP", page, new Vector2(-205, 260), new Vector2(430, 36), 27, new Color(.3f, .06f, .36f, 1)).alignment = TextAlignmentOptions.Left;
        Text(L($"수용자 LV {CardProgressionService.Level:00}  ·  해금 팩 {CardProgressionService.Level}/{CardProgressionService.PackCount}", $"PRISONER LV {CardProgressionService.Level:00}  ·  UNLOCKED {CardProgressionService.Level}/{CardProgressionService.PackCount}"), page, new Vector2(255, 260), new Vector2(390, 28), 16, Color.black).alignment = TextAlignmentOptions.Right;
        Text(L("각 카드팩은 요구 레벨, 희귀도 보정과 판매가 배율이 다릅니다.", "EACH PACK HAS A UNIQUE LEVEL, LUCK BONUS, AND VALUE MULTIPLIER."), page, new Vector2(-105, 225), new Vector2(630, 24), 15, new Color(.28f, .28f, .32f, 1)).alignment = TextAlignmentOptions.Left;

        GameObject viewportObject = ImageObject("Card Pack Viewport", page, new Color(.94f, .94f, .95f, 1));
        SetRect(viewportObject.GetComponent<RectTransform>(), new Vector2(-8, -25), new Vector2(820, 465));
        RectMask2D mask = viewportObject.AddComponent<RectMask2D>();
        mask.padding = new Vector4(4, 4, 4, 4);

        GameObject contentObject = new("Card Pack Content", typeof(RectTransform));
        contentObject.transform.SetParent(viewportObject.transform, false);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = contentRect.anchorMax = new Vector2(.5f, 1f);
        contentRect.pivot = new Vector2(.5f, 1f);
        contentRect.sizeDelta = new Vector2(780, 15 * 128 + 12);
        contentRect.anchoredPosition = Vector2.zero;

        ScrollRect scroll = viewportObject.AddComponent<ScrollRect>();
        scroll.content = contentRect;
        scroll.viewport = viewportObject.GetComponent<RectTransform>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 48f;
        scroll.inertia = true;
        scroll.decelerationRate = .12f;

        GameObject scrollbarObject = ImageObject("Card Pack Scrollbar", page, new Color(.72f, .72f, .74f, 1));
        SetRect(scrollbarObject.GetComponent<RectTransform>(), new Vector2(418, -25), new Vector2(18, 465));
        Scrollbar scrollbar = scrollbarObject.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        GameObject handleObject = ImageObject("Handle", scrollbarObject.transform, new Color(.24f, .08f, .3f, 1));
        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.offsetMin = new Vector2(2, 2);
        handleRect.offsetMax = new Vector2(-2, -2);
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handleObject.GetComponent<Image>();
        scrollbar.size = Mathf.Clamp01(465f / contentRect.sizeDelta.y);
        scrollbar.value = 1f;
        scroll.verticalScrollbar = scrollbar;
        scroll.verticalScrollbarSpacing = 5f;

        for (int index = 0; index < CardProgressionService.PackCount; index++)
        {
            CardPackDefinition pack = CardProgressionService.GetPack(index);
            int column = index % 2;
            int row = index / 2;
            Vector2 position = new(column == 0 ? -195 : 195, -64 - row * 128);
            CreateComputerPackCard(pack, position, contentObject.transform, window);
        }
    }

    private void CreateComputerPackCard(CardPackDefinition pack, Vector2 position, Transform parent, GameObject window)
    {
        bool unlocked = CardProgressionService.Level >= pack.RequiredLevel;
        float tier = (pack.RequiredLevel - 1f) / (CardProgressionService.PackCount - 1f);
        Color cardColor = unlocked ? Color.Lerp(new Color(.88f, .9f, .94f, 1), new Color(.91f, .82f, .67f, 1), tier) : new Color(.74f, .74f, .76f, 1);
        GameObject card = ImageObject(pack.Name, parent, cardColor);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        SetRect(cardRect, position, new Vector2(370, 112));
        cardRect.anchorMin = cardRect.anchorMax = new Vector2(.5f, 1f);
        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = unlocked ? Color.Lerp(new Color(.18f, .16f, .28f, 1), new Color(.45f, .24f, .04f, 1), tier) : new Color(.35f, .35f, .37f, 1);
        outline.effectDistance = new Vector2(1, -1);
        GameObject tierStripe = ImageObject("Tier", card.transform, Color.Lerp(new Color(.1f, .16f, .48f, 1), new Color(.78f, .43f, .04f, 1), tier));
        SetRect(tierStripe.GetComponent<RectTransform>(), new Vector2(-179, 0), new Vector2(7, 106));
        Text(pack.Name, card.transform, new Vector2(-62, 30), new Vector2(225, 26), 18, unlocked ? new Color(.14f, .04f, .2f, 1) : new Color(.3f, .3f, .32f, 1)).alignment = TextAlignmentOptions.Left;
        Text($"LV {pack.RequiredLevel:00}", card.transform, new Vector2(130, 31), new Vector2(72, 23), 14, unlocked ? new Color(.3f, .06f, .36f, 1) : new Color(.42f, .12f, .12f, 1));
        Text(L($"행운 {pack.LuckBonus * 100f:0}%   ·   카드 가치 x{pack.ValueMultiplier:N0}   ·   EXP +{pack.Experience}", $"LUCK {pack.LuckBonus * 100f:0}%   ·   VALUE x{pack.ValueMultiplier:N0}   ·   EXP +{pack.Experience}"), card.transform, new Vector2(-35, 3), new Vector2(280, 22), 13, new Color(.25f, .25f, .28f, 1)).alignment = TextAlignmentOptions.Left;
        int price = CardProgressionService.GetPackPrice(pack.Name);
        Text(L($"{price:N0} 노동값", $"{price:N0} LABOR"), card.transform, new Vector2(-63, -28), new Vector2(220, 22), 14, Color.black).alignment = TextAlignmentOptions.Left;
        Button buy = CreateWindowButton(unlocked ? L("구매", "BUY") : L($"LV {pack.RequiredLevel} 필요", $"REQUIRES LV {pack.RequiredLevel}"), card.transform, new Vector2(122, -27), new Vector2(118, 34));
        buy.interactable = unlocked;
        buy.onClick.AddListener(() => BuyCardPack(pack.Name, price, window));
    }

    private void BuyCardPack(string pack, int price, GameObject window)
    {
        if (GameSaveService.Labor < price)
        {
            GameNotificationCenter.Error($"구매 실패: 노동값이 {price - GameSaveService.Labor:N0} 부족합니다.");
            return;
        }
        GameSaveService.SaveProgress(GameSaveService.Day, GameSaveService.Labor - price, GameSaveService.Debt);
        ItemInventoryService.QueueDelivery(pack);
        GameNotificationCenter.Success($"{pack} 구매 완료 · 물건 투입구로 배송했습니다.");
        RefreshStatusHud();
    }

    private void CreateUpgradeRow(string type, string title, string description, Vector2 position, Transform parent)
    {
        GameObject row = ImageObject(title, parent, new Color(.89f, .89f, .91f, 1));
        SetRect(row.GetComponent<RectTransform>(), position, new Vector2(600, 56));
        Outline outline = row.AddComponent<Outline>();
        outline.effectColor = new Color(.35f, .35f, .38f, 1);
        outline.effectDistance = new Vector2(1, -1);
        Text(title, row.transform, new Vector2(-125, 11), new Vector2(270, 22), 17, new Color(.08f, .1f, .16f, 1)).alignment = TextAlignmentOptions.Left;
        Text(description, row.transform, new Vector2(-105, -13), new Vector2(310, 20), 13, new Color(.32f, .32f, .36f, 1)).alignment = TextAlignmentOptions.Left;
        int level = UpgradeService.GetLevel(type);
        int max = UpgradeService.GetMaxLevel(type);
        Text($"LV {level}/{max}", row.transform, new Vector2(100, 0), new Vector2(70, 22), 14, Color.black);
        int cost = UpgradeService.GetCost(type);
        Button buy = CreateWindowButton(level >= max ? "MAX" : $"{cost:N0} 구매", row.transform, new Vector2(215, 0), new Vector2(150, 34));
        buy.interactable = level < max;
        buy.onClick.AddListener(() => PurchaseUpgrade(type, parent.parent.gameObject));
    }

    private void PurchaseUpgrade(string type, GameObject window)
    {
        int cost = UpgradeService.GetCost(type);
        if (UpgradeService.GetLevel(type) >= UpgradeService.GetMaxLevel(type))
        {
            GameNotificationCenter.Show("이미 최대 단계인 업그레이드입니다.");
            return;
        }
        if (GameSaveService.Labor < cost)
        {
            GameNotificationCenter.Error($"업그레이드 실패: 노동값이 {cost - GameSaveService.Labor:N0} 부족합니다.");
            return;
        }
        if (!UpgradeService.Purchase(type)) return;
        GameNotificationCenter.Success($"업그레이드 완료 · {cost:N0} 노동값 사용");
        slotCapacity = UpgradeService.InventoryCapacity;
        RefreshInventoryUi();
        SelectSlot(Mathf.Min(selectedSlot, slotCapacity - 1));
        RefreshStatusHud();
        Destroy(window);
        OpenAppWindow("업그레이드 상점");
    }

    private void BuildChuteWindow()
    {
        ItemInventoryService.EnsureInitialDelivery();
        terminalPanel = CreateModalSurface("Delivery Chute", new Vector2(900, 590), new Color(.055f, .065f, .07f, .98f));
        CreateHeader(terminalPanel.transform, "물건 투입구", "DELIVERY CHUTE  //  RECEIVING", new Color(.08f, .32f, .36f, 1), new Vector2(860, 76), 242);

        Text("시설 배송망", terminalPanel.transform, new Vector2(-340, 178), new Vector2(180, 26), 17, new Color(.35f, .8f, .78f, 1)).alignment = TextAlignmentOptions.Left;
        Text(GameLanguage.IsEnglish ? $"PENDING  {ItemInventoryService.DeliveryCount}" : $"대기 {ItemInventoryService.DeliveryCount}개", terminalPanel.transform, new Vector2(330, 178), new Vector2(170, 26), 17, new Color(.72f, .78f, .77f, 1)).alignment = TextAlignmentOptions.Right;

        GameObject listPanel = CreateRoundedPanel("Delivery Queue", terminalPanel.transform, new Vector2(-155, -5), new Vector2(510, 330), new Color(.09f, .105f, .11f, 1), 12);
        Text("수령 대기 목록", listPanel.transform, new Vector2(0, 130), new Vector2(410, 28), 22, Color.white).alignment = TextAlignmentOptions.Left;
        if (ItemInventoryService.DeliveryCount == 0)
        {
            Text("도착한 물품이 없습니다.\n컴퓨터에서 구매한 상품은 이곳으로 배송됩니다.", listPanel.transform, Vector2.zero, new Vector2(410, 85), 18, new Color(.55f, .61f, .61f, 1));
        }
        else
        {
            GameObject viewportObject = CreateRoundedPanel("Delivery Viewport", listPanel.transform, new Vector2(-8, -25), new Vector2(442, 245), new Color(.075f, .088f, .092f, 1), 8);
            viewportObject.AddComponent<RectMask2D>();
            GameObject contentObject = new("Delivery Content", typeof(RectTransform));
            contentObject.transform.SetParent(viewportObject.transform, false);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = contentRect.anchorMax = new Vector2(.5f, 1f);
            contentRect.pivot = new Vector2(.5f, 1f);
            contentRect.sizeDelta = new Vector2(410, Mathf.Max(245, ItemInventoryService.DeliveryCount * 58 + 8));
            contentRect.anchoredPosition = Vector2.zero;

            ScrollRect scroll = viewportObject.AddComponent<ScrollRect>();
            scroll.content = contentRect;
            scroll.viewport = viewportObject.GetComponent<RectTransform>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 42f;

            for (int index = 0; index < ItemInventoryService.DeliveryCount; index++)
            {
                GameObject row = CreateRoundedPanel("Delivery Item", contentObject.transform, new Vector2(-5, -29 - index * 58), new Vector2(390, 50), new Color(.14f, .16f, .17f, 1), 8);
                RectTransform rowRect = row.GetComponent<RectTransform>();
                rowRect.anchorMin = rowRect.anchorMax = new Vector2(.5f, 1f);
                Text(ItemInventoryService.GetDelivery(index), row.transform, new Vector2(-88, 0), new Vector2(205, 30), 16, Color.white).alignment = TextAlignmentOptions.Left;
                int deliveryIndex = index;
                Button receive = CreateActionButton("수령", row.transform, new Vector2(142, 0), new Vector2(82, 34), new Color(.08f, .42f, .39f, 1));
                receive.onClick.AddListener(() => ClaimSingleDeliveryAndRefresh(deliveryIndex));
            }

            GameObject scrollTrack = CreateRoundedPanel("Delivery Scrollbar", listPanel.transform, new Vector2(224, -25), new Vector2(12, 245), new Color(.12f, .14f, .145f, 1), 5);
            Scrollbar scrollbar = scrollTrack.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            GameObject handle = CreateRoundedPanel("Handle", scrollTrack.transform, Vector2.zero, new Vector2(8, 70), new Color(.22f, .58f, .56f, 1), 4);
            scrollbar.handleRect = handle.GetComponent<RectTransform>();
            scrollbar.targetGraphic = handle.GetComponent<Image>();
            scroll.verticalScrollbar = scrollbar;
            StartCoroutine(ResetScrollToTop(scroll));
        }

        GameObject capacity = CreateRoundedPanel("Capacity", terminalPanel.transform, new Vector2(275, 38), new Vector2(245, 245), new Color(.085f, .095f, .1f, 1), 12);
        int used = ItemInventoryService.UsedSlots(slotCapacity);
        Text("인벤토리", capacity.transform, new Vector2(0, 82), new Vector2(190, 28), 21, Color.white);
        Text($"{used} / {slotCapacity}", capacity.transform, new Vector2(0, 24), new Vector2(190, 52), 34, new Color(.35f, .8f, .78f, 1));
        Text("빈 슬롯만큼 수령하며\n나머지는 이곳에 보관됩니다.", capacity.transform, new Vector2(0, -55), new Vector2(200, 58), 15, new Color(.56f, .62f, .62f, 1));

        Button claim = CreateActionButton("모두 수령", terminalPanel.transform, new Vector2(275, -125), new Vector2(245, 54), new Color(.08f, .42f, .39f, 1));
        claim.interactable = ItemInventoryService.DeliveryCount > 0;
        claim.onClick.AddListener(ClaimDeliveriesAndRefresh);
        Text("E/ESC  닫기", terminalPanel.transform, new Vector2(330, -250), new Vector2(190, 24), 14, new Color(.42f, .47f, .47f, 1));
    }

    private void ClaimDeliveriesAndRefresh()
    {
        int freeSlots = slotCapacity - ItemInventoryService.UsedSlots(slotCapacity);
        if (freeSlots <= 0)
        {
            GameNotificationCenter.Error("인벤토리가 가득 찼습니다. 물건을 판매하거나 슬롯을 확장하십시오.");
            return;
        }

        int before = ItemInventoryService.DeliveryCount;
        int claimed = ItemInventoryService.ClaimAll(slotCapacity);
        RefreshInventoryUi();
        if (claimed <= 0)
        {
            GameNotificationCenter.Error("물품을 수령할 빈 인벤토리 슬롯이 없습니다.");
            return;
        }
        if (claimed < before)
            GameNotificationCenter.Show($"{claimed}개 수령 · 남은 {before - claimed}개는 투입구에 보관됩니다.");
        else
            GameNotificationCenter.Success($"배송 물품 {claimed}개를 모두 수령했습니다.");
        CloseTerminal();
    }

    private void ClaimSingleDeliveryAndRefresh(int deliveryIndex)
    {
        if (ItemInventoryService.UsedSlots(slotCapacity) >= slotCapacity)
        {
            GameNotificationCenter.Error("인벤토리가 가득 찼습니다. 빈 슬롯을 먼저 확보하십시오.");
            return;
        }

        string item = ItemInventoryService.GetDelivery(deliveryIndex);
        if (!ItemInventoryService.ClaimAt(deliveryIndex, slotCapacity))
        {
            GameNotificationCenter.Error("선택한 물품을 수령하지 못했습니다.");
            return;
        }

        RefreshInventoryUi();
        GameNotificationCenter.Success($"{item} 1개를 수령했습니다.");
        Destroy(terminalPanel);
        terminalPanel = null;
        BuildChuteWindow();
    }

    private static System.Collections.IEnumerator ResetScrollToTop(ScrollRect scroll)
    {
        yield return null;
        if (scroll == null) yield break;
        Canvas.ForceUpdateCanvases();
        scroll.verticalNormalizedPosition = 1f;
    }

    private void BuildWorkbenchWindow()
    {
        terminalPanel = CreateModalSurface("Workbench", new Vector2(920, 590), new Color(.065f, .052f, .04f, .985f));
        CreateHeader(terminalPanel.transform, "작업대", "LABOR BENCH  //  CARD & CONTAINER", new Color(.42f, .14f, .045f, 1), new Vector2(880, 76), 242);
        Text("개봉 및 해제", terminalPanel.transform, new Vector2(0, 167), new Vector2(820, 32), 25, Color.white).alignment = TextAlignmentOptions.Left;
        Text("카드팩은 즉시 개봉하고, 봉인된 상자는 락핀 스킬 체크로 해제합니다.", terminalPanel.transform, new Vector2(0, 128), new Vector2(820, 22), 15, new Color(.65f, .58f, .5f, 1)).alignment = TextAlignmentOptions.Left;

        GameObject packList = CreateRoundedPanel("Pack List", terminalPanel.transform, new Vector2(0, -64), new Vector2(820, 300), new Color(.105f, .085f, .065f, 1), 12);
        int row = 0;
        for (int slot = 0; slot < slotCapacity && row < 4; slot++)
        {
            string item = ItemInventoryService.GetItem(slot);
            if (!ItemInventoryService.IsCardPack(item)) continue;
            int capturedSlot = slot;
            GameObject packRow = CreateRoundedPanel("Pack", packList.transform, new Vector2(0, 95 - row * 64), new Vector2(720, 54), new Color(.16f, .125f, .09f, 1), 8);
            Text(item, packRow.transform, new Vector2(-220, 8), new Vector2(230, 26), 19, Color.white).alignment = TextAlignmentOptions.Left;
            Text($"인벤토리 슬롯 {slot + 1}", packRow.transform, new Vector2(-220, -16), new Vector2(230, 18), 13, new Color(.62f, .55f, .47f, 1)).alignment = TextAlignmentOptions.Left;
            Button open = CreateActionButton("개봉", packRow.transform, new Vector2(275, 0), new Vector2(120, 38), new Color(.48f, .18f, .07f, 1));
            open.onClick.AddListener(() => StartCardPackOpening(capturedSlot));
            row++;
        }
        for (int slot = 0; slot < slotCapacity && row < 4; slot++)
        {
            string boxItem = ItemInventoryService.GetItem(slot);
            if (!ItemInventoryService.IsLootBox(boxItem)) continue;
            int capturedSlot = slot;
            GameObject boxRow = CreateRoundedPanel("Sealed Box", packList.transform, new Vector2(0, 95 - row * 64), new Vector2(720, 54), new Color(.17f, .105f, .055f, 1), 8);
            Text(boxItem, boxRow.transform, new Vector2(-220, 8), new Vector2(230, 26), 19, Color.white).alignment = TextAlignmentOptions.Left;
            Text(GameLanguage.IsEnglish
                    ? $"SLOT {slot + 1} · RARITY {ItemInventoryService.GetLootBoxRarity(boxItem)} · LOCKPICK REQUIRED"
                    : $"슬롯 {slot + 1} · 희귀도 {ItemInventoryService.GetLootBoxRarity(boxItem)} · 락핀 필요",
                boxRow.transform, new Vector2(-190, -16), new Vector2(290, 18), 13, new Color(.72f, .48f, .3f, 1)).alignment = TextAlignmentOptions.Left;
            CreateActionButton("락픽", boxRow.transform, new Vector2(190, 0), new Vector2(82, 38), new Color(.52f, .16f, .045f, 1)).onClick.AddListener(() => StartLockpickSkillCheck(capturedSlot));
            CreateActionButton("드릴", boxRow.transform, new Vector2(278, 0), new Vector2(78, 38), new Color(.34f, .2f, .08f, 1)).onClick.AddListener(() => StartDrillCheck(capturedSlot));
            CreateActionButton("절단", boxRow.transform, new Vector2(348, 0), new Vector2(60, 38), new Color(.24f, .26f, .22f, 1)).onClick.AddListener(() => UseHydraulicCutter(capturedSlot));
            row++;
        }
        if (row == 0)
            Text("개봉할 카드팩이나 봉인된 상자가 없습니다.\n컴퓨터와 야시장에서 물품을 구하십시오.", packList.transform, Vector2.zero, new Vector2(620, 80), 20, new Color(.64f, .58f, .5f, 1));
    }

    private void StartCardPackOpening(int slot)
    {
        string pack = ItemInventoryService.GetItem(slot);
        if (!ItemInventoryService.IsCardPack(pack)) return;

        int packLevel = 1;
        if (CardProgressionService.TryGetPack(pack, out CardPackDefinition definition)) packLevel = definition.RequiredLevel;
        else if (pack == "보급 카드팩") packLevel = 8;
        else if (pack == "고급 카드팩") packLevel = 16;
        packOpeningProtocol = (packLevel - 1) % 5;
        if (packOpeningProtocol == 4 && FindInventoryItem("미니 노트북") < 0)
        {
            GameNotificationCenter.Error("이 카드팩은 미니 노트북 해킹이 필요합니다. 도구 상점에서 구매하십시오.");
            return;
        }

        packOpeningActive = true;
        packOpeningSlot = slot;
        packOpeningName = pack;
        packOpeningSpeed = .66f + packLevel * .032f;
        packOpeningZoneWidth = Mathf.Max(.075f, .31f - packLevel * .0065f);
        if (FindInventoryItem("신호 복호기") >= 0 && packOpeningProtocol <= 2) packOpeningZoneWidth *= 1.22f;
        packOpeningPhase = packOpeningProtocol == 2 ? Random.value : packOpeningProtocol == 3 ? -1f : Random.value * 2f;
        packOpeningTarget = Random.Range(-.68f, .68f);
        packChargeStarted = false;
        packHackIndex = 0;
        packHackCode = packOpeningProtocol == 4 ? GenerateHackCode(Mathf.Clamp(3 + packLevel / 5, 3, 9)) : string.Empty;
        packOpeningOverlay = CreateRoundedPanel("Card Pack Opening", terminalPanel.transform, Vector2.zero, new Vector2(820, 470), new Color(.035f, .026f, .02f, .995f), 16);
        packOpeningOverlay.transform.SetAsLastSibling();
        int protocolNameIndex = Mathf.Clamp(packLevel - 1, 0, PackProtocolNames.Length - 1);
        Text(GameLanguage.IsEnglish ? PackProtocolNamesEnglish[protocolNameIndex] : PackProtocolNames[protocolNameIndex], packOpeningOverlay.transform, new Vector2(0, 185), new Vector2(700, 38), 26, new Color(.94f, .72f, .38f, 1));
        Text(pack, packOpeningOverlay.transform, new Vector2(0, 140), new Vector2(680, 30), 21, Color.white);
        string instruction = GameLanguage.IsEnglish ? packOpeningProtocol switch
        {
            0 => "PRESS SPACE WHEN THE HORIZONTAL SCANNER ENTERS THE GOLD ZONE.",
            1 => "PRESS SPACE WHEN THE VERTICAL MARKER ENTERS THE AUTHORIZATION ZONE.",
            2 => "PRESS SPACE AS THE ROTARY DIAL PASSES THROUGH THE GOLD ZONE.",
            3 => "HOLD SPACE TO CHARGE, THEN RELEASE IT INSIDE THE TARGET ZONE.",
            _ => "ENTER THE W/A/S/D ACCESS CODE SHOWN ON THE MINI NOTEBOOK."
        } : packOpeningProtocol switch
        {
            0 => "가로 스캐너를 황금 구간에 맞춰 SPACE를 누르십시오.",
            1 => "세로 광학 표식을 인증 구간에 맞춰 SPACE를 누르십시오.",
            2 => "회전 다이얼이 황금 구간을 지날 때 SPACE를 누르십시오.",
            3 => "SPACE를 누르고 충전한 뒤 목표 구간에서 손을 떼십시오.",
            _ => "미니 노트북에 표시된 W/A/S/D 접속 코드를 순서대로 입력하십시오."
        };
        Text(instruction + (GameLanguage.IsEnglish ? "\nHIGHER-LEVEL PACKS MOVE FASTER AND HAVE NARROWER SUCCESS ZONES." : "\n팩 레벨이 높을수록 속도가 빨라지고 판정 범위가 좁아집니다."), packOpeningOverlay.transform, new Vector2(0, 92), new Vector2(700, 52), 15, new Color(.7f, .65f, .58f, 1));

        if (packOpeningProtocol == 0 || packOpeningProtocol == 3)
        {
            GameObject track = CreateRoundedPanel("Horizontal Protocol", packOpeningOverlay.transform, new Vector2(0, 5), new Vector2(620, 36), new Color(.13f, .11f, .09f, 1), 9);
            float targetWidth = Mathf.Max(30f, packOpeningZoneWidth * 570f);
            CreateRoundedPanel("Perfect Zone", track.transform, new Vector2(packOpeningTarget * 285f, 0), new Vector2(targetWidth, 28), new Color(.8f, .5f, .08f, 1), 7).GetComponent<Image>().raycastTarget = false;
            packOpeningMarker = CreateRoundedPanel("Opening Marker", track.transform, new Vector2(-285, 0), new Vector2(12, 48), new Color(1f, .94f, .72f, 1), 5).GetComponent<RectTransform>();
        }
        else if (packOpeningProtocol == 1)
        {
            GameObject track = CreateRoundedPanel("Vertical Protocol", packOpeningOverlay.transform, new Vector2(0, -5), new Vector2(42, 230), new Color(.13f, .11f, .09f, 1), 9);
            CreateRoundedPanel("Perfect Zone", track.transform, new Vector2(0, packOpeningTarget * 98f), new Vector2(64, Mathf.Max(24f, packOpeningZoneWidth * 196f)), new Color(.25f, .65f, .42f, 1), 7).GetComponent<Image>().raycastTarget = false;
            packOpeningMarker = CreateRoundedPanel("Opening Marker", track.transform, Vector2.zero, new Vector2(74, 10), new Color(.82f, .94f, 1f, 1), 5).GetComponent<RectTransform>();
        }
        else if (packOpeningProtocol == 2)
        {
            GameObject ring = ImageObject("Rotary Protocol", packOpeningOverlay.transform, Color.white);
            SetRect(ring.GetComponent<RectTransform>(), new Vector2(0, -32), new Vector2(390, 170));
            ring.GetComponent<Image>().sprite = CreateSkillRingSprite((packOpeningTarget + 1f) * .5f, packOpeningZoneWidth * .5f);
            ring.GetComponent<Image>().preserveAspect = true;
            packOpeningMarker = CreateRoundedPanel("Dial Marker", packOpeningOverlay.transform, new Vector2(0, -32), new Vector2(18, 18), new Color(1f, .86f, .32f, 1), 8).GetComponent<RectTransform>();
        }
        else
        {
            GameObject laptop = CreateRoundedPanel("Mini Notebook", packOpeningOverlay.transform, new Vector2(0, -15), new Vector2(570, 190), new Color(.035f, .09f, .075f, 1), 10);
            Text("MINI NOTEBOOK // ACCESS SEQUENCE", laptop.transform, new Vector2(0, 62), new Vector2(520, 26), 16, new Color(.35f, .95f, .62f, 1));
            Text(string.Join("  ", packHackCode.ToCharArray()), laptop.transform, new Vector2(0, 12), new Vector2(520, 45), 29, Color.white);
            packOpeningStatus = Text(L("입력 대기...", "AWAITING INPUT..."), laptop.transform, new Vector2(0, -52), new Vector2(520, 28), 16, new Color(.35f, .95f, .62f, 1));
        }
        if (packOpeningProtocol != 4)
            packOpeningStatus = Text(L($"PROTOCOL {packLevel:00}  ·  속도 x{packOpeningSpeed:0.00}", $"PROTOCOL {packLevel:00}  ·  SPEED x{packOpeningSpeed:0.00}"), packOpeningOverlay.transform, new Vector2(0, -145), new Vector2(600, 24), 14, new Color(.64f, .5f, .34f, 1));
        Text(L("ESC  취소", "ESC  CANCEL"), packOpeningOverlay.transform, new Vector2(310, -205), new Vector2(140, 22), 13, new Color(.48f, .45f, .42f, 1));
        StartCoroutine(UiOpenAnimator.Play(packOpeningOverlay));
        GameplayTutorialController.ShowContext("card_pack");
    }

    private void UpdateCardPackOpening()
    {
        if (!packOpeningActive) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            packOpeningActive = false;
            Destroy(packOpeningOverlay);
            packOpeningOverlay = null;
            packOpeningMarker = null;
            GameNotificationCenter.Show("카드팩 개봉을 취소했습니다.");
            return;
        }

        if (packOpeningProtocol == 4)
        {
            char input = ReadHackDirection();
            if (input == '\0') return;
            if (packHackCode[packHackIndex] != input)
            {
                if (packOpeningStatus != null) packOpeningStatus.text = GameLanguage.IsEnglish
                    ? $"ACCESS DENIED · EXPECTED {packHackCode[packHackIndex]} / INPUT {input}"
                    : $"ACCESS DENIED · 예상 {packHackCode[packHackIndex]} / 입력 {input}";
                ResolveCardPackOpening(.12f);
                return;
            }
            packHackIndex++;
            if (packOpeningStatus != null) packOpeningStatus.text = $"ACCESS {packHackIndex}/{packHackCode.Length}  " + new string('■', packHackIndex);
            if (packHackIndex >= packHackCode.Length) ResolveCardPackOpening(1f);
            return;
        }

        if (packOpeningMarker == null) return;
        float normalized;
        if (packOpeningProtocol == 2)
        {
            packOpeningPhase = Mathf.Repeat(packOpeningPhase + Time.unscaledDeltaTime * packOpeningSpeed * .34f, 1f);
            float angle = packOpeningPhase * Mathf.PI * 2f;
            packOpeningMarker.anchoredPosition = new Vector2(Mathf.Cos(angle) * 160f, Mathf.Sin(angle) * 61f - 32f);
            if (!Input.GetKeyDown(KeyCode.Space)) return;
            float targetPhase = (packOpeningTarget + 1f) * .5f;
            float distance = Mathf.Abs(Mathf.DeltaAngle(packOpeningPhase * 360f, targetPhase * 360f)) / 180f;
            ResolveCardPackOpening(Mathf.Clamp01(1f - distance / Mathf.Max(.08f, packOpeningZoneWidth)));
            return;
        }

        if (packOpeningProtocol == 3)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                packChargeStarted = true;
                packOpeningPhase = Mathf.Min(1.05f, packOpeningPhase + Time.unscaledDeltaTime * packOpeningSpeed * .42f);
                packOpeningMarker.anchoredPosition = new Vector2(packOpeningPhase * 285f, 0);
                if (packOpeningPhase >= 1.05f) ResolveCardPackOpening(0f);
            }
            else if (packChargeStarted && Input.GetKeyUp(KeyCode.Space))
                ResolveCardPackOpening(Mathf.Clamp01(1f - Mathf.Abs(packOpeningPhase - packOpeningTarget) / Mathf.Max(.12f, packOpeningZoneWidth)));
            return;
        }

        packOpeningPhase = Mathf.Repeat(packOpeningPhase + Time.unscaledDeltaTime * packOpeningSpeed, 2f);
        normalized = packOpeningPhase <= 1f ? Mathf.Lerp(-1f, 1f, packOpeningPhase) : Mathf.Lerp(1f, -1f, packOpeningPhase - 1f);
        packOpeningMarker.anchoredPosition = packOpeningProtocol == 1 ? new Vector2(0, normalized * 98f) : new Vector2(normalized * 285f, 0);
        if (!Input.GetKeyDown(KeyCode.Space)) return;
        ResolveCardPackOpening(Mathf.Clamp01(1f - Mathf.Abs(normalized - packOpeningTarget) / Mathf.Max(.12f, packOpeningZoneWidth)));
    }

    private static string GenerateHackCode(int length)
    {
        const string keys = "WASD";
        char[] result = new char[length];
        for (int index = 0; index < length; index++) result[index] = keys[Random.Range(0, keys.Length)];
        return new string(result);
    }

    private static char ReadHackDirection()
    {
        if (Input.GetKeyDown(KeyCode.W)) return 'W';
        if (Input.GetKeyDown(KeyCode.A)) return 'A';
        if (Input.GetKeyDown(KeyCode.S)) return 'S';
        if (Input.GetKeyDown(KeyCode.D)) return 'D';
        return '\0';
    }

    private void ResolveCardPackOpening(float accuracy)
    {
        string card = ItemInventoryService.RollCard(packOpeningName, accuracy * .45f);
        ItemInventoryService.SetItem(packOpeningSlot, card);
        bool levelUp = CardProgressionService.AddExperience(CardProgressionService.PackExperience(packOpeningName));
        packOpeningActive = false;
        Destroy(packOpeningOverlay);
        packOpeningOverlay = null;
        packOpeningMarker = null;
        RefreshInventoryUi();

        GameObject reveal = CreateRoundedPanel("Card Reveal", terminalPanel.transform, Vector2.zero, new Vector2(650, 390), new Color(.075f, .045f, .025f, .995f), 18);
        reveal.transform.SetAsLastSibling();
        Text(accuracy >= .85f ? "PERFECT OPEN" : accuracy >= .5f ? "CLEAN OPEN" : "ROUGH OPEN", reveal.transform, new Vector2(0, 135), new Vector2(560, 34), 22, new Color(.94f, .66f, .25f, 1));
        GameObject cardPanel = CreateRoundedPanel("Revealed Card", reveal.transform, new Vector2(0, 25), new Vector2(430, 150), new Color(.17f, .12f, .075f, 1), 12);
        Text(card, cardPanel.transform, new Vector2(0, 18), new Vector2(390, 44), 24, Color.white);
        Text(L($"판매 가치  {ItemInventoryService.GetValue(card):N0} 노동값\n개봉 정확도  {accuracy * 100f:0}%", $"SALE VALUE  {ItemInventoryService.GetValue(card):N0} LABOR\nOPENING ACCURACY  {accuracy * 100f:0}%"), cardPanel.transform, new Vector2(0, -35), new Vector2(390, 52), 16, new Color(.78f, .67f, .52f, 1));
        Button confirm = CreateActionButton(L("확인", "CONFIRM"), reveal.transform, new Vector2(0, -135), new Vector2(160, 44), new Color(.48f, .18f, .07f, 1));
        confirm.onClick.AddListener(() =>
        {
            Destroy(terminalPanel);
            terminalPanel = null;
            BuildWorkbenchWindow();
        });
        StartCoroutine(UiOpenAnimator.Play(reveal));
        GameNotificationCenter.Success($"{packOpeningName} 개봉 · {card} 획득");
        if (levelUp) GameNotificationCenter.Success($"LEVEL UP!  LV {CardProgressionService.Level:00} · 새 카드팩이 해금되었습니다.");
    }

    private void StartLockpickSkillCheck(int boxSlot)
    {
        int lockpinSlot = FindInventoryItem("락핀");
        if (lockpinSlot < 0)
        {
            GameNotificationCenter.Error("락핀이 없습니다. 컴퓨터의 도구 상점에서 구매하십시오.");
            return;
        }

        skillBoxSlot = boxSlot;
        skillLockpinSlot = lockpinSlot;
        skillPhase = Random.value;
        int rarity = ItemInventoryService.GetLootBoxRarity(ItemInventoryService.GetItem(boxSlot));
        float rarityScale = Mathf.Lerp(1f, .38f, (rarity - 1f) / 4f);
        skillZoneWidth = Mathf.Max(.035f, UpgradeService.SkillZoneWidth * rarityScale);
        skillCheckActive = true;

        skillCheckOverlay = CreateRoundedPanel("Lockpin Skill Check", terminalPanel.transform, Vector2.zero, new Vector2(820, 470), new Color(.035f, .038f, .04f, .995f), 16);
        skillCheckOverlay.transform.SetAsLastSibling();
        Text(L("봉인 해제", "CONTAINER UNSEALING"), skillCheckOverlay.transform, new Vector2(0, 190), new Vector2(700, 38), 28, Color.white);
        Text(L("SPACE를 누른 순간 표식이 초록 구간 안에 있으면 성공합니다.", "PRESS SPACE WHILE THE MARKER IS INSIDE THE GREEN ZONE."), skillCheckOverlay.transform, new Vector2(0, 153), new Vector2(700, 25), 16, new Color(.68f, .7f, .7f, 1));

        GameObject ring = ImageObject("Elliptic Timing Track", skillCheckOverlay.transform, Color.white);
        SetRect(ring.GetComponent<RectTransform>(), new Vector2(0, 5), new Vector2(560, 250));
        ring.GetComponent<Image>().sprite = CreateSkillRingSprite(.15f, skillZoneWidth);
        ring.GetComponent<Image>().preserveAspect = true;
        ring.GetComponent<Image>().raycastTarget = false;

        GameObject marker = CreateRoundedPanel("Timing Marker", skillCheckOverlay.transform, Vector2.zero, new Vector2(24, 24), new Color(1f, .9f, .72f, 1), 10);
        marker.transform.SetAsLastSibling();
        skillMarker = marker.GetComponent<RectTransform>();
        Text(L($"상자 희귀도 {rarity}   ·   성공 구간 {skillZoneWidth * 100f:0.0}%   ·   실패 시 락핀 소모", $"BOX RARITY {rarity}   ·   SUCCESS ZONE {skillZoneWidth * 100f:0.0}%   ·   FAILURE CONSUMES LOCKPICK"), skillCheckOverlay.transform, new Vector2(0, -155), new Vector2(650, 25), 15, new Color(.72f, .5f, .38f, 1));
        Text(L("ESC  취소", "ESC  CANCEL"), skillCheckOverlay.transform, new Vector2(310, -205), new Vector2(140, 22), 13, new Color(.48f, .5f, .5f, 1));
        GameplayTutorialController.ShowContext("lockpick");
    }

    private void UpdateSkillCheck()
    {
        if (!skillCheckActive || skillMarker == null) return;
        skillPhase = Mathf.Repeat(skillPhase + Time.unscaledDeltaTime * .62f, 1f);
        float angle = skillPhase * Mathf.PI * 2f;
        skillMarker.anchoredPosition = new Vector2(Mathf.Cos(angle) * 245f, Mathf.Sin(angle) * 95f + 5f);
        float currentDistance = Mathf.Abs(Mathf.DeltaAngle(skillPhase * 360f, .15f * 360f)) / 360f;
        skillMarker.GetComponent<Image>().color = currentDistance <= skillZoneWidth * .5f ? new Color(.3f, 1f, .48f, 1) : new Color(1f, .9f, .72f, 1);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelSkillCheck();
            return;
        }
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        ResolveSkillCheck(currentDistance <= skillZoneWidth * .5f);
    }

    private void ResolveSkillCheck(bool success)
    {
        if (skillLockpinSlot >= 0) ItemInventoryService.SetItem(skillLockpinSlot, string.Empty);
        if (success)
        {
            int rarity = ItemInventoryService.GetLootBoxRarity(ItemInventoryService.GetItem(skillBoxSlot));
            int rewardPackIndex = Mathf.Clamp(CardProgressionService.Level - 1 + rarity * 2, 0, CardProgressionService.PackCount - 1);
            string reward = CardProgressionService.RollCard(CardProgressionService.GetPack(rewardPackIndex).Name, rarity * .06f);
            ItemInventoryService.SetItem(skillBoxSlot, reward);
            bool levelUp = CardProgressionService.AddExperience(12 + rarity * 9);
            GameNotificationCenter.Success($"봉인 해제 성공 · {reward} 획득");
            if (levelUp) GameNotificationCenter.Success($"LEVEL UP!  LV {CardProgressionService.Level:00}");
        }
        else
        {
            GameNotificationCenter.Error("봉인 해제 실패 · 락핀이 부러졌습니다.");
        }
        FinishSkillCheck();
    }

    private void CancelSkillCheck()
    {
        skillCheckActive = false;
        if (skillCheckOverlay != null) Destroy(skillCheckOverlay);
        skillCheckOverlay = null;
        skillMarker = null;
        GameNotificationCenter.Show("봉인 해제를 취소했습니다. 락핀은 소모되지 않았습니다.");
    }

    private void FinishSkillCheck()
    {
        skillCheckActive = false;
        skillCheckOverlay = null;
        skillMarker = null;
        RefreshInventoryUi();
        Destroy(terminalPanel);
        terminalPanel = null;
        BuildWorkbenchWindow();
    }

    private void StartDrillCheck(int boxSlot)
    {
        int toolSlot = FindInventoryItem("휴대용 드릴");
        if (toolSlot < 0)
        {
            GameNotificationCenter.Error("휴대용 드릴이 없습니다. 컴퓨터 도구 상점에서 구매하십시오.");
            return;
        }

        drillCheckActive = true;
        drillBoxSlot = boxSlot;
        drillToolSlot = toolSlot;
        drillCoolantSlot = FindInventoryItem("냉각 스프레이");
        drillProgress = 0f;
        drillHeat = .18f;
        drillCheckOverlay = CreateRoundedPanel("Drill Skill Check", terminalPanel.transform, Vector2.zero, new Vector2(820, 470), new Color(.035f, .032f, .025f, .995f), 16);
        drillCheckOverlay.transform.SetAsLastSibling();
        Text("DRILL OVERRIDE", drillCheckOverlay.transform, new Vector2(0, 185), new Vector2(700, 38), 28, new Color(1f, .66f, .18f, 1));
        Text(L("SPACE를 누르면 천공하고, 놓으면 식습니다.\n과열시키지 않고 진행도 100%를 채우십시오.", "HOLD SPACE TO DRILL; RELEASE IT TO COOL DOWN.\nREACH 100% PROGRESS WITHOUT OVERHEATING."), drillCheckOverlay.transform, new Vector2(0, 128), new Vector2(700, 52), 16, new Color(.72f, .68f, .58f, 1));

        Text(L("천공 진행", "DRILL PROGRESS"), drillCheckOverlay.transform, new Vector2(-210, 58), new Vector2(180, 24), 16, Color.white).alignment = TextAlignmentOptions.Left;
        GameObject progressTrack = CreateRoundedPanel("Drill Progress", drillCheckOverlay.transform, new Vector2(0, 22), new Vector2(500, 28), new Color(.12f, .12f, .11f, 1), 7);
        GameObject progressFill = CreateRoundedPanel("Fill", progressTrack.transform, new Vector2(-246, 0), new Vector2(0, 20), new Color(.24f, .72f, .38f, 1), 5);
        drillProgressFill = progressFill.GetComponent<RectTransform>();
        drillProgressFill.anchorMin = drillProgressFill.anchorMax = new Vector2(0, .5f);
        drillProgressFill.pivot = new Vector2(0, .5f);
        drillProgressFill.anchoredPosition = new Vector2(4, 0);

        Text(L("모터 온도", "MOTOR TEMPERATURE"), drillCheckOverlay.transform, new Vector2(-210, -42), new Vector2(180, 24), 16, Color.white).alignment = TextAlignmentOptions.Left;
        GameObject heatTrack = CreateRoundedPanel("Drill Heat", drillCheckOverlay.transform, new Vector2(0, -78), new Vector2(500, 28), new Color(.12f, .12f, .11f, 1), 7);
        GameObject safeZone = CreateRoundedPanel("Safe Heat", heatTrack.transform, new Vector2(40, 0), new Vector2(150, 20), new Color(.14f, .34f, .18f, 1), 5);
        safeZone.GetComponent<Image>().raycastTarget = false;
        GameObject heatFill = CreateRoundedPanel("Heat Fill", heatTrack.transform, new Vector2(-246, 0), new Vector2(0, 20), new Color(.95f, .5f, .12f, .9f), 5);
        drillHeatFill = heatFill.GetComponent<RectTransform>();
        drillHeatFill.anchorMin = drillHeatFill.anchorMax = new Vector2(0, .5f);
        drillHeatFill.pivot = new Vector2(0, .5f);
        drillHeatFill.anchoredPosition = new Vector2(4, 0);
        Text(L("ESC  취소 · 취소 시 드릴 보존", "ESC  CANCEL · DRILL IS PRESERVED"), drillCheckOverlay.transform, new Vector2(260, -205), new Vector2(250, 22), 13, new Color(.5f, .48f, .43f, 1));
        StartCoroutine(UiOpenAnimator.Play(drillCheckOverlay));
        GameplayTutorialController.ShowContext("drill");
    }

    private void UseHydraulicCutter(int boxSlot)
    {
        int cutterSlot = FindInventoryItem("유압 절단기");
        if (cutterSlot < 0)
        {
            GameNotificationCenter.Error("유압 절단기가 없습니다. 컴퓨터 도구 상점에서 구매하십시오.");
            return;
        }
        string box = ItemInventoryService.GetItem(boxSlot);
        int rarity = ItemInventoryService.GetLootBoxRarity(box);
        ItemInventoryService.SetItem(cutterSlot, string.Empty);
        int packIndex = Mathf.Clamp(CardProgressionService.Level + rarity * 3, 0, CardProgressionService.PackCount - 1);
        string reward = CardProgressionService.RollCard(CardProgressionService.GetPack(packIndex).Name, .42f);
        ItemInventoryService.SetItem(boxSlot, reward);
        CardProgressionService.AddExperience(25 + rarity * 12);
        RefreshInventoryUi();
        GameNotificationCenter.Success($"유압 절단 완료 · {reward} 획득");
        GameplayTutorialController.ShowContext("cutter");
        Destroy(terminalPanel);
        terminalPanel = null;
        BuildWorkbenchWindow();
    }

    private void UpdateDrillCheck()
    {
        if (!drillCheckActive) return;
        float delta = Time.unscaledDeltaTime;
        if (Input.GetKey(KeyCode.Space))
        {
            drillHeat = Mathf.Clamp01(drillHeat + delta * .34f);
            float efficiency = 1f - Mathf.Abs(drillHeat - .58f);
            drillProgress = Mathf.Clamp01(drillProgress + delta * (.11f + efficiency * .18f));
        }
        else drillHeat = Mathf.Max(0f, drillHeat - delta * .22f);

        if (drillProgressFill != null) drillProgressFill.sizeDelta = new Vector2(492f * drillProgress, 20);
        if (drillHeatFill != null)
        {
            drillHeatFill.sizeDelta = new Vector2(492f * drillHeat, 20);
            drillHeatFill.GetComponent<Image>().color = drillHeat >= .82f ? new Color(1f, .12f, .04f, 1) : new Color(.95f, .5f, .12f, .9f);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            drillCheckActive = false;
            Destroy(drillCheckOverlay);
            drillCheckOverlay = null;
            GameNotificationCenter.Show("드릴 해제를 취소했습니다.");
            return;
        }
        if (drillHeat >= 1f && drillCoolantSlot >= 0)
        {
            ItemInventoryService.SetItem(drillCoolantSlot, string.Empty);
            drillCoolantSlot = -1;
            drillHeat = .3f;
            GameNotificationCenter.Show("냉각 스프레이 자동 사용 · 과열을 1회 방지했습니다.");
        }
        else if (drillHeat >= 1f) ResolveDrillCheck(false);
        else if (drillProgress >= 1f) ResolveDrillCheck(true);
    }

    private void ResolveDrillCheck(bool success)
    {
        if (drillToolSlot >= 0) ItemInventoryService.SetItem(drillToolSlot, string.Empty);
        if (success)
        {
            int rarity = ItemInventoryService.GetLootBoxRarity(ItemInventoryService.GetItem(drillBoxSlot));
            int packIndex = Mathf.Clamp(CardProgressionService.Level + rarity * 2, 0, CardProgressionService.PackCount - 1);
            string reward = CardProgressionService.RollCard(CardProgressionService.GetPack(packIndex).Name, .28f + rarity * .05f);
            ItemInventoryService.SetItem(drillBoxSlot, reward);
            CardProgressionService.AddExperience(18 + rarity * 10);
            GameNotificationCenter.Success($"드릴 해제 성공 · {reward} 획득");
        }
        else GameNotificationCenter.Error("드릴 과열 · 도구가 파손되었습니다. 상자는 유지됩니다.");

        drillCheckActive = false;
        drillCheckOverlay = null;
        drillProgressFill = null;
        drillHeatFill = null;
        RefreshInventoryUi();
        Destroy(terminalPanel);
        terminalPanel = null;
        BuildWorkbenchWindow();
    }

    private int FindInventoryItem(string item)
    {
        for (int slot = 0; slot < slotCapacity; slot++)
            if (ItemInventoryService.GetItem(slot) == item) return slot;
        return -1;
    }

    private static Sprite CreateSkillRingSprite(float targetCenter, float targetWidth)
    {
        const int width = 256;
        const int height = 128;
        Texture2D texture = new(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        Color32[] pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float nx = (x - width * .5f) / (width * .5f);
            float ny = (y - height * .5f) / (height * .5f);
            float radius = Mathf.Sqrt(nx * nx + ny * ny);
            if (radius < .76f || radius > .94f)
            {
                pixels[y * width + x] = new Color32(0, 0, 0, 0);
                continue;
            }
            float phase = Mathf.Repeat(Mathf.Atan2(ny, nx) / (Mathf.PI * 2f), 1f);
            float difference = Mathf.Abs(Mathf.DeltaAngle(phase * 360f, targetCenter * 360f)) / 360f;
            pixels[y * width + x] = difference <= targetWidth * .5f
                ? new Color32(55, 220, 120, 255)
                : new Color32(82, 88, 92, 255);
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(.5f, .5f), 100f);
    }

    private string DeliveryList()
    {
        string result = string.Empty;
        for (int index = 0; index < ItemInventoryService.DeliveryCount; index++) result += "- " + ItemInventoryService.GetDelivery(index) + "\n";
        return result;
    }

    private void BuildShopWindow()
    {
        terminalPanel = CreateModalSurface("Shop Network", new Vector2(1120, 680), new Color(.055f, .045f, .04f, .985f));
        CreateHeader(terminalPanel.transform, "상점", "DEBT PIT  //  EXCHANGE NETWORK", new Color(.47f, .16f, .07f, 1), new Vector2(1080, 76), 287);

        GameObject categories = CreateRoundedPanel("Categories", terminalPanel.transform, new Vector2(-425, -35), new Vector2(220, 510), new Color(.09f, .07f, .06f, 1), 12);
        Text("거래 메뉴", categories.transform, new Vector2(0, 205), new Vector2(180, 28), 19, new Color(.83f, .7f, .58f, 1)).alignment = TextAlignmentOptions.Left;
        Button sellCategory = CreateCategoryButton("판매", "보유품 처분", categories.transform, new Vector2(0, 135));
        sellCategory.onClick.AddListener(ShowShopSell);
        Button marketCategory = CreateCategoryButton("야시장", "할인품 구매", categories.transform, new Vector2(0, 55));
        marketCategory.onClick.AddListener(ShowNightMarket);
        Text($"보유 노동값\n{GameSaveService.Labor:N0}", categories.transform, new Vector2(0, -158), new Vector2(165, 70), 18, Color.white);

        GameObject content = CreateRoundedPanel("Shop Content", terminalPanel.transform, new Vector2(145, -35), new Vector2(850, 510), new Color(.105f, .09f, .075f, 1), 12);
        terminalContent = content.transform;
        ShowShopSell();
    }

    private void ShowShopSell()
    {
        ClearTerminalContent();
        Text("보유품 판매", terminalContent, new Vector2(-115, 205), new Vector2(500, 38), 28, Color.white).alignment = TextAlignmentOptions.Left;
        Text("개별 판매는 최대 5회 흥정할 수 있습니다. 전체 판매는 기본가로 즉시 정산됩니다.", terminalContent, new Vector2(-30, 166), new Vector2(670, 25), 15, new Color(.63f, .57f, .5f, 1)).alignment = TextAlignmentOptions.Left;
        Button sellAll = CreateActionButton("전체 판매", terminalContent, new Vector2(300, 207), new Vector2(150, 40), new Color(.38f, .16f, .065f, 1));
        sellAll.onClick.AddListener(ShowSellAllConfirmation);
        bool hasItem = false;
        int visibleRow = 0;
        for (int index = 0; index < slotCapacity; index++)
        {
            string item = ItemInventoryService.GetItem(index);
            if (string.IsNullOrEmpty(item)) continue;
            hasItem = true;
            int slot = index;
            GameObject row = CreateRoundedPanel("Sell Item", terminalContent, new Vector2(0, 102 - visibleRow * 68), new Vector2(650, 58), new Color(.15f, .125f, .1f, 1), 8);
            Text(item, row.transform, new Vector2(-205, 8), new Vector2(205, 25), 18, Color.white).alignment = TextAlignmentOptions.Left;
            Text($"기본가 {ItemInventoryService.GetValue(item):N0}", row.transform, new Vector2(-205, -16), new Vector2(205, 20), 14, new Color(.65f, .58f, .49f, 1)).alignment = TextAlignmentOptions.Left;
            Text($"슬롯 {index + 1}", row.transform, new Vector2(55, 0), new Vector2(80, 22), 14, new Color(.55f, .5f, .45f, 1));
            Button sell = CreateActionButton("가격 제시", row.transform, new Vector2(235, 0), new Vector2(120, 38), new Color(.48f, .18f, .07f, 1));
            sell.onClick.AddListener(() => StartHaggle(slot));
            visibleRow++;
            if (visibleRow >= 5) break;
        }
        if (!hasItem)
        {
            Text("판매할 물건이 없습니다", terminalContent, new Vector2(0, 35), new Vector2(560, 38), 24, Color.white);
            Text("물건 투입구에서 배송품을 수령한 뒤 다시 방문하십시오.", terminalContent, new Vector2(0, -10), new Vector2(580, 28), 17, new Color(.58f, .53f, .47f, 1));
        }
    }

    private void ShowNightMarket()
    {
        EnsureNightMarketState();
        ClearTerminalContent();
        Text("오늘의 야시장", terminalContent, new Vector2(-105, 205), new Vector2(520, 38), 28, Color.white).alignment = TextAlignmentOptions.Left;
        nightMarketTimerLabel = Text(string.Empty, terminalContent, new Vector2(-210, 161), new Vector2(310, 25), 16, new Color(.63f, .57f, .5f, 1));
        nightMarketTimerLabel.alignment = TextAlignmentOptions.Left;
        int rerollCost = GetNightMarketRerollCost();
        Button reroll = CreateActionButton($"즉시 리롤  {rerollCost:N0}", terminalContent, new Vector2(260, 162), new Vector2(180, 36), new Color(.38f, .16f, .065f, 1));
        reroll.onClick.AddListener(RerollNightMarket);

        for (int index = 0; index < 3; index++)
        {
            int stockIndex = (int)((uint)(nightMarketSeed + index * 7) % (uint)NightMarketBoxes.Length);
            string item = NightMarketBoxes[stockIndex];
            int rarity = ItemInventoryService.GetLootBoxRarity(item);
            int price = Mathf.Max(1, Mathf.RoundToInt(ItemInventoryService.GetValue(item) * (.72f + rarity * .035f)));
            string description = rarity >= 5 ? "최상급 보안 · 극희귀 보상" : rarity >= 4 ? "군수 등급 · 고위험 고보상" : rarity >= 3 ? "내용물 미확인 · 정밀 해제" : rarity == 2 ? "보급 등급 · 중간 보상" : "낮은 등급 · 저렴한 입문 상자";
            CreateMarketCard(item, description, price, new Vector2(-215 + index * 215, 3));
        }
        GameplayTutorialController.ShowContext("night_market");
    }

    private void EndDayFromComputer()
    {
        bool paid = GameSaveService.DailyPaymentPaid;
        CloseForSystemTransition();
        DailyStoryController.BeginEndOfDay(paid);
    }

    private void ShowSellAllConfirmation()
    {
        int count = 0;
        long total = 0;
        for (int slot = 0; slot < slotCapacity; slot++)
        {
            string item = ItemInventoryService.GetItem(slot);
            if (string.IsNullOrEmpty(item)) continue;
            count++;
            total += ItemInventoryService.GetValue(item);
        }
        if (count == 0)
        {
            GameNotificationCenter.Show("판매할 물건이 없습니다.");
            return;
        }

        ClearTerminalContent();
        Text("전체 판매 확인", terminalContent, new Vector2(0, 170), new Vector2(650, 42), 29, Color.white);
        GameObject summary = CreateRoundedPanel("Sale Summary", terminalContent, new Vector2(0, 35), new Vector2(560, 190), new Color(.15f, .125f, .1f, 1), 12);
        Text($"판매 물품  {count}개\n정산 금액  {System.Math.Min(total, 1_500_000_000L):N0} 노동값", summary.transform, Vector2.zero, new Vector2(480, 110), 23, new Color(.9f, .78f, .6f, 1));
        Text("전체 판매에는 흥정 보너스가 적용되지 않습니다.", terminalContent, new Vector2(0, -78), new Vector2(580, 28), 15, new Color(.68f, .58f, .5f, 1));
        CreateActionButton("모두 판매", terminalContent, new Vector2(-105, -145), new Vector2(170, 46), new Color(.48f, .18f, .07f, 1)).onClick.AddListener(SellAllInventory);
        CreateActionButton("취소", terminalContent, new Vector2(105, -145), new Vector2(150, 46), new Color(.25f, .23f, .21f, 1)).onClick.AddListener(ShowShopSell);
    }

    private void SellAllInventory()
    {
        int count = 0;
        long total = 0;
        for (int slot = 0; slot < slotCapacity; slot++)
        {
            string item = ItemInventoryService.GetItem(slot);
            if (string.IsNullOrEmpty(item)) continue;
            total += ItemInventoryService.GetValue(item);
            ItemInventoryService.SetItem(slot, string.Empty);
            count++;
        }
        int labor = (int)System.Math.Min(1_500_000_000L, (long)GameSaveService.Labor + total);
        GameSaveService.SaveProgress(GameSaveService.Day, labor, GameSaveService.Debt);
        CardProgressionService.AddExperience(Mathf.Max(1, count * 3));
        RefreshInventoryUi();
        RefreshStatusHud();
        GameNotificationCenter.Success($"전체 판매 완료 · {count}개 · +{total:N0} 노동값");
        ShowShopSell();
    }

    private void EnsureNightMarketState()
    {
        if (nightMarketDay == 0 && PlayerPrefs.GetInt(NightMarketDayKey, -1) == GameSaveService.Day)
        {
            nightMarketDay = GameSaveService.Day;
            nightMarketSeed = PlayerPrefs.GetInt(NightMarketSeedKey, GameSaveService.Day * 97);
            nightMarketRerolls = Mathf.Max(0, PlayerPrefs.GetInt(NightMarketRerollsKey, 0));
            float remaining = Mathf.Clamp(PlayerPrefs.GetFloat(NightMarketRemainingKey, 60f), 0f, 60f);
            nightMarketRefreshAt = Time.unscaledTime + remaining;
        }

        if (nightMarketDay != GameSaveService.Day)
        {
            nightMarketDay = GameSaveService.Day;
            nightMarketRerolls = 0;
            nightMarketSeed = GameSaveService.Day * 97 + Random.Range(0, 10000);
            nightMarketRefreshAt = Time.unscaledTime + 60f;
            SaveNightMarketState();
        }
        else if (nightMarketRefreshAt <= 0f || Time.unscaledTime >= nightMarketRefreshAt)
            AdvanceNightMarketStock();
    }

    private void AdvanceNightMarketStock()
    {
        nightMarketSeed = unchecked(nightMarketSeed * 31 + 7919);
        nightMarketRefreshAt = Time.unscaledTime + 60f;
        SaveNightMarketState();
    }

    private void SaveNightMarketState()
    {
        if (!GameSaveService.HasSave || nightMarketDay <= 0) return;
        PlayerPrefs.SetInt(NightMarketDayKey, nightMarketDay);
        PlayerPrefs.SetInt(NightMarketSeedKey, nightMarketSeed);
        PlayerPrefs.SetInt(NightMarketRerollsKey, nightMarketRerolls);
        PlayerPrefs.SetFloat(NightMarketRemainingKey, Mathf.Clamp(nightMarketRefreshAt - Time.unscaledTime, 0f, 60f));
        PlayerPrefs.Save();
    }

    public static void ResetSavedNightMarket()
    {
        PlayerPrefs.DeleteKey(NightMarketDayKey);
        PlayerPrefs.DeleteKey(NightMarketSeedKey);
        PlayerPrefs.DeleteKey(NightMarketRerollsKey);
        PlayerPrefs.DeleteKey(NightMarketRemainingKey);
    }

    private int GetNightMarketRerollCost()
    {
        int baseCost = 1 + nightMarketRerolls * (nightMarketRerolls + 1);
        return GameEconomy.Scale(baseCost);
    }

    private void RerollNightMarket()
    {
        int cost = GetNightMarketRerollCost();
        if (GameSaveService.Labor < cost)
        {
            GameNotificationCenter.Error($"리롤 비용이 {cost - GameSaveService.Labor:N0} 노동값 부족합니다.");
            return;
        }
        GameSaveService.SaveProgress(GameSaveService.Day, GameSaveService.Labor - cost, GameSaveService.Debt);
        nightMarketRerolls++;
        AdvanceNightMarketStock();
        RefreshStatusHud();
        GameNotificationCenter.Success($"야시장 재고 갱신 · {cost:N0} 노동값 사용");
        ShowNightMarket();
    }

    private void StartHaggle(int slot)
    {
        haggleSlot = slot;
        haggleAttempts = 0;
        hagglePrice = ItemInventoryService.GetValue(ItemInventoryService.GetItem(slot));
        haggleMessage = L("상점이 첫 가격을 제시했습니다.", "THE SHOP HAS MADE ITS OPENING OFFER.");
        RenderHaggle();
    }

    private void RenderHaggle()
    {
        ClearTerminalContent();
        string item = ItemInventoryService.GetItem(haggleSlot);
        if (string.IsNullOrEmpty(item)) { ShowShopSell(); return; }
        Text(L("가격 협상", "PRICE NEGOTIATION"), terminalContent, new Vector2(0, 205), new Vector2(750, 38), 28, Color.white).alignment = TextAlignmentOptions.Left;
        Text(item, terminalContent, new Vector2(-195, 155), new Vector2(360, 30), 21, new Color(.86f, .72f, .55f, 1)).alignment = TextAlignmentOptions.Left;
        GameObject offer = CreateRoundedPanel("Offer", terminalContent, new Vector2(-170, 30), new Vector2(390, 190), new Color(.15f, .125f, .1f, 1), 10);
        Text(L("현재 제시가", "CURRENT OFFER"), offer.transform, new Vector2(0, 52), new Vector2(300, 24), 16, new Color(.62f, .56f, .49f, 1));
        Text(hagglePrice.ToString("N0"), offer.transform, new Vector2(0, 5), new Vector2(300, 55), 38, Color.white);
        Text(L("노동값", "LABOR"), offer.transform, new Vector2(0, -48), new Vector2(300, 22), 15, new Color(.62f, .56f, .49f, 1));
        Text(haggleMessage, terminalContent, new Vector2(210, 75), new Vector2(300, 55), 17, Color.white);
        Text(L($"남은 흥정  {5 - haggleAttempts} / 5", $"ATTEMPTS LEFT  {5 - haggleAttempts} / 5"), terminalContent, new Vector2(210, 25), new Vector2(300, 24), 16, new Color(.68f, .61f, .53f, 1));
        for (int index = 0; index < 5; index++)
        {
            GameObject pip = ImageObject("Attempt", terminalContent, index < haggleAttempts ? new Color(.48f, .18f, .07f, 1) : new Color(.23f, .2f, .17f, 1));
            SetRect(pip.GetComponent<RectTransform>(), new Vector2(154 + index * 30, -13), new Vector2(20, 8));
        }
        Button haggle = CreateActionButton(L("흥정하기", "NEGOTIATE"), terminalContent, new Vector2(180, -85), new Vector2(170, 48), new Color(.48f, .18f, .07f, 1));
        haggle.interactable = haggleAttempts < 5;
        haggle.onClick.AddListener(TryHaggle);
        Button accept = CreateActionButton(L("이 가격에 판매", "ACCEPT OFFER"), terminalContent, new Vector2(-35, -160), new Vector2(200, 46), new Color(.22f, .36f, .22f, 1));
        accept.onClick.AddListener(FinalizeSale);
        CreateActionButton(L("취소", "CANCEL"), terminalContent, new Vector2(200, -160), new Vector2(140, 46), new Color(.25f, .23f, .21f, 1)).onClick.AddListener(ShowShopSell);
    }

    private void TryHaggle()
    {
        if (haggleAttempts >= 5) return;
        haggleAttempts++;
        if (Random.value < UpgradeService.HaggleChance)
        {
            int increase = Mathf.Max(1, Mathf.RoundToInt(hagglePrice * Random.Range(UpgradeService.HaggleMinIncrease, UpgradeService.HaggleMaxIncrease)));
            hagglePrice += increase;
            haggleMessage = L($"흥정 성공. 제시가가 {increase:N0} 올랐습니다.", $"NEGOTIATION SUCCESS. OFFER INCREASED BY {increase:N0}.");
            GameNotificationCenter.Success($"흥정 성공 · 제시가 +{increase:N0}");
        }
        else
        {
            haggleMessage = haggleAttempts >= 5
                ? L("더 이상의 흥정은 거부되었습니다.", "THE SHOP REFUSES FURTHER NEGOTIATION.")
                : L("상점이 제안을 거절했습니다.", "THE SHOP REJECTED YOUR OFFER.");
            GameNotificationCenter.Error(haggleAttempts >= 5 ? "흥정 기회를 모두 사용했습니다." : "흥정에 실패했습니다.");
        }
        RenderHaggle();
    }

    private void FinalizeSale()
    {
        string item = ItemInventoryService.GetItem(haggleSlot);
        if (string.IsNullOrEmpty(item)) { ShowShopSell(); return; }
        ItemInventoryService.SetItem(haggleSlot, string.Empty);
        int labor = (int)System.Math.Min(1_500_000_000L, (long)GameSaveService.Labor + hagglePrice);
        GameSaveService.SaveProgress(GameSaveService.Day, labor, GameSaveService.Debt);
        bool levelUp = CardProgressionService.AddExperience(5);
        GameNotificationCenter.Success($"{item} 판매 완료 · +{hagglePrice:N0} 노동값");
        if (levelUp) GameNotificationCenter.Success($"LEVEL UP!  LV {CardProgressionService.Level:00} · 새 카드가 해금되었습니다.");
        RefreshInventoryUi();
        RefreshStatusHud();
        ClearTerminalContent();
        Text(L("거래 완료", "TRANSACTION COMPLETE"), terminalContent, new Vector2(0, 80), new Vector2(520, 42), 30, new Color(.45f, .82f, .52f, 1));
        Text(L($"{item}\n+ {hagglePrice:N0} 노동값", $"{GameLanguage.Item(item)}\n+ {hagglePrice:N0} LABOR"), terminalContent, new Vector2(0, 5), new Vector2(500, 70), 21, Color.white);
        CreateActionButton(L("판매 목록", "BACK TO SELL LIST"), terminalContent, new Vector2(0, -105), new Vector2(190, 46), new Color(.48f, .18f, .07f, 1)).onClick.AddListener(ShowShopSell);
    }

    private void ClearTerminalContent()
    {
        if (terminalContent == null) return;
        for (int index = terminalContent.childCount - 1; index >= 0; index--) Destroy(terminalContent.GetChild(index).gameObject);
    }

    private void BuildDeveloperBadge()
    {
        if (developerBadge != null || hudRoot == null) return;
        developerBadge = CreateRoundedPanel("Developer Badge", hudRoot.transform, new Vector2(805, 485), new Vector2(230, 42), new Color(.09f, .11f, .11f, .94f), 8);
        developerBadge.GetComponent<Image>().raycastTarget = false;
        Text("DEV MODE  ·  F1", developerBadge.transform, Vector2.zero, new Vector2(200, 28), 16, new Color(.35f, .9f, .58f, 1));
    }

    private void OpenDeveloperPanel()
    {
        if (!developerMode || developerPanel != null || hudRoot == null) return;
        if (terminalPanel != null) CloseTerminal();

        developerPanel = CreateRoundedPanel("Developer Panel", hudRoot.transform, Vector2.zero, new Vector2(820, 650), new Color(.075f, .08f, .085f, .99f), 16);
        Shadow shadow = developerPanel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, .7f);
        shadow.effectDistance = new Vector2(10, -10);

        GameObject header = CreateRoundedPanel("Developer Header", developerPanel.transform, new Vector2(0, 279), new Vector2(780, 72), new Color(.12f, .25f, .2f, 1), 11);
        TextMeshProUGUI title = Text("개발자 모드", header.transform, new Vector2(-230, 10), new Vector2(280, 30), 25, Color.white);
        title.alignment = TextAlignmentOptions.Left;
        TextMeshProUGUI subtitle = Text("suxghui  //  RUNTIME TEST CONTROLS", header.transform, new Vector2(-145, -18), new Vector2(450, 18), 12, new Color(1, 1, 1, .6f));
        subtitle.alignment = TextAlignmentOptions.Left;
        Button close = CreateActionButton("X", header.transform, new Vector2(350, 0), new Vector2(42, 42), new Color(0, 0, 0, .3f));
        close.onClick.AddListener(CloseDeveloperPanel);

        Text("경제 / 시간", developerPanel.transform, new Vector2(-285, 213), new Vector2(190, 28), 18, new Color(.55f, .9f, .68f, 1)).alignment = TextAlignmentOptions.Left;
        CreateDeveloperButton("노동값 +1", new Vector2(-250, 155), () => DeveloperAddLabor(1));
        CreateDeveloperButton("노동값 +100,000", new Vector2(0, 155), () => DeveloperAddLabor(100_000));
        CreateDeveloperButton("노동값 +10,000,000", new Vector2(250, 155), () => DeveloperAddLabor(10_000_000));
        CreateDeveloperButton("DAY +1", new Vector2(-250, 85), DeveloperAdvanceDay);
        CreateDeveloperButton("오늘 납부 완료", new Vector2(0, 85), DeveloperCompletePayment);
        CreateDeveloperButton("자유 기금 완료", new Vector2(250, 85), DeveloperCompleteFreedomFund);

        Text("아이템 / 진행", developerPanel.transform, new Vector2(-285, 27), new Vector2(190, 28), 18, new Color(.55f, .9f, .68f, 1)).alignment = TextAlignmentOptions.Left;
        CreateDeveloperButton("고급 카드팩 지급", new Vector2(-250, -30), () => DeveloperGrantItem("고급 카드팩"));
        CreateDeveloperButton("황금 계약 카드 지급", new Vector2(0, -30), () => DeveloperGrantItem("황금 계약 카드"));
        CreateDeveloperButton("미납 처형 테스트", new Vector2(250, -30), DeveloperTestFailure);
        CreateDeveloperButton("속도 낮추기", new Vector2(-250, -100), () => DeveloperChangeGameSpeed(-1));
        CreateDeveloperButton($"게임 속도  x{Time.timeScale:0.#}", new Vector2(0, -100), DeveloperResetGameSpeed);
        CreateDeveloperButton("속도 올리기", new Vector2(250, -100), () => DeveloperChangeGameSpeed(1));
        CreateDeveloperButton("엔딩 넘기기", new Vector2(-125, -165), DeveloperSkipEnding);
        CreateDeveloperButton("패널 닫기  [F1]", new Vector2(125, -165), CloseDeveloperPanel);

        GameObject status = CreateRoundedPanel("Developer Status", developerPanel.transform, new Vector2(0, -255), new Vector2(700, 64), new Color(.105f, .115f, .12f, 1), 10);
        Text($"DAY {GameSaveService.Day:00}   |   노동값 {GameSaveService.Labor:N0}   |   인벤토리 {ItemInventoryService.UsedSlots(slotCapacity)}/{slotCapacity}   |   SPEED x{Time.timeScale:0.#}", status.transform, Vector2.zero, new Vector2(650, 34), 15, new Color(.82f, .84f, .84f, 1));

        IsTerminalOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(UiOpenAnimator.Play(developerPanel));
    }

    private void CreateDeveloperButton(string label, Vector2 position, UnityEngine.Events.UnityAction callback)
    {
        Button button = CreateActionButton(label, developerPanel.transform, position, new Vector2(220, 48), new Color(.14f, .3f, .24f, 1));
        button.onClick.AddListener(callback);
    }

    private void CloseDeveloperPanel()
    {
        if (developerPanel != null) Destroy(developerPanel);
        developerPanel = null;
        IsTerminalOpen = terminalPanel != null;
        if (!IsTerminalOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void RefreshDeveloperPanel()
    {
        if (developerPanel == null) return;
        Destroy(developerPanel);
        developerPanel = null;
        OpenDeveloperPanel();
    }

    private void DeveloperAddLabor(int amount)
    {
        int labor = (int)System.Math.Min(1_500_000_000L, (long)GameSaveService.Labor + amount);
        GameSaveService.SaveProgress(GameSaveService.Day, labor, GameSaveService.Debt);
        RefreshStatusHud();
        GameNotificationCenter.Success($"개발자 지급: +{amount:N0} 노동값");
        RefreshDeveloperPanel();
    }

    private void DeveloperAdvanceDay()
    {
        GameSaveService.MarkDailyPaymentPaid();
        CloseDeveloperPanel();
        DailyStoryController.BeginEndOfDay(true);
    }

    private void DeveloperCompletePayment()
    {
        GameSaveService.MarkDailyPaymentPaid();
        GameNotificationCenter.Success("오늘의 노동값 납부를 완료 처리했습니다.");
        RefreshDeveloperPanel();
    }

    private void DeveloperChangeGameSpeed(int direction)
    {
        int current = 0;
        float closest = float.MaxValue;
        for (int index = 0; index < DeveloperGameSpeeds.Length; index++)
        {
            float difference = Mathf.Abs(DeveloperGameSpeeds[index] - Time.timeScale);
            if (difference >= closest) continue;
            closest = difference;
            current = index;
        }
        current = Mathf.Clamp(current + direction, 0, DeveloperGameSpeeds.Length - 1);
        Time.timeScale = DeveloperGameSpeeds[current];
        GameNotificationCenter.Success($"게임 속도를 x{Time.timeScale:0.#}(으)로 변경했습니다.");
        RefreshDeveloperPanel();
    }

    private void DeveloperResetGameSpeed()
    {
        Time.timeScale = 1f;
        GameNotificationCenter.Show("게임 속도를 x1로 초기화했습니다.");
        RefreshDeveloperPanel();
    }

    private void DeveloperCompleteFreedomFund()
    {
        GameSaveService.SetFreedomFund(GameEconomy.FreedomGoal);
        RefreshStatusHud();
        GameNotificationCenter.Success("자유 기금을 최대치로 설정했습니다.");
        RefreshDeveloperPanel();
    }

    private void DeveloperGrantItem(string item)
    {
        if (ItemInventoryService.TryAdd(item, slotCapacity))
        {
            RefreshInventoryUi();
            GameNotificationCenter.Success($"{item}을(를) 인벤토리에 지급했습니다.");
        }
        else
        {
            ItemInventoryService.QueueDelivery(item);
            GameNotificationCenter.Error($"인벤토리가 가득 차 {item}을(를) 물건 투입구로 보냈습니다.");
        }
        RefreshDeveloperPanel();
    }

    private static void DeveloperTestNotifications()
    {
        GameNotificationCenter.Show("기본 알림입니다.");
        GameNotificationCenter.Success("구매 또는 성공 알림입니다.");
        GameNotificationCenter.Error("실패 또는 노동값 부족 알림입니다.");
        GameNotificationCenter.Show("네 번째 알림은 대기열에서 표시됩니다.");
        GameNotificationCenter.Success("다섯 번째 알림도 순서대로 표시됩니다.");
    }

    private void DeveloperTestFailure()
    {
        CloseDeveloperPanel();
        DailyStoryController.BeginEndOfDay(false);
    }

    private void DeveloperSkipEnding()
    {
        GameSaveService.SetFreedomFund(GameEconomy.FreedomGoal);
        GameSaveService.MarkIntroSeen();
        GameNotificationCenter.Success("엔딩 조건을 완료하고 타이틀로 이동합니다.");
        StartCoroutine(DeveloperReturnToTitle());
    }

    private System.Collections.IEnumerator DeveloperReturnToTitle()
    {
        yield return new WaitForSecondsRealtime(.8f);
        CloseDeveloperPanel();
        SceneFade.Load("Title");
    }

    private GameObject CreateModalSurface(string name, Vector2 size, Color color)
    {
        GameObject panel = CreateRoundedPanel(name, transform.GetComponentInChildren<Canvas>().transform, Vector2.zero, size, color, 14);
        Shadow shadow = panel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, .65f);
        shadow.effectDistance = new Vector2(10, -10);
        return panel;
    }

    private void CreateHeader(Transform parent, string title, string subtitle, Color color, Vector2 size, float y)
    {
        GameObject header = CreateRoundedPanel("Header", parent, new Vector2(0, y), size, color, 10);
        TextMeshProUGUI headerTitle = Text(title, header.transform, new Vector2(-300, 9), new Vector2(230, 30), 24, Color.white);
        headerTitle.alignment = TextAlignmentOptions.Left;
        headerTitle.textWrappingMode = TextWrappingModes.NoWrap;
        TextMeshProUGUI headerSubtitle = Text(subtitle, header.transform, new Vector2(-160, -18), new Vector2(500, 20), 13, new Color(1, 1, 1, .62f));
        headerSubtitle.alignment = TextAlignmentOptions.Left;
        headerSubtitle.textWrappingMode = TextWrappingModes.NoWrap;
        Button close = CreateActionButton("X", header.transform, new Vector2(size.x * .5f - 35, 0), new Vector2(38, 38), new Color(0, 0, 0, .28f));
        close.onClick.AddListener(CloseTerminal);
    }

    private static GameObject CreateRoundedPanel(string name, Transform parent, Vector2 position, Vector2 size, Color color, int radius)
    {
        GameObject panel = ImageObject(name, parent, color);
        SetRect(panel.GetComponent<RectTransform>(), position, size);
        SetRoundedSprite(panel.GetComponent<Image>(), ref roundedPanelSprite, radius, Mathf.Max(8, radius));
        return panel;
    }

    private Button CreateCategoryButton(string title, string subtitle, Transform parent, Vector2 position)
    {
        GameObject item = CreateRoundedPanel(title + " Category", parent, position, new Vector2(180, 64), new Color(.15f, .12f, .1f, 1), 8);
        Button button = item.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.18f, 1.12f, 1.06f, 1);
        colors.pressedColor = new Color(.78f, .72f, .68f, 1);
        button.colors = colors;
        TextMeshProUGUI titleLabel = Text(title, item.transform, new Vector2(0, 9), new Vector2(150, 24), 18, Color.white);
        titleLabel.alignment = TextAlignmentOptions.Left;
        titleLabel.textWrappingMode = TextWrappingModes.NoWrap;
        Text(subtitle, item.transform, new Vector2(0, -14), new Vector2(150, 18), 12, new Color(.62f, .56f, .5f, 1)).alignment = TextAlignmentOptions.Left;
        return button;
    }

    private static Button CreateActionButton(string label, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject item = CreateRoundedPanel(label + " Button", parent, position, size, color, 8);
        Button button = item.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.12f, 1.08f, 1);
        colors.pressedColor = new Color(.72f, .68f, .64f, 1);
        colors.disabledColor = new Color(.35f, .35f, .35f, .65f);
        button.colors = colors;
        TextMeshProUGUI buttonLabel = Text(label, item.transform, Vector2.zero, size - new Vector2(12, 8), 17, Color.white);
        buttonLabel.textWrappingMode = TextWrappingModes.NoWrap;
        if (label.Length >= 13) buttonLabel.fontSize = 13;
        else if (label.Length >= 9) buttonLabel.fontSize = 15;
        return button;
    }

    private void CreateMarketCard(string item, string description, int price, Vector2 position)
    {
        GameObject card = CreateRoundedPanel(item, terminalContent, position, new Vector2(195, 255), new Color(.15f, .125f, .1f, 1), 10);
        GameObject icon = CreateRoundedPanel("Item Mark", card.transform, new Vector2(0, 62), new Vector2(60, 60), new Color(.28f, .2f, .14f, 1), 10);
        Text(MarketIcon(item), icon.transform, Vector2.zero, new Vector2(48, 42), 24, new Color(.87f, .72f, .52f, 1));
        Text(item, card.transform, new Vector2(0, 15), new Vector2(175, 28), 18, Color.white);
        Text(description, card.transform, new Vector2(0, -18), new Vector2(170, 36), 13, new Color(.62f, .56f, .5f, 1));
        Text(L(price.ToString("N0") + " 노동값", price.ToString("N0") + " LABOR"), card.transform, new Vector2(0, -58), new Vector2(170, 25), 16, new Color(.87f, .72f, .52f, 1));
        Button buy = CreateActionButton(L("구매", "BUY"), card.transform, new Vector2(0, -96), new Vector2(140, 36), new Color(.48f, .18f, .07f, 1));
        buy.onClick.AddListener(() => BuyMarketItem(item, price));
    }

    private static string MarketIcon(string item)
    {
        if (!GameLanguage.IsEnglish) return string.IsNullOrEmpty(item) ? "?" : item.Substring(0, 1);
        return item switch
        {
            "녹슨 가챠 상자" => "RU",
            "보급 가챠 상자" => "SU",
            "봉인된 상자" => "SB",
            "군수 가챠 상자" => "MI",
            "검은 금고" => "BV",
            _ => "BX"
        };
    }

    private void BuyMarketItem(string item, int price)
    {
        if (GameSaveService.Labor < price)
        {
            GameNotificationCenter.Error($"구매 실패: 노동값이 {price - GameSaveService.Labor:N0} 부족합니다.");
            return;
        }
        GameSaveService.SaveProgress(GameSaveService.Day, GameSaveService.Labor - price, GameSaveService.Debt);
        ItemInventoryService.QueueDelivery(item);
        GameNotificationCenter.Success($"{item} 구매 완료 · 물건 투입구로 배송했습니다.");
        RefreshStatusHud();
        ShowNightMarket();
    }

    private static Button CreateWindowButton(string label, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = ImageObject(label + " Button", parent, new Color(.75f, .75f, .75f, 1));
        SetRect(buttonObject.GetComponent<RectTransform>(), position, size);
        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(.85f, .85f, .85f, 1f);
        colors.pressedColor = new Color(.55f, .55f, .55f, 1f);
        button.colors = colors;
        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = new Color(.18f, .18f, .18f, 1f);
        outline.effectDistance = new Vector2(1, -1);
        TextMeshProUGUI buttonLabel = Text(label, buttonObject.transform, Vector2.zero, size - new Vector2(8, 4), 17, Color.black);
        buttonLabel.textWrappingMode = TextWrappingModes.NoWrap;
        if (label.Length >= 13) buttonLabel.fontSize = 13;
        else if (label.Length >= 9) buttonLabel.fontSize = 15;
        return button;
    }

    private static TMP_InputField CreateNumericInput(string placeholderText, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject inputObject = ImageObject("Numeric Input", parent, Color.white);
        SetRect(inputObject.GetComponent<RectTransform>(), position, size);
        Outline outline = inputObject.AddComponent<Outline>();
        outline.effectColor = new Color(.18f, .18f, .18f, 1f);
        outline.effectDistance = new Vector2(2, -2);

        GameObject viewport = new("Text Area", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(inputObject.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(14, 5);
        viewportRect.offsetMax = new Vector2(-14, -5);

        TextMeshProUGUI valueText = Text(string.Empty, viewport.transform, Vector2.zero, size - new Vector2(28, 10), 20, Color.black);
        valueText.alignment = TextAlignmentOptions.MidlineLeft;
        valueText.textWrappingMode = TextWrappingModes.NoWrap;
        TextMeshProUGUI placeholder = Text(placeholderText, viewport.transform, Vector2.zero, size - new Vector2(28, 10), 18, new Color(.42f, .42f, .42f, 1));
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.textWrappingMode = TextWrappingModes.NoWrap;

        TMP_InputField input = inputObject.AddComponent<TMP_InputField>();
        input.targetGraphic = inputObject.GetComponent<Image>();
        input.textViewport = viewportRect;
        input.textComponent = valueText;
        input.placeholder = placeholder;
        input.contentType = TMP_InputField.ContentType.IntegerNumber;
        input.characterValidation = TMP_InputField.CharacterValidation.Integer;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterLimit = 10;
        input.caretColor = Color.black;
        input.selectionColor = new Color(.2f, .35f, .7f, .35f);
        return input;
    }

    private static TMP_InputField CreateTextInput(string placeholderText, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject inputObject = ImageObject("Text Input", parent, Color.white);
        SetRect(inputObject.GetComponent<RectTransform>(), position, size);
        Outline outline = inputObject.AddComponent<Outline>();
        outline.effectColor = new Color(.18f, .18f, .18f, 1f);
        outline.effectDistance = new Vector2(2, -2);

        GameObject viewport = new("Text Area", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(inputObject.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(14, 5);
        viewportRect.offsetMax = new Vector2(-14, -5);

        TextMeshProUGUI valueText = Text(string.Empty, viewport.transform, Vector2.zero, size - new Vector2(28, 10), 17, Color.black);
        valueText.alignment = TextAlignmentOptions.MidlineLeft;
        valueText.textWrappingMode = TextWrappingModes.NoWrap;
        TextMeshProUGUI placeholder = Text(placeholderText, viewport.transform, Vector2.zero, size - new Vector2(28, 10), 16, new Color(.42f, .42f, .42f, 1));
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.textWrappingMode = TextWrappingModes.NoWrap;

        TMP_InputField input = inputObject.AddComponent<TMP_InputField>();
        input.targetGraphic = inputObject.GetComponent<Image>();
        input.textViewport = viewportRect;
        input.textComponent = valueText;
        input.placeholder = placeholder;
        input.contentType = TMP_InputField.ContentType.Standard;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterLimit = 1024;
        input.caretColor = Color.black;
        input.selectionColor = new Color(.2f, .35f, .7f, .35f);
        return input;
    }

    private static void CreateBevel(Transform parent, Vector2 position, Vector2 size, bool inset)
    {
        GameObject bevel = ImageObject("Bevel", parent, Color.white);
        SetRect(bevel.GetComponent<RectTransform>(), position, size);
        bevel.transform.SetAsFirstSibling();
    }

    private static string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(Mathf.Max(0, seconds) / 60f);
        int remaining = Mathf.FloorToInt(Mathf.Max(0, seconds) % 60f);
        return $"{minutes:00}:{remaining:00}";
    }

    private static string L(string korean, string english) => GameLanguage.IsEnglish ? english : korean;

    private void CloseTerminal()
    {
        packOpeningActive = false;
        packOpeningOverlay = null;
        packOpeningMarker = null;
        packOpeningSlot = -1;
        drillCheckActive = false;
        drillCheckOverlay = null;
        drillProgressFill = null;
        drillHeatFill = null;
        drillBoxSlot = -1;
        drillToolSlot = -1;
        drillCoolantSlot = -1;
        skillCheckActive = false;
        skillCheckOverlay = null;
        skillMarker = null;
        skillBoxSlot = -1;
        skillLockpinSlot = -1;
        Destroy(terminalPanel);
        terminalPanel = null;
        terminalContent = null;
        computerTimeLabel = null;
        nightMarketTimerLabel = null;
        activeTerminalTarget = null;
        IsTerminalOpen = developerPanel != null;
        if (!IsTerminalOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnDestroy()
    {
        SaveNightMarketState();
        if (pausePanel != null && Time.timeScale <= 0f)
            Time.timeScale = 1f;
        IsTerminalOpen = false;
    }

    private static GameObject ImageObject(string name, Transform parent, Color color)
    {
        GameObject item = new(name, typeof(RectTransform), typeof(Image));
        item.transform.SetParent(parent, false);
        item.GetComponent<Image>().color = color;
        return item;
    }

    private static void SetRoundedSprite(Image image, ref Sprite sprite, int radius, int border)
    {
        sprite ??= CreateRoundedSprite(radius, border);
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
    }

    private static Sprite CreateRoundedSprite(int radius, int border)
    {
        const int size = 64;
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
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
    }

    private static TextMeshProUGUI Text(string name, Transform parent, Vector2 pos, Vector2 size, float fontSize, Color color)
    {
        GameObject item = new(name, typeof(RectTransform));
        item.transform.SetParent(parent, false);
        SetRect(item.GetComponent<RectTransform>(), pos, size);
        TextMeshProUGUI text = item.AddComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = GameLanguage.Item(name);
        text.raycastTarget = false;
        text.enableAutoSizing = false;
        text.fontStyle = FontStyles.Normal;
        text.characterSpacing = 0f;
        text.wordSpacing = 0f;
        text.lineSpacing = 2f;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 pos, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
