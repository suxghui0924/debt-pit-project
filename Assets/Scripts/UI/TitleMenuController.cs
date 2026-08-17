using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class TitleMenuController : MonoBehaviour
{
    private static readonly Color NormalTextColor = new(0.94f, 0.92f, 0.86f, 1f);
    private static readonly Color SelectedTextColor = new(0.84f, 0.08f, 0.06f, 1f);
    private static readonly Color PanelColor = new(0.035f, 0.03f, 0.025f, 0.96f);

    private readonly Button[] menuButtons = new Button[4];
    private readonly TMP_Text[] menuLabels = new TMP_Text[4];
    private readonly Image[] menuIndicators = new Image[4];

    private GameObject confirmationOverlay;
    private int selectedIndex;
    private Transform titleCamera;
    private Vector3 titleCameraBasePosition;
    private Quaternion titleCameraBaseRotation;
    private float titleCameraBreathTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void EnsureInstalled()
    {
        if (SceneManager.GetActiveScene().name != "Title") return;
        if (FindFirstObjectByType<TitleMenuController>() != null) return;

        GameObject controller = new("Title Menu Controller");
        controller.AddComponent<TitleMenuController>();
    }

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Camera sceneCamera = Camera.main;
        if (sceneCamera != null)
        {
            titleCamera = sceneCamera.transform;
            titleCameraBasePosition = titleCamera.localPosition;
            titleCameraBaseRotation = titleCamera.localRotation;
        }
        GameObject verticalList = GameObject.Find("Vertical List");
        if (verticalList != null && verticalList.TryGetComponent<LayoutGroup>(out var layout))
            layout.enabled = false;

        menuButtons[0] = FindButton("Play Button");
        menuButtons[1] = CreateLoadButton(FindButton("Setting Button"));
        menuButtons[2] = FindButton("Setting Button");
        menuButtons[3] = FindButton("Exit Button");

        for (int index = 0; index < menuButtons.Length; index++)
        {
            if (menuButtons[index] == null) continue;

            int capturedIndex = index;
            menuLabels[index] = menuButtons[index].GetComponentInChildren<TMP_Text>(true);
            StyleMenuButton(menuButtons[index], index);
            AddPointerEvent(menuButtons[index].gameObject, EventTriggerType.PointerEnter, () =>
            {
                if (menuButtons[capturedIndex] != null && menuButtons[capturedIndex].interactable)
                    SelectMenu(capturedIndex);
            });
            menuButtons[index].onClick.AddListener(() => SelectMenu(capturedIndex));
        }

        RefreshLocalizedText();
        GameLanguage.Changed += RefreshLocalizedText;

        if (menuButtons[3] != null)
            menuButtons[3].onClick.AddListener(ShowExitConfirmation);
        if (menuButtons[2] != null)
            menuButtons[2].onClick.AddListener(ShowSettings);
        if (menuButtons[0] != null)
            menuButtons[0].onClick.AddListener(StartNewGame);
        if (menuButtons[1] != null)
            menuButtons[1].onClick.AddListener(LoadGame);

        SelectMenu(0);
        RefreshLoadButton();
    }

    private void Update()
    {
        AnimateTitleCamera();
        if (confirmationOverlay != null && Input.GetKeyDown(KeyCode.Escape))
            HideExitConfirmation();
    }

    private void AnimateTitleCamera()
    {
        if (titleCamera == null) return;
        titleCameraBreathTime += Time.unscaledDeltaTime;
        float time = titleCameraBreathTime;
        float horizontal = Mathf.Sin(time * .42f) * .045f + Mathf.Sin(time * .91f + 1.4f) * .012f;
        float vertical = Mathf.Sin(time * .56f + .8f) * .027f + Mathf.Sin(time * .19f) * .009f;
        float depth = Mathf.Sin(time * .31f + 2.1f) * .018f;
        titleCamera.localPosition = titleCameraBasePosition + new Vector3(horizontal, vertical, depth);
        titleCamera.localRotation = titleCameraBaseRotation * Quaternion.Euler(
            Mathf.Sin(time * .37f + .6f) * .13f,
            Mathf.Sin(time * .29f + 2f) * .18f,
            Mathf.Sin(time * .47f) * .11f);
    }

    private void OnDestroy()
    {
        GameLanguage.Changed -= RefreshLocalizedText;
        if (titleCamera == null) return;
        titleCamera.localPosition = titleCameraBasePosition;
        titleCamera.localRotation = titleCameraBaseRotation;
    }

    private void RefreshLocalizedText()
    {
        if (menuLabels[0] != null) menuLabels[0].text = GameLanguage.Text("new_game");
        if (menuLabels[1] != null) menuLabels[1].text = GameLanguage.Text("load_game");
        if (menuLabels[2] != null) menuLabels[2].text = GameLanguage.Text("settings");
        if (menuLabels[3] != null) menuLabels[3].text = GameLanguage.Text("exit");
    }

    private static Button FindButton(string objectName)
    {
        GameObject buttonObject = GameObject.Find(objectName);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    private void StyleMenuButton(Button button, int index)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.sizeDelta = new Vector2(250f, 72f);
        rect.anchoredPosition = new Vector2(56f, 338f - index * 82f);

        Image background = button.GetComponent<Image>();
        background.color = Color.clear;

        TMP_Text label = menuLabels[index];
        if (label != null)
        {
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label.alignment = TextAlignmentOptions.Left;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.fontSize = 42f;
            label.fontStyle = FontStyles.Bold;
            label.color = NormalTextColor;
        }

        GameObject indicatorObject = new("Selection Indicator", typeof(RectTransform), typeof(Image));
        indicatorObject.transform.SetParent(button.transform, false);
        Image indicator = indicatorObject.GetComponent<Image>();
        indicator.color = SelectedTextColor;
        RectTransform indicatorRect = indicator.rectTransform;
        indicatorRect.anchorMin = new Vector2(0f, 0f);
        indicatorRect.anchorMax = new Vector2(1f, 0f);
        indicatorRect.pivot = new Vector2(0.5f, 0f);
        indicatorRect.sizeDelta = new Vector2(0f, 4f);
        indicatorRect.anchoredPosition = Vector2.zero;
        menuIndicators[index] = indicator;
    }

    private void SelectMenu(int index)
    {
        if (index < 0 || index >= menuButtons.Length || menuButtons[index] == null || !menuButtons[index].interactable)
            return;
        selectedIndex = index;
        for (int i = 0; i < menuLabels.Length; i++)
        {
            if (menuLabels[i] != null)
            {
                bool disabled = menuButtons[i] != null && !menuButtons[i].interactable;
                menuLabels[i].color = disabled
                    ? new Color(.45f, .42f, .38f, 1f)
                    : i == selectedIndex ? SelectedTextColor : NormalTextColor;
            }
            if (menuIndicators[i] != null)
                menuIndicators[i].enabled = i == selectedIndex && menuButtons[i] != null && menuButtons[i].interactable;
        }
    }

    private void ShowExitConfirmation()
    {
        if (confirmationOverlay != null) return;

        GameObject buttonsCanvasObject = GameObject.Find("ButtonsCanvas");
        Canvas canvas = buttonsCanvasObject != null
            ? buttonsCanvasObject.GetComponent<Canvas>()
            : FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        confirmationOverlay = new GameObject("Exit Confirmation", typeof(RectTransform), typeof(Image));
        confirmationOverlay.transform.SetParent(canvas.transform, false);
        RectTransform overlayRect = confirmationOverlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        Image overlay = confirmationOverlay.GetComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.72f);

        GameObject panel = CreateUiObject("Panel", confirmationOverlay.transform, new Vector2(640f, 300f));
        StartCoroutine(UiOpenAnimator.Play(panel));
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = PanelColor;

        TMP_FontAsset font = menuLabels[0] != null ? menuLabels[0].font : TMP_Settings.defaultFontAsset;
        CreateText(GameLanguage.Text("exit_question"), panel.transform, font, 32f, new Vector2(0f, 52f), new Vector2(560f, 70f), NormalTextColor);

        Button confirmButton = CreateModalButton(GameLanguage.Text("confirm"), panel.transform, font, new Vector2(-120f, -72f));
        Button cancelButton = CreateModalButton(GameLanguage.Text("cancel"), panel.transform, font, new Vector2(120f, -72f));
        confirmButton.onClick.AddListener(QuitGame);
        cancelButton.onClick.AddListener(HideExitConfirmation);
        AddPointerEvent(overlay.gameObject, EventTriggerType.PointerClick, HideExitConfirmation);

        EventSystem.current?.SetSelectedGameObject(cancelButton.gameObject);
    }

    private void ShowSettings()
    {
        GameObject buttonsCanvasObject = GameObject.Find("ButtonsCanvas");
        Canvas canvas = buttonsCanvasObject != null ? buttonsCanvasObject.GetComponent<Canvas>() : FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        TitleSettingsPanel.Show(canvas, menuLabels[0] != null ? menuLabels[0].font : TMP_Settings.defaultFontAsset);
    }

    private static void StartNewGame()
    {
        GameSaveService.StartNewGame();
        SceneFade.Load("Loading");
    }

    private static void LoadGame()
    {
        if (GameSaveService.HasSave)
            SceneFade.Load("Loading");
    }

    private Button CreateLoadButton(Button template)
    {
        if (template == null) return null;
        GameObject loadButtonObject = Instantiate(template.gameObject, template.transform.parent);
        loadButtonObject.name = "Load Game Button";
        return loadButtonObject.GetComponent<Button>();
    }

    private void RefreshLoadButton()
    {
        if (menuButtons[1] == null) return;
        menuButtons[1].interactable = GameSaveService.HasSave;
        if (menuLabels[1] != null)
            menuLabels[1].color = GameSaveService.HasSave ? NormalTextColor : new Color(.45f, .42f, .38f, 1f);
        if (!GameSaveService.HasSave && menuIndicators[1] != null)
            menuIndicators[1].enabled = false;
        if (!GameSaveService.HasSave && selectedIndex == 1)
            SelectMenu(0);
    }

    private static GameObject CreateUiObject(string objectName, Transform parent, Vector2 size)
    {
        GameObject uiObject = new(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        RectTransform rect = uiObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        return uiObject;
    }

    private static void CreateText(string text, Transform parent, TMP_FontAsset font, float size, Vector2 position, Vector2 dimensions, Color color)
    {
        GameObject textObject = CreateUiObject("Text", parent, dimensions);
        textObject.GetComponent<RectTransform>().anchoredPosition = position;
        TMP_Text label = textObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        label.text = text;
        label.fontSize = size;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
    }

    private static Button CreateModalButton(string labelText, Transform parent, TMP_FontAsset font, Vector2 position)
    {
        GameObject buttonObject = CreateUiObject(labelText + " Button", parent, new Vector2(200f, 64f));
        buttonObject.GetComponent<RectTransform>().anchoredPosition = position;
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.16f, 0.14f, 0.12f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.55f, 0.5f, 1f);
        colors.pressedColor = new Color(0.8f, 0.25f, 0.2f, 1f);
        button.colors = colors;
        CreateText(labelText, buttonObject.transform, font, 30f, Vector2.zero, new Vector2(180f, 52f), NormalTextColor);
        return button;
    }

    private static void AddPointerEvent(GameObject target, EventTriggerType type, Action callback)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>() ?? target.AddComponent<EventTrigger>();
        trigger.triggers ??= new System.Collections.Generic.List<EventTrigger.Entry>();
        EventTrigger.Entry entry = new() { eventID = type };
        entry.callback.AddListener(_ => callback());
        trigger.triggers.Add(entry);
    }

    private void HideExitConfirmation()
    {
        if (confirmationOverlay == null) return;
        Destroy(confirmationOverlay);
        confirmationOverlay = null;
        EventSystem.current?.SetSelectedGameObject(menuButtons[selectedIndex].gameObject);
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
