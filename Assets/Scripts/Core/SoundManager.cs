using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SoundManager : Singleton<SoundManager>
{
    private const string TitleMusicPath = "Audio/Music/title_ambient";
    private const string LoadingMusicPath = "Audio/Music/loading_ambient";
    private const string FacilityMusicPath = "Audio/Music/facility_ambient";
    private const string UiHoverPath = "Audio/Sfx/ui_hover";
    private const string UiClickPath = "Audio/Sfx/ui_click";
    private const string WindowOpenPath = "Audio/Sfx/window_open";
    private const string WindowClosePath = "Audio/Sfx/window_close";
    private const string SuccessPath = "Audio/Sfx/success";
    private const string ErrorPath = "Audio/Sfx/error";
    private const string PurchasePath = "Audio/Sfx/purchase";
    private const string DeliveryPath = "Audio/Sfx/delivery";
    private const string TogglePath = "Audio/Sfx/toggle";
    private const string DialogueTypePath = "Audio/Sfx/dialogue_type";
    private const string DialogueAdvancePath = "Audio/Sfx/dialogue_advance";
    private const string DialoguePagePath = "Audio/Sfx/dialogue_page";
    private const string BootstrapPowerOnPath = "Audio/Sfx/bootstrap_power_on";
    private const string BootstrapComputerLoopPath = "Audio/Sfx/bootstrap_computer_loop";
    private const string BootstrapPostBeepPath = "Audio/Sfx/bootstrap_post_beep";
    private static readonly string[] MetalFootstepPaths =
    {
        "Audio/Sfx/Footsteps/metal_step_01",
        "Audio/Sfx/Footsteps/metal_step_02",
        "Audio/Sfx/Footsteps/metal_step_03",
        "Audio/Sfx/Footsteps/metal_step_04",
        "Audio/Sfx/Footsteps/metal_step_05",
        "Audio/Sfx/Footsteps/metal_step_06",
        "Audio/Sfx/Footsteps/metal_step_07",
        "Audio/Sfx/Footsteps/metal_step_08"
    };

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource bootstrapLoopSource;
    [SerializeField, Min(1)] private int initialSfxSources = 8;

    private readonly List<AudioSource> sfxSources = new();
    private Coroutine musicFade;
    private Coroutine bootstrapRoutine;
    private float musicVolume = 1f;
    private float masterVolume = 1f;
    private float musicTrackGain = 1f;
    private float sfxVolume = 1f;
    private float nextHoverTime;
    private const float BootstrapLoopGain = .25f;
    private int metalFootstepIndex = -1;

    private AudioClip titleMusic;
    private AudioClip loadingMusic;
    private AudioClip facilityMusic;
    private AudioClip uiHover;
    private AudioClip uiClick;
    private AudioClip windowOpen;
    private AudioClip windowClose;
    private AudioClip success;
    private AudioClip error;
    private AudioClip purchase;
    private AudioClip delivery;
    private AudioClip toggle;
    private AudioClip dialogueType;
    private AudioClip dialogueAdvance;
    private AudioClip dialoguePage;
    private AudioClip bootstrapPowerOn;
    private AudioClip bootstrapComputerLoop;
    private AudioClip bootstrapPostBeep;
    private AudioClip[] metalFootsteps;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstalled()
    {
        if (Instance == null)
            new GameObject("Sound Manager").AddComponent<SoundManager>();
    }

    protected override void Awake()
    {
        base.Awake();
        if (!IsPrimaryInstance) return;

        masterVolume = Mathf.Clamp(PlayerPrefs.GetFloat("Settings.MasterVolume", 1f), 0f, GameSettings.MaxVolume);
        musicVolume = Mathf.Clamp(PlayerPrefs.GetFloat("Settings.BgmVolume", 1f), 0f, GameSettings.MaxVolume);
        sfxVolume = Mathf.Clamp(PlayerPrefs.GetFloat("Settings.SfxVolume", 1f), 0f, GameSettings.MaxVolume);
        LoadAudioLibrary();

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;

        if (bootstrapLoopSource == null)
            bootstrapLoopSource = gameObject.AddComponent<AudioSource>();
        bootstrapLoopSource.loop = true;
        bootstrapLoopSource.playOnAwake = false;
        bootstrapLoopSource.spatialBlend = 0f;
        bootstrapLoopSource.volume = SfxOutputVolume(BootstrapLoopGain);

        for (int i = 0; i < initialSfxSources; i++)
            sfxSources.Add(CreateSfxSource());

        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(BindUiAudioRoutine());
    }

    protected override void OnDestroy()
    {
        if (IsPrimaryInstance)
            SceneManager.sceneLoaded -= OnSceneLoaded;
        StopBootstrapAudio();
        base.OnDestroy();
    }

    public void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || sfxVolume <= 0f) return;

        AudioSource source = GetAvailableSfxSource();
        source.pitch = Mathf.Clamp(pitch, .25f, 3f);
        source.volume = SfxOutputVolume(volume);
        source.clip = clip;
        source.gameObject.SetActive(true);
        source.Play();
        StartCoroutine(ReleaseAfterPlayback(source, clip.length / Mathf.Max(.01f, Mathf.Abs(source.pitch))));
    }

    public void PlayMusic(AudioClip clip, float volume = 1f, float fadeDuration = .5f)
    {
        musicTrackGain = Mathf.Clamp01(volume);

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            if (musicFade != null) StopCoroutine(musicFade);
            musicFade = StartCoroutine(FadeMusicVolume(MusicOutputVolume(), fadeDuration));
            return;
        }

        if (musicFade != null) StopCoroutine(musicFade);
        musicFade = StartCoroutine(FadeMusic(clip, MusicOutputVolume(), fadeDuration));
    }

    public void PlayUiHover()
    {
        if (Time.unscaledTime < nextHoverTime) return;
        nextHoverTime = Time.unscaledTime + .045f;
        PlaySfx(uiHover, .34f, Random.Range(.98f, 1.025f));
    }

    public void PlayUiClick() => PlaySfx(uiClick, .72f, Random.Range(.97f, 1.025f));
    public void PlayWindowOpen() => PlaySfx(windowOpen, .5f);
    public void PlayWindowClose() => PlaySfx(windowClose, .5f);
    public void PlaySuccess() => PlaySfx(success, .76f);
    public void PlayError() => PlaySfx(error, .78f);
    public void PlayPurchase() => PlaySfx(purchase, .72f);
    public void PlayDelivery() => PlaySfx(delivery, .72f);
    public void PlayToggle() => PlaySfx(toggle, .62f);
    public void PlayDialogueType() => PlaySfx(dialogueType, .24f, Random.Range(.94f, 1.08f));
    public void PlayDialogueAdvance() => PlaySfx(dialogueAdvance, .52f, Random.Range(.98f, 1.03f));
    public void PlayDialoguePage() => PlaySfx(dialoguePage, .38f);
    public void PlayMetalFootstep()
    {
        if (metalFootsteps == null || metalFootsteps.Length == 0) return;
        metalFootstepIndex = (metalFootstepIndex + Random.Range(1, metalFootsteps.Length)) % metalFootsteps.Length;
        PlaySfx(metalFootsteps[metalFootstepIndex], .5f, Random.Range(.94f, 1.065f));
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp(volume, 0f, GameSettings.MaxVolume);
        if (musicSource != null)
            musicSource.volume = MusicOutputVolume();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp(volume, 0f, GameSettings.MaxVolume);
        if (bootstrapLoopSource != null)
            bootstrapLoopSource.volume = SfxOutputVolume(BootstrapLoopGain);
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp(volume, 0f, GameSettings.MaxVolume);
        if (musicSource != null) musicSource.volume = MusicOutputVolume();
        if (bootstrapLoopSource != null) bootstrapLoopSource.volume = SfxOutputVolume(BootstrapLoopGain);
    }

    private float MusicOutputVolume() => Mathf.Clamp01(musicTrackGain * musicVolume * masterVolume);
    private float SfxOutputVolume(float clipGain) => Mathf.Clamp01(Mathf.Clamp01(clipGain) * sfxVolume * masterVolume);

    private void LoadAudioLibrary()
    {
        titleMusic = Resources.Load<AudioClip>(TitleMusicPath);
        loadingMusic = Resources.Load<AudioClip>(LoadingMusicPath);
        facilityMusic = Resources.Load<AudioClip>(FacilityMusicPath);
        uiHover = Resources.Load<AudioClip>(UiHoverPath);
        uiClick = Resources.Load<AudioClip>(UiClickPath);
        windowOpen = Resources.Load<AudioClip>(WindowOpenPath);
        windowClose = Resources.Load<AudioClip>(WindowClosePath);
        success = Resources.Load<AudioClip>(SuccessPath);
        error = Resources.Load<AudioClip>(ErrorPath);
        purchase = Resources.Load<AudioClip>(PurchasePath);
        delivery = Resources.Load<AudioClip>(DeliveryPath);
        toggle = Resources.Load<AudioClip>(TogglePath);
        dialogueType = Resources.Load<AudioClip>(DialogueTypePath);
        dialogueAdvance = Resources.Load<AudioClip>(DialogueAdvancePath);
        dialoguePage = Resources.Load<AudioClip>(DialoguePagePath);
        bootstrapPowerOn = Resources.Load<AudioClip>(BootstrapPowerOnPath);
        bootstrapComputerLoop = Resources.Load<AudioClip>(BootstrapComputerLoopPath);
        bootstrapPostBeep = Resources.Load<AudioClip>(BootstrapPostBeepPath);
        metalFootsteps = new AudioClip[MetalFootstepPaths.Length];
        for (int index = 0; index < MetalFootstepPaths.Length; index++)
            metalFootsteps[index] = Resources.Load<AudioClip>(MetalFootstepPaths[index]);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Bootstrap") StartBootstrapAudio();
        else StopBootstrapAudio();
        ApplySceneMusic(scene.name);
        BindUiAudioNow();
    }

    private void ApplySceneMusic(string sceneName)
    {
        switch (sceneName)
        {
            case "Title":
                PlayMusic(titleMusic, .34f, .8f);
                break;
            case "Loading":
                PlayMusic(loadingMusic, .31f, .65f);
                break;
            case "Map":
                PlayMusic(facilityMusic, .27f, .8f);
                break;
            default:
                PlayMusic(null, 0f, .35f);
                break;
        }
    }

    private void StartBootstrapAudio()
    {
        StopBootstrapAudio();
        bootstrapRoutine = StartCoroutine(BootstrapAudioSequence());
    }

    private void StopBootstrapAudio()
    {
        if (bootstrapRoutine != null)
        {
            StopCoroutine(bootstrapRoutine);
            bootstrapRoutine = null;
        }
        if (bootstrapLoopSource != null)
        {
            bootstrapLoopSource.Stop();
            bootstrapLoopSource.clip = null;
        }
    }

    private IEnumerator BootstrapAudioSequence()
    {
        PlaySfx(bootstrapPowerOn, .82f, .96f);
        yield return new WaitForSecondsRealtime(.62f);
        PlaySfx(bootstrapPostBeep, .48f);
        yield return new WaitForSecondsRealtime(.58f);

        if (SceneManager.GetActiveScene().name != "Bootstrap" || bootstrapLoopSource == null)
            yield break;

        bootstrapLoopSource.clip = bootstrapComputerLoop;
        bootstrapLoopSource.volume = SfxOutputVolume(BootstrapLoopGain);
        if (bootstrapComputerLoop != null) bootstrapLoopSource.Play();
        bootstrapRoutine = null;
    }

    private IEnumerator BindUiAudioRoutine()
    {
        WaitForSecondsRealtime delay = new(.35f);
        while (true)
        {
            BindUiAudioNow();
            yield return delay;
        }
    }

    private static void BindUiAudioNow()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            if (button != null && button.GetComponent<UiAudioFeedback>() == null)
                button.gameObject.AddComponent<UiAudioFeedback>();
        }
    }

    private AudioSource GetAvailableSfxSource()
    {
        foreach (AudioSource candidate in sfxSources)
        {
            if (!candidate.isPlaying) return candidate;
        }

        AudioSource newSource = CreateSfxSource();
        sfxSources.Add(newSource);
        return newSource;
    }

    private AudioSource CreateSfxSource()
    {
        GameObject sourceObject = new("SFX Source");
        sourceObject.transform.SetParent(transform);
        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        return source;
    }

    private IEnumerator ReleaseAfterPlayback(AudioSource source, float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (!source.isPlaying)
        {
            source.clip = null;
            source.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeMusic(AudioClip nextClip, float targetVolume, float duration)
    {
        duration = Mathf.Max(.01f, duration);
        float startVolume = musicSource.volume;
        for (float time = 0; time < duration; time += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = nextClip;
        if (nextClip != null) musicSource.Play();

        for (float time = 0; time < duration; time += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, targetVolume, time / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        musicFade = null;
    }

    private IEnumerator FadeMusicVolume(float targetVolume, float duration)
    {
        duration = Mathf.Max(.01f, duration);
        float startVolume = musicSource.volume;
        for (float time = 0; time < duration; time += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        musicFade = null;
    }
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class UiAudioFeedback : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private Button button;

    private void Awake() => button = GetComponent<Button>();

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.IsInteractable())
            SoundManager.Instance?.PlayUiHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || button == null || !button.IsInteractable())
            return;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        string action = label == null ? string.Empty : label.text.Trim();
        if (action is "X" or "닫기" or "Close" or "취소" or "Cancel" or "컴퓨터 끄기")
            SoundManager.Instance?.PlayWindowClose();
        else
            SoundManager.Instance?.PlayUiClick();
    }
}
