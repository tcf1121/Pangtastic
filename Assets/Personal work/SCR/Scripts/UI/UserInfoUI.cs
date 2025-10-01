using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoUI : MonoBehaviour
{
    public static UserInfoUI Instance;
    [SerializeField] TMP_Text _coin;
    [SerializeField] TMP_Text _heart;
    [SerializeField] TMP_Text _heartTime;
    [SerializeField] TMP_Text _heartFull;
    [SerializeField] StringSO _heartSO;
    [SerializeField] TMP_Text _star;
    [SerializeField] float _respwanTime;
    [SerializeField] GameObject _infinityImage;
    [SerializeField] GameObject _ui;
    Action<float> _heartTimer;
    Action _OnTimerFinished;
    Action _OnInfinityFinished;
    Coroutine _heartCor;
    Coroutine _infinityCor;
    float _leftTime;

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            SetHeartTime();
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        Manager.Language.ChangedLanguage += () =>
        _heartFull.text = _heartSO.GetText(Manager.Language.GetLanguage());
    }

    void Start()
    {
        Manager.User.OnChangedCoin += Instance.SetCoin;
        Manager.User.OnChangedHeart += Instance.SetHeart;
        Manager.User.OnChangedHeartTime += Instance.SetHeartTimer;
        Manager.User.OnChangedStar += Instance.SetStar;
        Manager.User.OnChangedProfile += Instance.SetProfile;
        Manager.User.OnUseHeart += Instance.StartHeartTimer;
        Manager.User.OnInfinityHeart += Instance.StartInfinityHeart;
        Debug.Log(Manager.User.GetCurrentUserData());
        _coin.text = SetNum(Manager.User.GetCoin());
        _star.text = SetNum(Manager.User.GetStar());
        _heart.text = $"{Manager.User.GetHeart()}";

        _heartTimer += Instance.HeartTimer;
        _OnTimerFinished += Instance.FinishHeartTimeCor;
        _OnInfinityFinished += Instance.FinishInfinity;
        if (Manager.User.GetHeart() == 5)
            _heartTime.text = _heartSO.GetText(Manager.Language.GetLanguage());
        else
            SetHeartTime();
    }

    void OnDestroy()
    {
        Manager.Language.ChangedLanguage -= () =>
        _heartFull.text = _heartSO.GetText(Manager.Language.GetLanguage());
    }

    public string SetNum(int num)
    {
        string returnNum = $"{num}";
        if (num > 10000000)
        {
            returnNum = $"{num / 1000000}m";
        }
        else if (num > 1000000)
        {
            float mnum = (float)num / 1000000;
            returnNum = $"{mnum.ToString("0.0")}m";
        }
        else if (num > 10000)
        {
            returnNum = $"{num / 1000}k";
        }
        else if (num > 1000)
        {
            float knum = (float)num / 1000;
            returnNum = $"{knum.ToString("0.0")}k";
        }

        return returnNum;
    }

    public void SetActive(bool value)
    {
        _ui.SetActive(value);
    }

    public void SetHeartTime()
    {
        if (Manager.User.GetHeart() == 6)
        {
            _infinityImage.SetActive(true);
            _heart.gameObject.SetActive(false);
            DateTime lastTime = DateTime.MinValue;
            if (!string.IsNullOrEmpty(Manager.User.GetLeaveTime()))
                lastTime = DateTime.Parse(Manager.User.GetLeaveTime());

            TimeSpan diff = DateTime.Now - lastTime;
            int TotalSeconds = Manager.User.GetHeartTime() - (int)diff.TotalSeconds;
            if (TotalSeconds <= 0) FinishInfinity();
            else
            {
                Instance._infinityCor = StartCoroutine(Manager.Timer.StartTimer(Instance._infinityCor,
             TotalSeconds, Instance._heartTimer, Instance._OnInfinityFinished));
            }
        }
        else if (Manager.User.GetHeart() == 5)
            Manager.User.SetHeartTime(0);
        else
        {
            DateTime lastTime = DateTime.MinValue;
            if (!string.IsNullOrEmpty(Manager.User.GetLeaveTime()))
                lastTime = DateTime.Parse(Manager.User.GetLeaveTime());

            TimeSpan diff = DateTime.Now - lastTime;
            int elapsed = (int)diff.TotalSeconds;

            int lastHeartTime = Manager.User.GetHeartTime(); // 나갈 당시 남은 시간
            int respawn = (int)_respwanTime; // 1800
            int recoveredHearts = 0;

            // 지난 시간으로 회복 계산
            if (elapsed >= lastHeartTime)
            {
                int totalPassed = elapsed - lastHeartTime;
                recoveredHearts = 1 + (totalPassed / respawn); // 첫 회복 포함

                lastHeartTime = respawn - (totalPassed % respawn);
                if (lastHeartTime == respawn) // 딱 맞아떨어진 경우
                    lastHeartTime = 0;
            }
            else
            {
                lastHeartTime -= elapsed;
            }

            Instance._heartCor = StartCoroutine(
                Manager.Timer.StartTimer(Instance._heartCor,
                lastHeartTime, Instance._heartTimer, Instance._OnTimerFinished));

            if (recoveredHearts > 0)
                Manager.User.AddHeart(recoveredHearts);

        }
    }

    private void FinishHeartTimeCor()
    {
        if (Instance == null) Instance = FindObjectOfType<UserInfoUI>();
        if (Instance._heartCor != null)
        {
            StopCoroutine(Instance._heartCor);
            Instance._heartCor = null;
            SetHeartTimer(0);
        }
        Manager.User.AddHeart();
    }


    private void StartHeartTimer()
    {
        if (Instance == null) Instance = FindObjectOfType<UserInfoUI>();
        if (Instance._heartCor == null)
        {
            Instance._heartCor = StartCoroutine(Manager.Timer.StartTimer(Instance._heartCor,
             _respwanTime, Instance._heartTimer, Instance._OnTimerFinished));
        }
    }

    private void StartInfinityHeart(float hour)
    {
        _infinityImage.SetActive(true);
        _heart.gameObject.SetActive(false);
        float fullTime = hour * 3600;
        if (Instance == null) Instance = FindObjectOfType<UserInfoUI>();

        if (Instance._heartCor != null)
        {
            StopCoroutine(Instance._heartCor);
            Instance._heartCor = null;
            SetHeartTimer(0);
        }

        if (Instance._infinityCor == null)
        {
            Instance._infinityCor = StartCoroutine(Manager.Timer.StartTimer(Instance._infinityCor,
             fullTime, Instance._heartTimer, Instance._OnInfinityFinished));
        }
        else if (Instance._infinityCor != null)
        {
            fullTime += _leftTime;
            StopCoroutine(Instance._infinityCor);
            Instance._infinityCor = null;
            Instance._infinityCor = StartCoroutine(Manager.Timer.StartTimer(Instance._infinityCor,
             fullTime, Instance._heartTimer, Instance._OnInfinityFinished));
        }
    }

    private void FinishInfinity()
    {
        _infinityImage.SetActive(false);
        _heart.gameObject.SetActive(true);
        if (Instance._infinityCor != null)
        {
            StopCoroutine(Instance._infinityCor);
            Instance._infinityCor = null;
        }
        Manager.User.InfinityHeart(0, true);
        SetHeartTimer(0);
    }

    private void HeartTimer(float leftTime)
    {
        _leftTime = leftTime;
        Manager.User.SetHeartTime((int)(leftTime));
    }

    private void SetHeart(int value)
    {
        _heart.text = $"{value}";
        if (value == 5)
        {
            if (Instance._heartCor != null)
            {
                StopCoroutine(Instance._heartCor);
                Instance._heartCor = null;
                SetHeartTimer(0);
            }
        }
        else if (value < 5)
        {
            StartHeartTimer();
        }
    }

    private void SetHeartTimer(int value)
    {
        if (Manager.User.GetHeart() == 5)
        {
            _heartFull.gameObject.SetActive(true);
            _heartTime.gameObject.SetActive(false);

        }

        else
        {
            _heartFull.gameObject.SetActive(false);
            _heartTime.gameObject.SetActive(true);
            int minutes = value / 60;
            int seconds = value % 60;
            _heartTime.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
        }
    }

    private void SetCoin(int value)
    {
        _coin.text = SetNum(Manager.User.GetCoin());
    }

    private void SetStar(int value)
    {
        _star.text = SetNum(Manager.User.GetStar());
    }

    private void SetProfile(int value)
    {

    }
}
