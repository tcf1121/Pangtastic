using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DateManager : Singleton<DateManager>
{
    private const string LastPurchaseDateKey = "LastPurchaseDate";
    private const string DailyCountKey = "DailyCount";
    private const string DailyGoodsListKey = "MyNumbers";
    private const string StreakKey = "LoginStreak";
    private const string WeeklyKey = "WeeklyKey";
    private const string ReceivedKey = "ReceivedKey";

    private const int MaxDailyCount = 5;
    public int LoginStreak { get { return _loginStreak; } }
    private int _loginStreak;
    public int WeekLevel { get { return _weekLevel; } }
    private int _weekLevel;
    public List<bool> Received { get { return _received; } }
    private List<bool> _received;
    private bool _checkMonday;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        CheckDailyReset();
        CheckReceived();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            CheckDailyReset();
        }
    }

    void CheckDailyReset()
    {
        string lastDateStr = PlayerPrefs.GetString(LastPurchaseDateKey, "");
        DateTime today = DateTime.Today;

        _loginStreak = PlayerPrefs.GetInt(StreakKey, 0);

        // 일간 초기화
        if (lastDateStr != today.ToString("yyyyMMdd"))
        {
            // 날짜가 다르면 카운트 초기화
            PlayerPrefs.SetInt(DailyCountKey, 0);
            PlayerPrefs.SetString(LastPurchaseDateKey, today.ToString("yyyyMMdd"));
            PlayerPrefs.Save();
            SetPurchaseList();

            if (string.IsNullOrEmpty(lastDateStr))
            {
                _loginStreak = 0;
            }
            else
            {
                _loginStreak++;
            }
            PlayerPrefs.SetInt(StreakKey, _loginStreak);
        }

        // 주간 초기화
        if (!string.IsNullOrEmpty(lastDateStr))
        {
            DateTime lastDate = DateTime.ParseExact(lastDateStr, "yyyyMMdd", null);

            // 마지막 접속일 이후 가장 가까운 월요일 찾기
            DateTime lastMonday = today;
            while (lastMonday.DayOfWeek != DayOfWeek.Monday)
                lastMonday = lastMonday.AddDays(-1);

            // 마지막 접속일 < 최근 월요일 && 오늘 >= 월요일
            if (lastDate < lastMonday)
            {
                _checkMonday = true;
            }
        }
        else
        {
            // 첫 접속일 경우: 오늘이 월요일이면 체크
            if (today.DayOfWeek == DayOfWeek.Monday)
                _checkMonday = true;
        }

        if (_checkMonday)
        {
            Debug.Log("주간 리셋 실행!");
            WeeklyReset();
        }
    }

    private void WeeklyReset()
    {
        PlayerPrefs.SetInt(WeeklyKey, 0);
        PlayerPrefs.SetString(ReceivedKey, "00000000000000000000");
    }

    public void CheckReceived()
    {
        _weekLevel = PlayerPrefs.GetInt(WeeklyKey, 0);
        string receivedString = PlayerPrefs.GetString(ReceivedKey, "00000000000000000000");
        _received = new();

        for (int i = 0; i < 20; i++)
        {
            char c = receivedString[i];
            int digit = c - '0';
            bool received = digit == 1 ? true : false;
            _received.Add(received);
        }
    }

    public void StageClear()
    {
        _weekLevel++;
        if (_weekLevel > 20) _weekLevel = 20;
        PlayerPrefs.SetInt(WeeklyKey, _weekLevel);
    }

    public void SetReceived(int index)
    {

        _received[index] = true;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < 20; i++)
        {
            sb.Append(_received[i] ? '1' : '0');
        }

        string result = sb.ToString();
        PlayerPrefs.GetString(ReceivedKey, result);
    }

    public string GetDate()
    {
        return PlayerPrefs.GetString(LastPurchaseDateKey, "");
    }

    public bool GetLoginStreak(int num)
    {
        return _loginStreak >= num;
    }

    public bool CanPurchase()
    {
        int count = PlayerPrefs.GetInt(DailyCountKey, 0);
        return count < MaxDailyCount;
    }

    public int GetPurchaseIndex()
    {
        return PlayerPrefs.GetInt(DailyCountKey, 0);
    }

    public void Purchase()
    {
        if (CanPurchase())
        {
            int count = PlayerPrefs.GetInt(DailyCountKey, 0);
            PlayerPrefs.SetInt(DailyCountKey, count + 1);
            PlayerPrefs.Save();

            Debug.Log("구매 완료! 오늘 구매 횟수: " + (count + 1));
        }
        else
        {
            Debug.Log("오늘은 더 이상 구매할 수 없습니다.");
        }
    }

    public void SetPurchaseList()
    {
        List<int> pool = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
        int[] numbers = new int[4];

        for (int i = 0; i < 4; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            numbers[i] = pool[index];
            pool.RemoveAt(index); // 중복 제거
        }

        // 배열을 문자열로 변환 (예: "3,0,6,2")
        string saveString = string.Join(",", numbers);

        // 저장
        PlayerPrefs.SetString(DailyGoodsListKey, saveString);
        PlayerPrefs.Save();

        Debug.Log("저장 완료: " + saveString);
    }

    public Goods LoadNumbers(int index)
    {
        if (PlayerPrefs.HasKey("MyNumbers"))
        {
            string loadString = PlayerPrefs.GetString("MyNumbers");
            string[] split = loadString.Split(',');
            int[] numbers = new int[split.Length];
            for (int i = 0; i < split.Length; i++)
            {
                numbers[i] = int.Parse(split[i]);
            }
            return (Goods)numbers[index];

        }
        return 0; // 저장된 값 없음
    }
}
