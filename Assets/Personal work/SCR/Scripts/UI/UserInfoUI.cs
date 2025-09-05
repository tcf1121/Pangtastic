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
    [SerializeField] TMP_Text _star;
    [SerializeField] Image _profileImage;
    [SerializeField] float _respwanTime;
    [SerializeField] GameObject _ui;
    Action<float> _heartTimer;
    Action _OnTimerFinished;
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
        Debug.Log(Manager.User.GetCurrentUserData());
        _coin.text = $"{Manager.User.GetCoin()}";
        _heart.text = $"{Manager.User.GetHeart()}";
        _star.text = $"{Manager.User.GetStar()}";
        _heartTimer += Instance.HeartTimer;
        _OnTimerFinished += Instance.FinishHeartTimeCor;
        if (Manager.User.GetHeart() == 5)
            _heartTime.text = $"가득 참";
        else
            SetHeartTime();
    }

    public void SetActive(bool value)
    {
        _ui.SetActive(value);
    }

    public void SetHeartTime()
    {
        if (Manager.User.GetHeart() == 5)
            Manager.User.SetHeartTime(0);
        else
        {
            DateTime lastTime = DateTime.MinValue;
            if (!string.IsNullOrEmpty(Manager.User.GetLeaveTime()))
                lastTime = DateTime.Parse(Manager.User.GetLeaveTime());

            TimeSpan diff = DateTime.Now - lastTime;
            int TotalSeconds = (int)(_respwanTime - Manager.User.GetHeartTime()) + (int)diff.TotalSeconds;
            int recoveredHearts = TotalSeconds / (int)_respwanTime;
            while (Manager.User.GetHeart() < 5 && recoveredHearts > 0)
            {
                recoveredHearts--;
                Manager.User.AddHeart();
            }
            if (Manager.User.GetHeart() < 5)
            {
                float leftTime = TotalSeconds % (int)_respwanTime;
                Instance._heartCor = StartCoroutine(Manager.Timer.StartTimer(Instance._heartCor,
                _respwanTime, leftTime, Instance._heartTimer, Instance._OnTimerFinished));
            }

        }
    }

    private void FinishHeartTimeCor()
    {
        if (Instance == null) Instance = FindObjectOfType<UserInfoUI>();
        Manager.User.AddHeart();
        if (Manager.User.GetHeart() < 5)
            StartHeartTimer();
    }


    private void StartHeartTimer()
    {
        if (Instance == null) Instance = FindObjectOfType<UserInfoUI>();
        if (Instance._heartCor == null)
        {
            Instance._heartCor = StartCoroutine(Manager.Timer.StartTimer(Instance._heartCor,
             _respwanTime, 0, Instance._heartTimer, Instance._OnTimerFinished));
        }
    }

    private void HeartTimer(float leftTime)
    {
        Manager.User.SetHeartTime((int)(_respwanTime - leftTime));
    }

    private void SetHeart(int value)
    {
        _heart.text = $"{value}";
        if (Instance._heartCor != null)
        {
            StopCoroutine(Instance._heartCor);
            Instance._heartCor = null;
        }
    }

    private void SetHeartTimer(int value)
    {
        if (Manager.User.GetHeart() == 5)
            _heartTime.text = $"가득 참";
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
