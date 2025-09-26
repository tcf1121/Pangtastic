using System;
using System.Collections.Generic;
using UnityEngine;

public class DateManager : Singleton<DateManager>
{
    private const string LastPurchaseDateKey = "LastPurchaseDate";
    private const string DailyCountKey = "DailyCount";
    private const string DailyGoodsListKey = "MyNumbers";
    private const string StreakKey = "LoginStreak";
    private const int MaxDailyCount = 5;
    public int LoginStreak { get { return _loginStreak; } }
    private int _loginStreak;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        CheckDailyReset();
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
