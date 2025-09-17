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
    [SerializeField] StringSO _heartSO;
    [SerializeField] TMP_Text _star;
    [SerializeField] Image _profileImage;
    [SerializeField] float _respwanTime;
    [SerializeField] GameObject _ui;
    Action<float> _heartTimer;
    Action _OnTimerFinished;
    Action _OnInfinityFinished;
    Coroutine _heartCor;

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
        _coin.text = $"{Manager.User.GetCoin()}";
        _heart.text = $"{Manager.User.GetHeart()}";
        _star.text = $"{Manager.User.GetStar()}";
        _heartTimer += Instance.HeartTimer;
        _OnTimerFinished += Instance.FinishHeartTimeCor;
        _OnInfinityFinished += Instance.FinishInfinity;
        if (Manager.User.GetHeart() == 5)
            _heartTime.text = _heartSO.GetText(Manager.Language.GetLanguage());
        else
            SetHeartTime();
    }

    public void SetActive(bool value)
    {
        _ui.SetActive(value);
    }

    public void SetHeartTime()
    {
        if (Manager.User.GetHeart() == 6)
        {
            DateTime lastTime = DateTime.MinValue;
            if (!string.IsNullOrEmpty(Manager.User.GetLeaveTime()))
                lastTime = DateTime.Parse(Manager.User.GetLeaveTime());

            TimeSpan diff = DateTime.Now - lastTime;
            int TotalSeconds = Manager.User.GetHeartTime() - (int)diff.TotalSeconds;
            if (TotalSeconds <= 0) Manager.User.InfinityHeart(0, true);
            else
            {
                Instance._heartCor = StartCoroutine(Manager.Timer.StartTimer(Instance._heartCor,
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


            while (Manager.User.GetHeart() < 5 && recoveredHearts > 0)
            {
                Manager.User.AddHeart();
                recoveredHearts--;
            }

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
        float fullTime = hour * 3600;
        if (Instance == null) Instance = FindObjectOfType<UserInfoUI>();

        if (Instance._heartCor != null)
        {
            StopCoroutine(Instance._heartCor);
            Instance._heartCor = null;
            SetHeartTimer(0);
        }

        if (Instance._heartCor == null)
        {
            Instance._heartCor = StartCoroutine(Manager.Timer.StartTimer(Instance._heartCor,
             fullTime, Instance._heartTimer, Instance._OnInfinityFinished));
        }
    }

    private void FinishInfinity()
    {
        if (Instance._heartCor != null)
        {
            StopCoroutine(Instance._heartCor);
            Instance._heartCor = null;
            SetHeartTimer(0);
        }
        Manager.User.InfinityHeart(0, true);
    }

    private void HeartTimer(float leftTime)
    {
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
            _heartTime.text = _heartSO.GetText(Manager.Language.GetLanguage());
        else
        {
            int minutes = value / 60;
            int seconds = value % 60;
            _heartTime.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
        }
    }

    private void SetCoin(int value)
    {
        _coin.text = $"{value}";
    }

    private void SetStar(int value)
    {
        _star.text = $"{value}";
    }

    private void SetProfile(int value)
    {

    }
}
