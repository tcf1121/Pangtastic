using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartSystem : MonoBehaviour
{
    public static HeartSystem Instance { get; private set; }

    [SerializeField] private TMP_Text _timerText; // UI Text (MM:SS 표시)
    [SerializeField] private TMP_Text _textHeart; // 0/0 표시

    [SerializeField] private int _startSeconds = 1800; // 시작 시간 (기본 30분, 초 단위)
    [SerializeField] private int _maxHearts = 5; // 최대 하트 개수
    [SerializeField] private int _currentHearts = 0; // 현재 하트 개수

    private float _remainingSeconds;

    protected void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StopAllCoroutines(); // 혹시 중복 실행된 코루틴이 있으면 정리
        StartCoroutine(CalcHeartData());
    }


    private IEnumerator CalcHeartData()
    {
        string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        var task = DatabaseSystem.Instance.GetUserPath(uid)
            .Child("heart")
            .GetValueAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("불러오기 실패: " + task.Exception);
            yield break;
        }

        DataSnapshot snapshot = task.Result;
        if (snapshot.Exists && snapshot.Value != null)
        {
            _currentHearts = int.Parse(snapshot.Value.ToString());
        }
        else
        {
            _currentHearts = _maxHearts;
        }

        Debug.Log($"현재 하트: {_currentHearts}");

        if (task.IsFaulted)
        {
            Debug.LogError("하트 데이터 불러오기 실패: " + task.Exception);
            // 기본값 세팅
            _currentHearts = _maxHearts;
            _remainingSeconds = _startSeconds;

        }
        else if (task.IsCompleted)
        {
            if (snapshot.Exists)
            {
                string currentHeart = snapshot.Child("currentHeart").Value?.ToString();
                string lastSaveTime = snapshot.Child("lastSaveTime").Value?.ToString();
                string remainingSeconds = snapshot.Child("remainingSeconds").Value?.ToString();

                _currentHearts = int.Parse(currentHeart);
                _remainingSeconds = int.Parse(remainingSeconds);

                DateTime lastTime = DateTime.Parse(lastSaveTime);
                TimeSpan diff = DateTime.Now - lastTime;

                // 지난 시간만큼 하트 충전
                int recoveredHearts = (int)(diff.TotalSeconds / _startSeconds);
                _currentHearts = Mathf.Min(_currentHearts + recoveredHearts, _maxHearts);

                if (_currentHearts < _maxHearts)
                {
                    _remainingSeconds -= (int)diff.TotalSeconds;

                    if (_remainingSeconds <= 0)
                    {
                        // 부족하면 추가로 하트 충전
                        int extraHearts = Mathf.Abs((int)_remainingSeconds) / _startSeconds + 1;

                        _currentHearts = Mathf.Min(_currentHearts + extraHearts, _maxHearts);

                        if (_currentHearts < _maxHearts)
                            _remainingSeconds =
                                _startSeconds - (Mathf.Abs(_remainingSeconds) % _startSeconds);
                        else
                            _remainingSeconds = 0;
                    }
                }
                else
                {
                    _remainingSeconds = 0;
                }

                Debug.Log($"[Firebase Load] Hearts: {_currentHearts}, RemainSec: {_remainingSeconds}");
            }
            else
            {
                // 데이터가 아예 없는 경우 → 처음 시작
                _currentHearts = _maxHearts;
                _remainingSeconds = _startSeconds;

                Debug.Log("Firebase에 하트 기본값 저장 완료");
            }
        }

        UpdateHeartUI();
        UpdateTimerUI();
        StartCoroutine(TimerCoroutine());
        StartCoroutine(HeartSaveData());
    }

    private IEnumerator HeartSaveData()
    {
        string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogError("UID가 비어있습니다! 로그인 완료 후 호출하세요.");
            yield break;
        }

        var task = DatabaseSystem.Instance.GetUserPath(uid)
        .Child("heart")
        .Child("currentHeart")
        .SetValueAsync(_currentHearts);

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("currentHeart 저장 실패: " + task.Exception);
            yield break;
        }

        if (_currentHearts >= _maxHearts)
        {
            // remainingSeconds 저장
            var taskRemain = DatabaseSystem.Instance.GetUserPath(uid)
                .Child("heart")
                .Child("remainingSeconds")
                .SetValueAsync(0);
            yield return new WaitUntil(() => taskRemain.IsCompleted);

            // lastSaveTime 저장
            var taskLast = DatabaseSystem.Instance.GetUserPath(uid)
                .Child("heart")
                .Child("lastSaveTime")
                .SetValueAsync("");
            yield return new WaitUntil(() => taskLast.IsCompleted);
        }
        else
        {
            // remainingSeconds 저장
            var taskRemain = DatabaseSystem.Instance.GetUserPath(uid)
                .Child("heart")
                .Child("remainingSeconds")
                .SetValueAsync(_remainingSeconds);
            yield return new WaitUntil(() => taskRemain.IsCompleted);

            // lastSaveTime 저장
            var taskLast = DatabaseSystem.Instance.GetUserPath(uid)
                .Child("heart")
                .Child("lastSaveTime")
                .SetValueAsync(DateTime.Now.ToString("O")); // ISO 8601 형식
            yield return new WaitUntil(() => taskLast.IsCompleted);
        }
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

        int beforeHearts = _currentHearts;
        _currentHearts += amount; // 제한 없이 누적

        UpdateHeartUI();
        StartCoroutine(HeartSaveData());

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
                    StartCoroutine(HeartSaveData());
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
        StartCoroutine(CalcHeartData());
        StartCoroutine(HeartSaveData());
    }

    /// <summary>
    /// 애플리케이션이 백그라운드로 갔을 때 하트 데이터를 저장합니다.
    /// </summary>
    /// <param name="pause"></param>
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            StartCoroutine(CalcHeartData());
            StartCoroutine(HeartSaveData());
        }
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
