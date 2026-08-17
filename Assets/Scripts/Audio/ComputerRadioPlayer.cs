using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public sealed class ComputerRadioPlayer : MonoBehaviour
{
    private const string VolumeKey = "Radio.Volume";
    private const string LoopKey = "Radio.Loop";
    private const string UrlKey = "Save.Radio.Url";
    private const string PositionKey = "Save.Radio.Position";
    private const string PlayingKey = "Save.Radio.Playing";
    private AudioSource source;
    private UnityWebRequest activeRequest;
    private Coroutine loadRoutine;
    private float nextStateSaveAt;
    private float restorePosition = -1f;
    private bool playbackRequested;

    public string Status { get; private set; } = "정지됨";
    public bool HasError { get; private set; }
    public float VolumePercent => PlayerPrefs.GetFloat(VolumeKey, 1f) * 100f;
    public bool LoopEnabled => PlayerPrefs.GetInt(LoopKey, 1) != 0;
    public string SavedUrl => PlayerPrefs.GetString(UrlKey, string.Empty);
    public string PlaybackPosition => source != null && source.clip != null
        ? $"{FormatTime(source.time)} / {FormatTime(source.clip.length)}"
        : "00:00 / 00:00";

    public static ComputerRadioPlayer GetOrCreate(Transform computer)
    {
        ComputerRadioPlayer player = computer.GetComponentInChildren<ComputerRadioPlayer>(true);
        if (player != null)
        {
            AudioSource existingSource = player.GetComponent<AudioSource>();
            if (existingSource == null) existingSource = player.gameObject.AddComponent<AudioSource>();
            player.ConfigureSource(existingSource);
            return player;
        }

        GameObject radio = new("Computer Spatial Radio", typeof(AudioSource));
        radio.transform.SetParent(computer, false);
        return radio.AddComponent<ComputerRadioPlayer>();
    }

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        ConfigureSource(source);
    }

    private void ConfigureSource(AudioSource configuredSource)
    {
        source = configuredSource;
        source.playOnAwake = false;
        source.loop = LoopEnabled;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 1.2f;
        source.maxDistance = 14f;
        source.dopplerLevel = 0f;
        source.spread = 25f;
        ApplyVolume();
    }

    private void Update() => ApplyVolume();

    private void Start()
    {
        if (!GameSaveService.HasSave || PlayerPrefs.GetInt(PlayingKey, 0) == 0) return;
        string savedUrl = PlayerPrefs.GetString(UrlKey, string.Empty);
        if (string.IsNullOrWhiteSpace(savedUrl)) return;
        restorePosition = Mathf.Max(0f, PlayerPrefs.GetFloat(PositionKey, 0f));
        Play(savedUrl);
    }

    public void SetVolumePercent(float percent)
    {
        PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp(percent, 0f, 1000f) / 100f);
        PlayerPrefs.Save();
        ApplyVolume();
    }

    public void Play(string url, Action completed = null)
    {
        url = url?.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeFile))
        {
            Fail(L("올바른 MP3/OGG/WAV 주소 또는 로컬 파일 경로를 입력하십시오.", "ENTER A VALID MP3/OGG/WAV URL OR LOCAL FILE PATH."));
            completed?.Invoke();
            return;
        }

        string requestUrl = uri.AbsoluteUri;
        if (uri.IsFile)
        {
            string localPath = Uri.UnescapeDataString(uri.LocalPath);
            if (!File.Exists(localPath))
            {
                Fail(L("로컬 음원 파일을 찾을 수 없습니다.", "LOCAL AUDIO FILE NOT FOUND."));
                completed?.Invoke();
                return;
            }

            // UnityWebRequest가 파일명의 대괄호/한글/공백을 URL 문법으로 오인하지 않도록 다시 이스케이프한다.
            requestUrl = new Uri(localPath).AbsoluteUri;
        }

        string host = uri.Host.ToLowerInvariant();
        if (host.Contains("youtube.com") || host.Contains("youtu.be") || host.Contains("music.youtube.com"))
        {
            Fail(L("YouTube 링크는 직접 오디오로 재생할 수 없습니다. 공식 임베디드 플레이어가 필요합니다.", "YOUTUBE URLS REQUIRE THE OFFICIAL EMBEDDED PLAYER AND CANNOT BE PLAYED AS DIRECT AUDIO."));
            completed?.Invoke();
            return;
        }

        string path = Uri.UnescapeDataString(uri.IsFile ? uri.LocalPath : uri.AbsolutePath).ToLowerInvariant();
        AudioType type = path.EndsWith(".ogg") ? AudioType.OGGVORBIS
            : path.EndsWith(".wav") ? AudioType.WAV
            : AudioType.MPEG;

        if (loadRoutine != null) StopCoroutine(loadRoutine);
        activeRequest?.Abort();
        activeRequest?.Dispose();
        loadRoutine = StartCoroutine(LoadAndPlay(requestUrl, type, uri.IsFile, completed));
    }

    public void Stop()
    {
        if (loadRoutine != null) StopCoroutine(loadRoutine);
        loadRoutine = null;
        activeRequest?.Abort();
        activeRequest?.Dispose();
        activeRequest = null;
        source.Stop();
        playbackRequested = false;
        Status = L("정지됨", "STOPPED");
        HasError = false;
        SavePlaybackState(false);
    }

    public void Seek(float seconds)
    {
        if (source == null || source.clip == null) return;
        source.time = Mathf.Clamp(source.time + seconds, 0f, Mathf.Max(0f, source.clip.length - .05f));
        SavePlaybackState(source.isPlaying);
    }

    public void ToggleLoop()
    {
        bool enabled = !LoopEnabled;
        PlayerPrefs.SetInt(LoopKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
        if (source != null) source.loop = enabled;
    }

    private IEnumerator LoadAndPlay(string url, AudioType type, bool isLocalFile, Action completed)
    {
        Status = L("방송 연결 중...", "CONNECTING...");
        HasError = false;
        activeRequest = UnityWebRequestMultimedia.GetAudioClip(url, type);
        if (activeRequest.downloadHandler is DownloadHandlerAudioClip handler)
        {
            // 로컬 MP3에서 streamAudio를 켜면 일부 Windows/Unity 조합에서 요청이 끝나지 않는다.
            handler.streamAudio = !isLocalFile;
            handler.compressed = true;
        }
        if (!isLocalFile) activeRequest.timeout = 30;

        yield return activeRequest.SendWebRequest();
        if (activeRequest.result != UnityWebRequest.Result.Success)
        {
            Fail(L("방송 연결 실패: ", "CONNECTION FAILED: ") + activeRequest.error);
        }
        else
        {
            AudioClip previous = source.clip;
            source.clip = DownloadHandlerAudioClip.GetContent(activeRequest);
            source.Play();
            playbackRequested = true;
            if (restorePosition >= 0f)
            {
                source.time = Mathf.Clamp(restorePosition, 0f, Mathf.Max(0f, source.clip.length - .05f));
                restorePosition = -1f;
            }
            if (previous != null) Destroy(previous);
            Status = L("방송 재생 중 · 컴퓨터 위치 기반 3D 오디오", "PLAYING · POSITIONAL 3D AUDIO");
            HasError = false;
            GameNotificationCenter.Success("시설 라디오 방송을 시작했습니다.");
            PlayerPrefs.SetString(UrlKey, url);
            SavePlaybackState(true);
        }

        activeRequest.Dispose();
        activeRequest = null;
        loadRoutine = null;
        completed?.Invoke();
    }

    private void ApplyVolume()
    {
        if (source == null) return;
        float radio = PlayerPrefs.GetFloat(VolumeKey, 1f);
        source.volume = Mathf.Clamp01(.5f * radio * GameSettings.MasterVolume * GameSettings.BgmVolume);
        // 100%에서는 방 안, 1000%에서는 시설 넓은 구역까지 들린다.
        source.maxDistance = Mathf.Lerp(14f, 100f, Mathf.InverseLerp(100f, 1000f, radio * 100f));
        if (source.isPlaying && Time.unscaledTime >= nextStateSaveAt)
        {
            nextStateSaveAt = Time.unscaledTime + 5f;
            SavePlaybackState(true);
        }
        else if (playbackRequested && source.clip != null && !source.isPlaying && !source.loop && source.time >= source.clip.length - .1f)
        {
            playbackRequested = false;
            SavePlaybackState(false);
        }
    }

    private void Fail(string message)
    {
        Status = message;
        HasError = true;
        GameNotificationCenter.Error(message);
    }

    private void OnDestroy()
    {
        SavePlaybackState(playbackRequested);
        activeRequest?.Abort();
        activeRequest?.Dispose();
    }

    private void OnApplicationQuit() => SavePlaybackState(playbackRequested);

    private void SavePlaybackState(bool isPlaying)
    {
        if (!GameSaveService.HasSave) return;
        PlayerPrefs.SetInt(PlayingKey, isPlaying ? 1 : 0);
        if (source != null && source.clip != null)
            PlayerPrefs.SetFloat(PositionKey, Mathf.Max(0f, source.time));
        PlayerPrefs.Save();
    }

    public static void ResetSavedPlayback()
    {
        PlayerPrefs.DeleteKey(UrlKey);
        PlayerPrefs.DeleteKey(PositionKey);
        PlayerPrefs.SetInt(PlayingKey, 0);
    }

    private static string FormatTime(float seconds)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return $"{total / 60:00}:{total % 60:00}";
    }

    private static string L(string korean, string english) => GameLanguage.IsEnglish ? english : korean;
}
