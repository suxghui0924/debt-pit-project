using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameLanguage
{
    private static List<KeyValuePair<string, string>> itemFragments;

    private static readonly Dictionary<string, string> ExactEnglish = new()
    {
        ["거래 메뉴"] = "TRADE MENU",
        ["보유품 처분"] = "SELL OWNED ITEMS",
        ["할인품 구매"] = "BUY DISCOUNTED ITEMS",
        ["판매할 물건이 없습니다"] = "NO ITEMS TO SELL",
        ["물건 투입구에서 배송품을 수령한 뒤 다시 방문하십시오."] = "COLLECT DELIVERIES FROM THE DELIVERY CHUTE, THEN RETURN.",
        ["개별 판매는 최대 5회 흥정할 수 있습니다. 전체 판매는 기본가로 즉시 정산됩니다."] = "NEGOTIATE INDIVIDUAL SALES UP TO 5 TIMES. SELL ALL SETTLES IMMEDIATELY AT BASE VALUE.",
        ["시설 배송망"] = "FACILITY DELIVERY NETWORK",
        ["수령 대기 목록"] = "PENDING DELIVERIES",
        ["도착한 물품이 없습니다.\n컴퓨터에서 구매한 상품은 이곳으로 배송됩니다."] = "NO DELIVERIES HAVE ARRIVED.\nITEMS PURCHASED ON THE COMPUTER ARE DELIVERED HERE.",
        ["빈 슬롯만큼 수령하며\n나머지는 이곳에 보관됩니다."] = "ITEMS FILL EMPTY SLOTS.\nTHE REST REMAIN STORED HERE.",
        ["개봉 및 해제"] = "OPEN & UNSEAL",
        ["카드팩은 즉시 개봉하고, 봉인된 상자는 락핀 스킬 체크로 해제합니다."] = "OPEN CARD PACKS OR UNSEAL CONTAINERS WITH A LOCKPICK SKILL CHECK.",
        ["개봉할 카드팩이나 봉인된 상자가 없습니다.\n컴퓨터와 야시장에서 물품을 구하십시오."] = "NO CARD PACKS OR SEALED BOXES ARE AVAILABLE.\nGET SUPPLIES FROM THE COMPUTER OR NIGHT MARKET.",
        ["내용물 미확인 · 정밀 해제"] = "CONTENTS UNKNOWN · PRECISION UNSEALING",
        ["최상급 보안 · 극희귀 보상"] = "MAXIMUM SECURITY · ULTRA-RARE REWARD",
        ["군수 등급 · 고위험 고보상"] = "MILITARY GRADE · HIGH RISK, HIGH REWARD",
        ["보급 등급 · 중간 보상"] = "SUPPLY GRADE · STANDARD REWARD",
        ["낮은 등급 · 저렴한 입문 상자"] = "LOW GRADE · ENTRY-LEVEL BOX",
        ["작업 장갑"] = "WORK GLOVES",
        ["캔 식량"] = "CANNED RATIONS",
        ["잠금 상자"] = "LOCKED BOX",
        ["무료 카드팩"] = "FREE CARD PACK",
        ["낡은 카드팩"] = "WORN CARD PACK",
        ["보급 카드팩"] = "SUPPLY CARD PACK",
        ["고급 카드팩"] = "PREMIUM CARD PACK",
        ["구겨진 시간표"] = "CRUMPLED TIMETABLE",
        ["녹슨 출입증"] = "RUSTED ACCESS PASS",
        ["빈 배급표"] = "EMPTY RATION TICKET",
        ["파손된 코일"] = "BROKEN COIL",
        ["작업반 명찰"] = "WORK-CREW BADGE",
        ["감시 기록 조각"] = "SURVEILLANCE FRAGMENT",
        ["봉인된 영수증"] = "SEALED RECEIPT",
        ["정비용 회로"] = "MAINTENANCE CIRCUIT",
        ["야간 통행증"] = "NIGHT PASS",
        ["폐기 승인서"] = "DISPOSAL APPROVAL",
        ["검열된 편지"] = "CENSORED LETTER",
        ["비상 전력 셀"] = "EMERGENCY POWER CELL",
        ["암호화된 장부"] = "ENCRYPTED LEDGER",
        ["관리자 서명"] = "ADMINISTRATOR SIGNATURE",
        ["검은 거래표"] = "BLACK MARKET TICKET",
        ["기억 복구 조각"] = "MEMORY RECOVERY FRAGMENT",
        ["격리구역 열쇠"] = "QUARANTINE KEY",
        ["시설 설계도"] = "FACILITY BLUEPRINT",
        ["위조된 판결문"] = "FORGED VERDICT",
        ["탈출 경로 지도"] = "ESCAPE ROUTE MAP",
        ["감독관 인장"] = "WARDEN'S SEAL",
        ["삭제된 신원표"] = "DELETED IDENTITY TAG",
        ["중앙 서버 키"] = "CENTRAL SERVER KEY",
        ["자유 채권"] = "FREEDOM BOND",
        ["사형 집행 유예장"] = "STAY OF EXECUTION",
        ["기억 원본 백업"] = "ORIGINAL MEMORY BACKUP",
        ["정부 비밀 계정"] = "GOVERNMENT SECRET ACCOUNT",
        ["시설 소유 증서"] = "FACILITY OWNERSHIP DEED",
        ["무기한 면책 계약"] = "INDEFINITE IMMUNITY CONTRACT",
        ["황금 자유 계약"] = "GOLDEN FREEDOM CONTRACT",
        ["녹슨 가챠 상자"] = "RUSTED LOOT BOX",
        ["보급 가챠 상자"] = "SUPPLY LOOT BOX",
        ["군수 가챠 상자"] = "MILITARY LOOT BOX",
        ["검은 금고"] = "BLACK VAULT",
        ["수령"] = "COLLECT",
        ["대기"] = "PENDING",
        ["개봉"] = "OPEN",
        ["슬롯"] = "SLOT",
        ["희귀도"] = "RARITY",
        ["락핀 필요"] = "LOCKPICK REQUIRED",
        ["정지됨"] = "STOPPED",
        ["방송 연결 중..."] = "CONNECTING...",
        ["방송 재생 중 · 컴퓨터 위치 기반 3D 오디오"] = "PLAYING · POSITIONAL 3D AUDIO",
        ["컴퓨터 오디오 장치를 찾지 못했습니다."] = "COMPUTER AUDIO DEVICE NOT FOUND."
        ,["폐기장 잔해 팩"] = "SCRAPYARD DEBRIS PACK"
        ,["녹슨 철문 팩"] = "RUSTED GATE PACK"
        ,["야간 배급 팩"] = "NIGHT RATION PACK"
        ,["작업반 교대 팩"] = "SHIFT CHANGE PACK"
        ,["정비구역 부품 팩"] = "MAINTENANCE PARTS PACK"
        ,["감시망 기록 팩"] = "SURVEILLANCE RECORD PACK"
        ,["밀봉 영수증 팩"] = "SEALED RECEIPT PACK"
        ,["비상 전력 팩"] = "EMERGENCY POWER PACK"
        ,["검열 우편 팩"] = "CENSORED MAIL PACK"
        ,["폐기 승인 팩"] = "DISPOSAL APPROVAL PACK"
        ,["지하 거래 팩"] = "UNDERGROUND TRADE PACK"
        ,["격리구역 보급 팩"] = "QUARANTINE SUPPLY PACK"
        ,["암호 장부 팩"] = "CIPHER LEDGER PACK"
        ,["관리국 인가 팩"] = "AUTHORITY CLEARANCE PACK"
        ,["검은 시장 팩"] = "BLACK MARKET PACK"
        ,["기억 파편 팩"] = "MEMORY FRAGMENT PACK"
        ,["보안구역 열쇠 팩"] = "SECURITY KEY PACK"
        ,["시설 설계도 팩"] = "FACILITY BLUEPRINT PACK"
        ,["위조 판결 팩"] = "FORGED VERDICT PACK"
        ,["탈출 경로 팩"] = "ESCAPE ROUTE PACK"
        ,["감독관 금고 팩"] = "WARDEN VAULT PACK"
        ,["삭제 신원 팩"] = "DELETED IDENTITY PACK"
        ,["중앙 서버 팩"] = "CENTRAL SERVER PACK"
        ,["자유 채권 팩"] = "FREEDOM BOND PACK"
        ,["집행 유예 팩"] = "STAY OF EXECUTION PACK"
        ,["기억 원본 팩"] = "ORIGINAL MEMORY PACK"
        ,["정부 비밀계정 팩"] = "GOVERNMENT SECRET ACCOUNT PACK"
        ,["시설 소유권 팩"] = "FACILITY OWNERSHIP PACK"
        ,["무기한 면책 팩"] = "INDEFINITE IMMUNITY PACK"
        ,["황금 자유계약 팩"] = "GOLDEN FREEDOM CONTRACT PACK"
        ,["승률은 공개되지 않습니다."] = "WIN ODDS ARE NOT DISCLOSED."
        ,["당첨. {reward:N0} 노동값이 지급되었습니다."] = "WIN. LABOR HAS BEEN AWARDED."
        ,["실패. 베팅 노동값을 잃었습니다."] = "FAILED. THE WAGERED LABOR WAS LOST."
    };

    private static readonly KeyValuePair<string, string>[] RuntimeEnglish =
    {
        new("게임 불러오기", "LOAD GAME"), new("새 게임", "NEW GAME"), new("설정", "SETTINGS"), new("나가기", "EXIT"),
        new("일일 납부", "DAILY PAYMENT"), new("자유 기금", "FREEDOM FUND"), new("데일리 상품", "DAILY SHOP"),
        new("데일리 보상", "DAILY REWARD"), new("카드 상점", "CARD SHOP"), new("도구 상점", "TOOL SHOP"),
        new("업그레이드 상점", "UPGRADE SHOP"), new("업그레이드", "UPGRADES"), new("위험 게임", "RISK GAME"),
        new("하루 넘기기", "END DAY"), new("도움말", "HELP"), new("도움 앱", "HELP"), new("라디오", "RADIO"),
        new("도박 앱", "RISK GAME"), new("하루 종료", "END DAY"),
        new("컴퓨터 열기", "OPEN COMPUTER"), new("상점 열기", "OPEN SHOP"),
        new("물건 투입구 열기", "OPEN DELIVERY CHUTE"), new("작업대 열기", "OPEN WORKBENCH"),
        new("내 컴퓨터", "MY COMPUTER"), new("채무 계정", "DEBT ACCOUNT"), new("물건 투입구", "DELIVERY CHUTE"),
        new("컴퓨터 끄기", "SHUT DOWN"), new("작업대", "WORKBENCH"), new("상점", "SHOP"),
        new("오늘의 야시장", "TODAY'S NIGHT MARKET"), new("야시장", "NIGHT MARKET"), new("보유품 판매", "SELL INVENTORY"),
        new("전체 판매", "SELL ALL"), new("판매", "SELL"), new("구매", "BUY"), new("가격 제시", "MAKE OFFER"),
        new("흥정", "NEGOTIATE"), new("수령 대기 목록", "PENDING DELIVERIES"), new("모두 수령", "COLLECT ALL"),
        new("수령 가능", "READY"), new("인벤토리", "INVENTORY"), new("보유 노동값", "LABOR BALANCE"),
        new("노동값", "LABOR"), new("오늘 미납", "PAYMENT DUE"), new("납부 완료", "PAID"), new("납부", "PAY"),
        new("자정까지", "UNTIL MIDNIGHT"), new("자동 갱신", "AUTO REFRESH"), new("즉시 리롤", "REROLL NOW"),
        new("재고 갱신", "STOCK REFRESH"), new("닫기", "CLOSE"), new("재생", "PLAY"), new("정지", "STOP"),
        new("반복 재생", "LOOP"), new("라디오 볼륨", "RADIO VOLUME"), new("방송 연결 중", "CONNECTING"),
        new("방송 재생 중", "PLAYING"), new("일시 정지", "PAUSED"), new("플레이 계속", "RESUME"),
        new("로비로 돌아가기", "RETURN TO LOBBY"), new("예 · 다시 시작", "YES · RESTART"), new("아니오 · 타이틀", "NO · TITLE"),
        new("시설 안내 시스템", "FACILITY GUIDANCE SYSTEM"), new("처분 관리 시스템", "DISPOSAL CONTROL SYSTEM"),
        new("석방 처리 시스템", "RELEASE PROCESSING SYSTEM"), new("계속", "CONTINUE"), new("성공", "SUCCESS"),
        new("튜토리얼 건너뛰기", "SKIP TUTORIAL"), new("확인", "CONFIRM"), new("다음", "NEXT"),
        new("실패", "FAILED"), new("부족합니다", "IS INSUFFICIENT"), new("구매 완료", "PURCHASE COMPLETE"),
        new("판매 완료", "SALE COMPLETE"), new("인벤토리가 가득 찼습니다", "INVENTORY IS FULL"),
        new("무료 보상", "FREE REWARD"), new("오늘 수령 완료", "CLAIMED TODAY"), new("보내기", "SEND"),
        new("작업 장갑", "WORK GLOVES"), new("캔 식량", "CANNED RATIONS"), new("잠금 상자", "LOCKED BOX"),
        new("녹슨 가챠 상자", "RUSTED LOOT BOX"), new("보급 가챠 상자", "SUPPLY LOOT BOX"),
        new("군수 가챠 상자", "MILITARY LOOT BOX"), new("검은 금고", "BLACK VAULT"),
        new("카드팩", "CARD PACK"), new("봉인된 상자", "SEALED BOX"), new("락핀", "LOCKPICK"),
        new("락픽", "LOCKPICK"), new("드릴", "DRILL"), new("절단", "CUT"), new("취소", "CANCEL"),
        new("속도", "SPEED"), new("입력 대기", "AWAITING INPUT"),
        new("휴대용 드릴", "PORTABLE DRILL"), new("냉각 스프레이", "COOLANT SPRAY"), new("유압 절단기", "HYDRAULIC CUTTER"),
        new("레벨", "LEVEL"), new("경험치", "EXP"), new("필요", "REQUIRED"), new("해금", "UNLOCKED"),
        new("현재 적립금", "CURRENT FUND"), new("현재", "CURRENT"), new("목표", "GOAL"), new("남은 목표액", "REMAINING GOAL"),
        new("기본가", "BASE VALUE"), new("슬롯", "SLOT"), new("희귀도", "RARITY"),
        new("대기 ", "PENDING "), new("락핀 필요", "LOCKPICK REQUIRED")
    };
    public static event Action Changed;
    public static bool IsEnglish { get => PlayerPrefs.GetInt("Settings.Language", 0) == 1; set { PlayerPrefs.SetInt("Settings.Language", value ? 1 : 0); Changed?.Invoke(); } }
    public static string Text(string key)
    {
        if (!IsEnglish) return key switch
        {
            "new_game" => "새 게임", "load_game" => "게임 불러오기", "settings" => "설정", "exit" => "나가기", "exit_question" => "정말로 나가시겠습니까?", "confirm" => "확인", "cancel" => "아니요",
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

    public static string Runtime(string value)
    {
        if (!IsEnglish || string.IsNullOrEmpty(value)) return value;
        if (ExactEnglish.TryGetValue(value, out string exact)) return exact;
        string translated = value;
        foreach (KeyValuePair<string, string> pair in RuntimeEnglish)
            translated = translated.Replace(pair.Key, pair.Value);
        return translated;
    }

    public static string Item(string value)
    {
        if (!IsEnglish || string.IsNullOrEmpty(value)) return value;
        if (ExactEnglish.TryGetValue(value, out string exact)) return exact;

        // Generated inventory names contain a rank prefix (for example
        // "01급·구겨진 시간표"), so an exact dictionary lookup alone cannot
        // translate them. Replace known item fragments without changing the
        // Korean value stored in PlayerPrefs or used by the economy systems.
        string translated = value;
        if (itemFragments == null)
        {
            itemFragments = new List<KeyValuePair<string, string>>(ExactEnglish);
            itemFragments.Sort((left, right) => right.Key.Length.CompareTo(left.Key.Length));
        }

        foreach (KeyValuePair<string, string> pair in itemFragments)
        {
            if (translated.Contains(pair.Key))
                translated = translated.Replace(pair.Key, pair.Value);
        }

        translated = System.Text.RegularExpressions.Regex.Replace(translated, @"^(\d{2})급·", "GRADE $1 · ");
        if (translated.StartsWith("특급·")) translated = "PREMIUM GRADE · " + translated[3..];
        else if (translated.StartsWith("배급·")) translated = "SUPPLY GRADE · " + translated[3..];
        else if (translated.StartsWith("폐기·")) translated = "SCRAP GRADE · " + translated[3..];

        return Runtime(translated);
    }

    public static string Notification(string value)
    {
        if (!IsEnglish || string.IsNullOrWhiteSpace(value)) return value;

        string exact = value switch
        {
            "오늘의 노동값은 이미 납부했습니다." => "TODAY'S LABOR PAYMENT HAS ALREADY BEEN COMPLETED.",
            "자유 기금 목표를 이미 달성했습니다." => "THE FREEDOM FUND GOAL HAS ALREADY BEEN REACHED.",
            "납부할 금액을 올바르게 입력하십시오." => "ENTER A VALID PAYMENT AMOUNT.",
            "오늘의 무료 보상은 이미 받았습니다." => "TODAY'S FREE REWARD HAS ALREADY BEEN CLAIMED.",
            "무료 카드팩을 물건 투입구로 보냈습니다." => "FREE CARD PACK SENT TO THE DELIVERY CHUTE.",
            "이미 최대 단계인 업그레이드입니다." => "THIS UPGRADE IS ALREADY AT MAXIMUM LEVEL.",
            "인벤토리가 가득 찼습니다. 물건을 판매하거나 슬롯을 확장하십시오." => "INVENTORY FULL. SELL ITEMS OR EXPAND YOUR SLOTS.",
            "물품을 수령할 빈 인벤토리 슬롯이 없습니다." => "NO EMPTY INVENTORY SLOT IS AVAILABLE.",
            "선택한 물품을 수령하지 못했습니다." => "THE SELECTED DELIVERY COULD NOT BE COLLECTED.",
            "판매할 물건이 없습니다." => "NO ITEMS ARE AVAILABLE TO SELL.",
            "흥정 기회를 모두 사용했습니다." => "ALL NEGOTIATION ATTEMPTS HAVE BEEN USED.",
            "흥정에 실패했습니다." => "NEGOTIATION FAILED.",
            "락핀이 없습니다. 컴퓨터의 도구 상점에서 구매하십시오." => "NO LOCKPICK AVAILABLE. PURCHASE ONE FROM THE COMPUTER TOOL STORE.",
            "휴대용 드릴이 없습니다. 컴퓨터 도구 상점에서 구매하십시오." => "NO PORTABLE DRILL AVAILABLE. PURCHASE ONE FROM THE COMPUTER TOOL STORE.",
            "유압 절단기가 없습니다. 컴퓨터 도구 상점에서 구매하십시오." => "NO HYDRAULIC CUTTER AVAILABLE. PURCHASE ONE FROM THE COMPUTER TOOL STORE.",
            "카드팩 개봉을 취소했습니다." => "CARD-PACK OPENING CANCELLED.",
            "봉인 해제 실패 · 락핀이 부러졌습니다." => "UNSEALING FAILED · THE LOCKPICK BROKE.",
            "드릴 과열 · 도구가 파손되었습니다. 상자는 유지됩니다." => "DRILL OVERHEATED · TOOL DESTROYED. THE BOX REMAINS.",
            "시설 라디오 방송을 시작했습니다." => "FACILITY RADIO PLAYBACK STARTED.",
            _ => null
        };
        if (exact != null) return exact;

        System.Text.RegularExpressions.Match match;
        match = System.Text.RegularExpressions.Regex.Match(value, @"^노동값이 ([\d,]+) 부족합니다\.$");
        if (match.Success) return $"INSUFFICIENT LABOR · {match.Groups[1].Value} MORE REQUIRED.";
        match = System.Text.RegularExpressions.Regex.Match(value, @"^구매 실패: 노동값이 ([\d,]+) 부족합니다\.$");
        if (match.Success) return $"PURCHASE FAILED · {match.Groups[1].Value} MORE LABOR REQUIRED.";
        match = System.Text.RegularExpressions.Regex.Match(value, @"^(.+) 구매 완료 · 물건 투입구로 배송했습니다\.$");
        if (match.Success) return $"PURCHASE COMPLETE: {Item(match.Groups[1].Value)} · SENT TO DELIVERY CHUTE.";
        match = System.Text.RegularExpressions.Regex.Match(value, @"^오늘의 노동값 ([\d,]+)을 납부했습니다\.$");
        if (match.Success) return $"DAILY PAYMENT COMPLETE · {match.Groups[1].Value} LABOR PAID.";
        match = System.Text.RegularExpressions.Regex.Match(value, @"^자유 기금에 ([\d,]+)을 납부했습니다\.$");
        if (match.Success) return $"FREEDOM FUND PAYMENT COMPLETE · {match.Groups[1].Value} LABOR.";
        match = System.Text.RegularExpressions.Regex.Match(value, @"^(.+) 1개를 수령했습니다\.$");
        if (match.Success) return $"COLLECTED 1× {Item(match.Groups[1].Value)}.";
        match = System.Text.RegularExpressions.Regex.Match(value, @"^배송 물품 ([\d,]+)개를 모두 수령했습니다\.$");
        if (match.Success) return $"COLLECTED ALL {match.Groups[1].Value} DELIVERIES.";
        match = System.Text.RegularExpressions.Regex.Match(value, @"^당첨! \+([\d,]+) 노동값$");
        if (match.Success) return $"WIN! +{match.Groups[1].Value} LABOR.";
        match = System.Text.RegularExpressions.Regex.Match(value, @"^실패 · ([\d,]+) 노동값을 잃었습니다\.$");
        if (match.Success) return $"FAILED · LOST {match.Groups[1].Value} LABOR.";
        match = System.Text.RegularExpressions.Regex.Match(value, @"^(.+) 개봉 · (.+) 획득$");
        if (match.Success) return $"OPENED {Item(match.Groups[1].Value)} · ACQUIRED {Item(match.Groups[2].Value)}.";
        match = System.Text.RegularExpressions.Regex.Match(value, @"^흥정 성공 · 제시가 \+([\d,]+)$");
        if (match.Success) return $"NEGOTIATION SUCCESS · OFFER +{match.Groups[1].Value}";
        match = System.Text.RegularExpressions.Regex.Match(value, @"^(.+) 판매 완료 · \+([\d,]+) 노동값$");
        if (match.Success) return $"SOLD {Item(match.Groups[1].Value)} · +{match.Groups[2].Value} LABOR";
        match = System.Text.RegularExpressions.Regex.Match(value, @"^전체 판매 완료 · ([\d,]+)개 · \+([\d,]+) 노동값$");
        if (match.Success) return $"SOLD ALL · {match.Groups[1].Value} ITEMS · +{match.Groups[2].Value} LABOR";
        match = System.Text.RegularExpressions.Regex.Match(value, @"^LEVEL UP!  LV (\d{2}) · 새 카드가 해금되었습니다\.$");
        if (match.Success) return $"LEVEL UP! · LV {match.Groups[1].Value} · NEW CARD UNLOCKED.";

        return Runtime(value);
    }
}
