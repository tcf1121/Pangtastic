using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartSystem : Singleton<HeartSystem>
{
    //public static HeartSystem Instance { get; private set; }

    [SerializeField] private TMP_Text _timerText; // UI Text (MM:SS 표시)
    [SerializeField] private TMP_Text _textHeart; // 0/0 표시

    [SerializeField] private int _startSeconds = 1800; // 시작 시간 (기본 30분, 초 단위)
    [SerializeField] private int _maxHearts = 5; // 최대 하트 개수
    [SerializeField] private float _currentHearts = 0; // 현재 하트 개수

    private float _remainingSeconds;

    public bool isPlaying = false;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("하트 시스템");
    }

    private void Start()
    {
        //StopAllCoroutines(); // 혹시 중복 실행된 코루틴이 있으면 정리
        StartCoroutine(CalcHeartData());
    }

    private IEnumerator CalcHeartData()
    {
        string authJson = Manager.DB.GetAuthInfo();
        DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);

        var task = Manager.DB.dbRef.Child(info.type)
            .Child(info.uid)
            .Child("heart")
            .GetValueAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("heart 불러오기 실패: " + task.Exception);
            yield break;
        }

        DataSnapshot snapshot = task.Result;

        if (!snapshot.Exists) // heart 키 자체가 없을 때 → 새로 생성
        {
            Debug.Log("heart 데이터 없음 → 새로 생성");

            _currentHearts = _maxHearts;
            _remainingSeconds = 0;

            var heartRef = Manager.DB.dbRef
                .Child(info.type)
                .Child(info.uid)
                .Child("heart");

            var setTask = heartRef.SetRawJsonValueAsync(JsonUtility.ToJson(new HeartData
            {
                currentHeart = _currentHearts,
                lastSaveTime = DateTime.Now.ToString("O"), // ISO8601
                remainingSeconds = _remainingSeconds
            }));

            yield return new WaitUntil(() => setTask.IsCompleted);

            if (setTask.Exception != null)
                Debug.LogError("heart 초기 생성 실패: " + setTask.Exception);
            else
                Debug.Log("heart 데이터 초기화 완료");

            // UI 초기화
            UpdateHeartUI();
            UpdateTimerUI();
            yield break;
        }

        // --- heart 데이터가 있을 때 불러오기 ---
        string currentHeart = snapshot.HasChild("currentHeart") ? snapshot.Child("currentHeart").Value?.ToString() : null;
        string lastSaveTime = snapshot.HasChild("lastSaveTime") ? snapshot.Child("lastSaveTime").Value?.ToString() : null;
        string remainingSeconds = snapshot.HasChild("remainingSeconds") ? snapshot.Child("remainingSeconds").Value?.ToString() : null;

        _currentHearts = string.IsNullOrEmpty(currentHeart) ? _maxHearts : int.Parse(currentHeart);
        _remainingSeconds = string.IsNullOrEmpty(remainingSeconds) ? _startSeconds : int.Parse(remainingSeconds);

        DateTime lastTime = DateTime.MinValue;
        if (!string.IsNullOrEmpty(lastSaveTime))
            lastTime = DateTime.Parse(lastSaveTime);

        TimeSpan diff = DateTime.Now - lastTime;

        // 지난 시간 동안 회복된 하트 계산
        int recoveredHearts = (int)(diff.TotalSeconds / _startSeconds);
        _currentHearts = Mathf.Min(_currentHearts + recoveredHearts, _maxHearts);

        if (_currentHearts < _maxHearts)
        {
            _remainingSeconds -= (int)diff.TotalSeconds;

            if (_remainingSeconds <= 0)
            {
                float extraHearts = Mathf.Abs(_remainingSeconds) / _startSeconds + 1;
                _currentHearts = Mathf.Min(_currentHearts + extraHearts, _maxHearts);

                if (_currentHearts < _maxHearts)
                {
                    _remainingSeconds = _startSeconds - (Mathf.Abs(_remainingSeconds) % _startSeconds);
                }
                else
                {
                    _remainingSeconds = 0;
                }
            }
        }
        else
        {
            _remainingSeconds = 0;
        }

        // UI 업데이트 및 저장
        UpdateHeartUI();
        UpdateTimerUI();
        StartCoroutine(TimerCoroutine());
        HeartSaveData();
    }

    [System.Serializable]
    public class HeartData
    {
        public float currentHeart;
        public string lastSaveTime;
        public float remainingSeconds;
    }

    private void HeartSaveData()
    {
        int remainingSeconds;
        string lastSaveTime;
        string authJson = Manager.DB.GetAuthInfo();
        DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);

        // 하트 시스템 저장안돼요. 수정 하지 마세요~
        Manager.DB.dbRef
            .Child(info.type)
            .Child(info.uid)
            .Child("heart")
            .Child("currentHeart")
            .SetValueAsync(_currentHearts);

        if (_currentHearts >= _maxHearts)
        {
            remainingSeconds = 0;
            lastSaveTime = "";
        }
        else
        {
            remainingSeconds = _startSeconds;
            lastSaveTime = DateTime.Now.ToString();
        }

        Manager.DB.dbRef
            .Child(info.type)
            .Child(info.uid)
            .Child("heart")
            .Child("remainingSeconds")
            .SetValueAsync(remainingSeconds);

        // lastSaveTime 저장
        Manager.DB.dbRef
            .Child(info.type)
            .Child(info.uid)
            .Child("heart")
            .Child("lastSaveTime")
            .SetValueAsync(lastSaveTime);
    }

    /// <summary>
    /// 
    /// 하트를 사용하여 스테이지를 시작합니다.
    /// </summary>
    /// <param name="requiredHearts"></param>
    /// <returns></returns>
    public bool TryStartStage()
    {
        if (_currentHearts != 0)
        {
            // _currentHearts -= requiredHearts;
            // UpdateHeartUI();
            // StartCoroutine(CalcHeartData());
            // StartCoroutine(HeartSaveData());
            // Debug.Log($"스테이지 시작! 하트 {requiredHearts}개 사용, 남은 하트: {_currentHearts}");
            Debug.Log($"스테이지 시작!");
            return true;
        }
        else
        {
            Debug.Log("하트 부족! 스테이지 시작 불가");
            return false;
        }
    }

    /// <summary>
    /// 스테이지 실패시 하트를 소모합니다.
    /// </summary>
    public void UseHearts()
    {
        if (_currentHearts > 0)
        {
            _currentHearts--;
        }
        UpdateHeartUI();
        HeartSaveData();
    }

    /// <summary>
    /// 하트를 회복(채우기)합니다.
    /// 최대치 제한 없음
    /// </summary>
    /// <param name="amount">회복할 하트 개수</param>
    public void AddHearts(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("회복할 하트 개수가 0 이하입니다.");
            return;
        }

        float beforeHearts = _currentHearts;
        _currentHearts += amount; // 제한 없이 누적

        UpdateHeartUI();
        HeartSaveData();

        Debug.Log($"하트 {amount}개 회복! ({beforeHearts} → {_currentHearts})");
    }

    /// <summary>
    /// 하트 UI를 업데이트합니다.
    /// </summary>
    private void UpdateHeartUI()
    {
        if (_textHeart != null)
            _textHeart.text = $"{_currentHearts}";
    }

    /// <summary>
    /// 하트 타이머
    /// 시간이 다 되면 하트를 충전합니다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator TimerCoroutine()
    {
        while (true) // 무한 루프 → 내부에서 조건으로 제어
        {
            // 하트가 최대치라면 충전하지 않고 리턴
            if (_currentHearts >= _maxHearts)
            {
                yield return null;
                continue;
            }

            if (_remainingSeconds > 0)
            {
                _remainingSeconds -= Time.unscaledDeltaTime;
                yield return null;
                UpdateTimerUI();
            }
            else
            {
                // 시간이 다 되었을 때
                if (_currentHearts < _maxHearts)
                {
                    _currentHearts++;
                    UpdateHeartUI();
                    Debug.Log("하트 충전! 현재 하트: " + _currentHearts);

                    // 다시 카운트다운 초기화
                    _remainingSeconds = _startSeconds;
                    UpdateTimerUI();
                    // CalcHeartData();
                    StartCoroutine(CalcHeartData());
                    HeartSaveData();
                }
            }

        }
    }


    /// <summary>
    /// 타이머 UI를 업데이트합니다.
    /// 하트가 가득찬경우 FULL로 표시합니다.
    /// </summary>
    private void UpdateTimerUI()
    {
        if (_timerText == null) return;

        if (_currentHearts >= _maxHearts)
        {
            _timerText.text = "가득 참";
            return;
        }
        int minutes = (int)_remainingSeconds / 60;
        int seconds = (int)_remainingSeconds % 60;
        _timerText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

    /// <summary>
    /// 애플리케이션이 종료될 때 하트 데이터를 저장합니다.
    /// </summary>
    private void OnApplicationQuit()
    {
        Quit();
    }

    /// <summary>
    /// 애플리케이션이 백그라운드로 갔을 때 하트 데이터를 저장합니다.
    /// </summary>
    /// <param name="pause"></param>
    // private void OnApplicationPause(bool pause)
    // {
        // if (pause)
        // {
            // Quit();
        // }
    // }

    private void Quit()
    {
        if (isPlaying)
        {
            UseHearts();
        }
        StartCoroutine(CalcHeartData());
        HeartSaveData();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// 씬이 비활성화될 때 이벤트 구독을 해제합니다.
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새 씬이 로드될 때마다 UI 다시 찾아 연결
        _timerText = GameObject.Find("TextTimer")?.GetComponent<TMP_Text>();
        _textHeart = GameObject.Find("TextHeart")?.GetComponent<TMP_Text>();

        // UI 즉시 갱신
        UpdateHeartUI();
        UpdateTimerUI();
    }

    public int GetMaxHearts()
    {
        return _maxHearts;
    }
}
