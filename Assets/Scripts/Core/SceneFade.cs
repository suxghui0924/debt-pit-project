using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SceneFade : MonoBehaviour
{
    private static SceneFade instance;
    private Image overlay;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        if (instance != null) return;
        GameObject root = new("Scene Fade", typeof(SceneFade));
        instance = root.GetComponent<SceneFade>();
        DontDestroyOnLoad(root);
        instance.Build();
        instance.StartCoroutine(instance.Fade(1f, 0f, .45f));
    }

    public static void Load(string sceneName)
    {
        if (instance == null) { SceneManager.LoadScene(sceneName); return; }
        instance.StartCoroutine(instance.LoadRoutine(sceneName));
    }

    private void Build()
    {
        GameObject canvas = new("Fade Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.transform.SetParent(transform, false);
        Canvas fadeCanvas = canvas.GetComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.overrideSorting = true;
        fadeCanvas.sortingOrder = 1000;
        overlay = new GameObject("Black", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        overlay.transform.SetParent(canvas.transform, false);
        overlay.color = Color.black;
        // The fade is purely visual. A transparent Image still receives UI raycasts,
        // which otherwise makes every title button appear visible but unclickable.
        overlay.raycastTarget = false;
        RectTransform rect = overlay.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        yield return Fade(0f, 1f, .35f);
        SceneManager.LoadScene(sceneName);
        yield return null;
        yield return Fade(1f, 0f, .45f);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        for (float time = 0; time < duration; time += Time.unscaledDeltaTime)
        {
            overlay.color = new Color(0, 0, 0, Mathf.Lerp(from, to, time / duration));
            yield return null;
        }
        overlay.color = new Color(0, 0, 0, to);
    }
}
