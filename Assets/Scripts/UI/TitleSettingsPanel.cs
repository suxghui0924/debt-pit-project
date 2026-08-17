using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TitleSettingsPanel : MonoBehaviour
{
    private static readonly Color Ink = new(0.94f, 0.92f, 0.86f, 1f);
    private static readonly Color Red = new(0.84f, 0.08f, 0.06f, 1f);
    private TMP_FontAsset font;
    private TextMeshProUGUI resolutionValue;
    private int resolutionIndex;

    public static void Show(Canvas canvas, TMP_FontAsset font)
    {
        if (FindFirstObjectByType<TitleSettingsPanel>() != null) return;
        var root = new GameObject("Settings Panel", typeof(RectTransform), typeof(Image), typeof(TitleSettingsPanel));
        root.transform.SetParent(canvas.transform, false);
        var rect = root.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        root.GetComponent<Image>().color = new Color(0, 0, 0, 0.82f);
        root.GetComponent<TitleSettingsPanel>().Build(font);
    }

    private void Build(TMP_FontAsset selectedFont)
    {
        font = selectedFont;
        var panel = Box("Settings Window", transform, new Vector2(880, 740), Vector2.zero, new Color(0.05f, 0.04f, 0.035f, 0.98f));
        StartCoroutine(UiOpenAnimator.Play(panel));
        Text(GameLanguage.Text("settings"), panel.transform, 52, new Vector2(0, 280), new Vector2(660, 64), Red);
        Resolution[] availableResolutions = Screen.resolutions;
        resolutionIndex = Array.FindIndex(availableResolutions, r => r.width == Screen.width && r.height == Screen.height);
        if (resolutionIndex < 0 && availableResolutions.Length > 0) resolutionIndex = availableResolutions.Length - 1;
        resolutionValue = ResolutionRow(panel.transform, new Vector2(0, 150));
        ValueRow(panel.transform, GameLanguage.Text("master"), new Vector2(0, 75), d => GameSettings.MasterVolume += d, Percent(GameSettings.MasterVolume), 1f, .01f);
        ValueRow(panel.transform, GameLanguage.Text("bgm"), new Vector2(0, 0), d => GameSettings.BgmVolume += d, Percent(GameSettings.BgmVolume), 1f, .01f);
        ValueRow(panel.transform, GameLanguage.Text("sfx"), new Vector2(0, -75), d => GameSettings.SfxVolume += d, Percent(GameSettings.SfxVolume), 1f, .01f);
        ValueRow(panel.transform, GameLanguage.Text("sensitivity"), new Vector2(0, -150), d => GameSettings.MouseSensitivity += d, GameSettings.MouseSensitivity.ToString("0.0"), 1f, .1f);
        LanguageRow(panel.transform, new Vector2(0, -220));
        var fullscreen = Button(GameLanguage.Text("fullscreen") + ": " + (Screen.fullScreen ? GameLanguage.Text("on") : GameLanguage.Text("off")), panel.transform, new Vector2(0, -275), new Vector2(300, 48));
        fullscreen.onClick.AddListener(() => { Screen.fullScreen = !Screen.fullScreen; fullscreen.GetComponentInChildren<TMP_Text>().text = GameLanguage.Text("fullscreen") + ": " + (Screen.fullScreen ? GameLanguage.Text("on") : GameLanguage.Text("off")); });
        var close = Button(GameLanguage.Text("close"), panel.transform, new Vector2(0, -330), new Vector2(180, 48));
        close.onClick.AddListener(() => { GameSettings.Save(); Destroy(gameObject); });
    }

    private TextMeshProUGUI ResolutionRow(Transform parent, Vector2 position)
    {
        Text(GameLanguage.Text("resolution"), parent, 28, position + Vector2.left * 250, new Vector2(250, 50), Ink, TextAlignmentOptions.Left);
        var left = Button("◀", parent, position + Vector2.right * -55, new Vector2(52, 52));
        var value = Text(ResolutionName(), parent, 24, position + Vector2.right * 105, new Vector2(250, 50), Ink);
        var right = Button("▶", parent, position + Vector2.right * 265, new Vector2(52, 52));
        left.onClick.AddListener(() => { ChangeResolution(-1); value.text = ResolutionName(); });
        right.onClick.AddListener(() => { ChangeResolution(1); value.text = ResolutionName(); });
        return value;
    }

    private void ValueRow(Transform parent, string name, Vector2 position, Action<float> change, string initial, float coarseStep, float fineStep)
    {
        Text(name, parent, 28, position + Vector2.left * 250, new Vector2(250, 50), Ink, TextAlignmentOptions.Left);
        var coarseDown = Button("<<", parent, position + Vector2.right * -95, new Vector2(50, 52));
        var fineDown = Button("‹", parent, position + Vector2.right * -35, new Vector2(50, 52));
        var value = Text(initial, parent, 25, position + Vector2.right * 80, new Vector2(140, 50), Ink);
        var fineUp = Button("›", parent, position + Vector2.right * 195, new Vector2(50, 52));
        var coarseUp = Button(">>", parent, position + Vector2.right * 255, new Vector2(50, 52));
        Action refresh = () => value.text = name == GameLanguage.Text("sensitivity") ? GameSettings.MouseSensitivity.ToString("0.0") : Percent(name == GameLanguage.Text("master") ? GameSettings.MasterVolume : name == GameLanguage.Text("bgm") ? GameSettings.BgmVolume : GameSettings.SfxVolume);
        coarseDown.onClick.AddListener(() => { change(-coarseStep); refresh(); });
        fineDown.onClick.AddListener(() => { change(-fineStep); refresh(); });
        fineUp.onClick.AddListener(() => { change(fineStep); refresh(); });
        coarseUp.onClick.AddListener(() => { change(coarseStep); refresh(); });
    }

    private void LanguageRow(Transform parent, Vector2 position)
    {
        Text(GameLanguage.Text("language"), parent, 28, position + Vector2.left * 250, new Vector2(250, 50), Ink, TextAlignmentOptions.Left);
        var button = Button(GameLanguage.IsEnglish ? GameLanguage.Text("english") : GameLanguage.Text("korean"), parent, position + Vector2.right * 90, new Vector2(250, 48));
        button.onClick.AddListener(() => { GameLanguage.IsEnglish = !GameLanguage.IsEnglish; GameSettings.Save(); Reopen(); });
    }

    private void Reopen()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        Build(font);
    }
    private void ChangeResolution(int direction) { var resolutions = Screen.resolutions; if (resolutions == null || resolutions.Length == 0) return; resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1); resolutionIndex = (resolutionIndex + direction + resolutions.Length) % resolutions.Length; var r = resolutions[resolutionIndex]; Screen.SetResolution(r.width, r.height, Screen.fullScreenMode, r.refreshRateRatio); }
    private string ResolutionName() { var resolutions = Screen.resolutions; if (resolutions == null || resolutions.Length == 0) return Screen.width + " x " + Screen.height; resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1); var r = resolutions[resolutionIndex]; return r.width + " x " + r.height; }
    private static string Percent(float value) => Mathf.RoundToInt(value * 100) + "%";
    private GameObject Box(string name, Transform parent, Vector2 size, Vector2 pos, Color color) { var o = new GameObject(name, typeof(RectTransform), typeof(Image)); o.transform.SetParent(parent, false); var r=o.GetComponent<RectTransform>(); r.anchorMin=r.anchorMax=new Vector2(.5f,.5f); r.sizeDelta=size; r.anchoredPosition=pos; o.GetComponent<Image>().color=color; return o; }
    private TextMeshProUGUI Text(string value, Transform parent, float size, Vector2 pos, Vector2 dimensions, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center) { var o=new GameObject("Text",typeof(RectTransform)); o.transform.SetParent(parent,false); var r=o.GetComponent<RectTransform>(); r.anchorMin=r.anchorMax=new Vector2(.5f,.5f); r.sizeDelta=dimensions; r.anchoredPosition=pos; var t=o.AddComponent<TextMeshProUGUI>(); t.font=font; t.text=value; t.fontSize=size; t.fontStyle=FontStyles.Bold; t.color=color; t.alignment=alignment; t.textWrappingMode=TextWrappingModes.NoWrap; return t; }
    private Button Button(string label, Transform parent, Vector2 pos, Vector2 size) { var o=Box(label,parent,size,pos,new Color(.16f,.13f,.11f,1)); var b=o.AddComponent<Button>(); var c=b.colors; c.highlightedColor=new Color(1,.55f,.5f,1); c.pressedColor=new Color(.8f,.25f,.2f,1); b.colors=c; Text(label,o.transform,24,Vector2.zero,size-Vector2.one*8,Ink); return b; }
}
