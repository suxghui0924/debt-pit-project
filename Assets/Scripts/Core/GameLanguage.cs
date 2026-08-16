using System;
using UnityEngine;

public static class GameLanguage
{
    public static event Action Changed;
    public static bool IsEnglish { get => PlayerPrefs.GetInt("Settings.Language", 0) == 1; set { PlayerPrefs.SetInt("Settings.Language", value ? 1 : 0); Changed?.Invoke(); } }
    public static string Text(string key)
    {
        if (!IsEnglish) return key switch
        {
            "new_game" => "새 게임", "load_game" => "로드 게임", "settings" => "설정", "exit" => "나가기", "exit_question" => "정말로 나가시겠습니까?", "confirm" => "확인", "cancel" => "아니요",
            "resolution" => "해상도", "master" => "마스터 볼륨", "bgm" => "BGM 볼륨", "sfx" => "SFX 볼륨", "sensitivity" => "마우스 감도", "fullscreen" => "전체화면", "on" => "켜짐", "off" => "꺼짐", "close" => "닫기", "language" => "언어", "korean" => "한국어", "english" => "English",
            "pause" => "일시 정지", "resume" => "플레이 계속", "return_lobby" => "로비로 돌아가기", _ => key
        };
        return key switch
        {
            "new_game" => "NEW GAME", "load_game" => "LOAD GAME", "settings" => "SETTINGS", "exit" => "EXIT", "exit_question" => "ARE YOU SURE YOU WANT TO EXIT?", "confirm" => "YES", "cancel" => "NO",
            "resolution" => "RESOLUTION", "master" => "MASTER VOLUME", "bgm" => "BGM VOLUME", "sfx" => "SFX VOLUME", "sensitivity" => "MOUSE SENSITIVITY", "fullscreen" => "FULLSCREEN", "on" => "ON", "off" => "OFF", "close" => "CLOSE", "language" => "LANGUAGE", "korean" => "한국어", "english" => "ENGLISH",
            "pause" => "PAUSED", "resume" => "RESUME", "return_lobby" => "RETURN TO LOBBY", _ => key
        };
    }
}
