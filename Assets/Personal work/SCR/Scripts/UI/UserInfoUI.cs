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
        Manager.User.OnChangedCoin += SetCoin;
        Manager.User.OnChangedHeart += SetHeart;
        Manager.User.OnChangedHeartTime += SetHeartTimer;
        Manager.User.OnChangedStar += SetStar;
        Manager.User.OnChangedProfile += SetProfile;
        Manager.User.OnUseHeart += StartHeartTimer;
        Debug.Log(Manager.User.GetCurrentUserData());
        _coin.text = $"{Manager.User.GetCoin()}";
        _heart.text = $"{Manager.User.GetHeart()}";
        _star.text = $"{Manager.User.GetStar()}";
        _heartTimer += HeartTimer;
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
            int recoveredHearts = (int)(diff.TotalSeconds / _respwanTime);
            while (Manager.User.GetHeart() < 5 && recoveredHearts > 0)
            {
                recoveredHearts--;
                Manager.User.AddHeart();
            }
            if (Manager.User.GetHeart() < 5)
            {
                float leftTime = (int)diff.TotalSeconds % (int)_respwanTime;
                _heartCor = StartCoroutine(Manager.Timer.StartTimer(_heartCor, _respwanTime, leftTime, _heartTimer));
            }

        }
    }

    private void StartHeartTimer()
    {
        if (_heartCor == null)
        {
            _heartCor = StartCoroutine(Manager.Timer.StartTimer(_heartCor, _respwanTime, 0, _heartTimer));
        }
    }

    private void HeartTimer(float leftTime)
    {
        Manager.User.SetHeartTime((int)(_respwanTime - leftTime));
    }

    private void SetHeart(int value)
    {
        if (value == 5)
        {
            if (_heartCor != null)
            {
                StopCoroutine(_heartCor);
                _heartCor = null;
            }
            _heartTime.text = $"가득 참";
        }
        _heart.text = $"{value}";
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
