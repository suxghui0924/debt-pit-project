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
    private AudioSource source;
    private UnityWebRequest activeRequest;
    private Coroutine loadRoutine;

    public string Status { get; private set; } = "정지됨";
    public bool HasError { get; private set; }
    public float VolumePercent => PlayerPrefs.GetFloat(VolumeKey, 1f) * 100f;
    public bool LoopEnabled => PlayerPrefs.GetInt(LoopKey, 1) != 0;
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

    public void SetVolumePercent(float percent)
    {
        PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp(percent, 0f, 500f) / 100f);
        PlayerPrefs.Save();
        ApplyVolume();
    }

    public void Play(string url, Action completed = null)
    {
        url = url?.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeFile))
        {
            Fail("올바른 MP3/OGG/WAV 주소 또는 로컬 파일 경로를 입력하십시오.");
            completed?.Invoke();
            return;
        }

        string requestUrl = uri.AbsoluteUri;
        if (uri.IsFile)
        {
            string localPath = Uri.UnescapeDataString(uri.LocalPath);
            if (!File.Exists(localPath))
            {
                Fail("로컬 음원 파일을 찾을 수 없습니다.");
                completed?.Invoke();
                return;
            }

            // UnityWebRequest가 파일명의 대괄호/한글/공백을 URL 문법으로 오인하지 않도록 다시 이스케이프한다.
            requestUrl = new Uri(localPath).AbsoluteUri;
        }

        string host = uri.Host.ToLowerInvariant();
        if (host.Contains("youtube.com") || host.Contains("youtu.be") || host.Contains("music.youtube.com"))
        {
            Fail("YouTube 링크는 직접 오디오로 재생할 수 없습니다. 공식 임베디드 플레이어가 필요합니다.");
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
        Status = "정지됨";
        HasError = false;
    }

    public void Seek(float seconds)
    {
        if (source == null || source.clip == null) return;
        source.time = Mathf.Clamp(source.time + seconds, 0f, Mathf.Max(0f, source.clip.length - .05f));
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
        Status = "방송 연결 중...";
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
            Fail("방송 연결 실패: " + activeRequest.error);
        }
        else
        {
            AudioClip previous = source.clip;
            source.clip = DownloadHandlerAudioClip.GetContent(activeRequest);
            source.Play();
            if (previous != null) Destroy(previous);
            Status = "방송 재생 중 · 컴퓨터 위치 기반 3D 오디오";
            HasError = false;
            GameNotificationCenter.Success("시설 라디오 방송을 시작했습니다.");
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
        // 100%에서는 방 안 정도, 500%에서는 넓은 구역까지 들리도록 감쇠 범위를 확장한다.
        source.maxDistance = Mathf.Lerp(14f, 55f, Mathf.InverseLerp(100f, 500f, radio * 100f));
    }

    private void Fail(string message)
    {
        Status = message;
        HasError = true;
        GameNotificationCenter.Error(message);
    }

    private void OnDestroy()
    {
        activeRequest?.Abort();
        activeRequest?.Dispose();
    }

    private static string FormatTime(float seconds)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return $"{total / 60:00}:{total % 60:00}";
    }
}
