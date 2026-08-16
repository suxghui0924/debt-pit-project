using System.Collections;
using UnityEngine;

public static class UiOpenAnimator
{
    public static IEnumerator Play(GameObject target, bool dramatic = false)
    {
        if (target == null) yield break;
        SoundManager.Instance?.PlayWindowOpen();
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null) group = target.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        Transform transform = target.transform;
        float startScale = dramatic ? .08f : .76f;
        float peakScale = dramatic ? 1.055f : 1.025f;
        float firstDuration = dramatic ? .32f : .18f;
        transform.localScale = Vector3.one * startScale;
        transform.localRotation = Quaternion.Euler(0f, 0f, dramatic ? -2.5f : -.6f);

        for (float time = 0f; time < firstDuration; time += Time.unscaledDeltaTime)
        {
            if (target == null) yield break;
            float t = EaseOutBack(Mathf.Clamp01(time / firstDuration));
            transform.localScale = Vector3.one * Mathf.LerpUnclamped(startScale, peakScale, t);
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(dramatic ? -2.5f : -.6f, 0f, Mathf.Clamp01(t)));
            group.alpha = Mathf.Clamp01(time / (firstDuration * .72f));
            yield return null;
        }

        const float settleDuration = .1f;
        for (float time = 0f; time < settleDuration; time += Time.unscaledDeltaTime)
        {
            if (target == null) yield break;
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(time / settleDuration), 3f);
            transform.localScale = Vector3.one * Mathf.Lerp(peakScale, 1f, t);
            group.alpha = 1f;
            yield return null;
        }

        if (target == null) yield break;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        group.alpha = 1f;
        group.blocksRaycasts = true;
    }

    private static float EaseOutBack(float value)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float shifted = value - 1f;
        return 1f + c3 * shifted * shifted * shifted + c1 * shifted * shifted;
    }
}
