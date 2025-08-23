using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartSystem : MonoBehaviour
{
    [Serializable]
    public class HeartData
    {
        public int currentHearts;
        public string lastSaveTime;
        public int remainingSeconds; 
    }
    
    private string SavePath => System.IO.Path.Combine(Application.persistentDataPath, "HeartData.json"); 
    
    public static HeartSystem Instance { get; private set; }
    
    [SerializeField] private TMP_Text _timerText; // UI Text (MM:SS 표시)
    [SerializeField] private TMP_Text _textHeart; // 
    [SerializeField] private int _startSeconds = 1800; // 시작 시간 (기본 30분, 초 단위)
    [SerializeField] private int _maxHearts = 5; // 최대 하트 개수
    [SerializeField] private int _currentHearts = 0; // 현재 하트 개수

    private int _remainingSeconds;


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
        // 혹시 중복 실행된 코루틴이 있으면 정리
        StopAllCoroutines();
        
        HeartLoadData();
        HeartSaveData();

        UpdateHeartUI();
        
        // 초기 시간 설정
        UpdateTimerUI();

        // 타이머 시작
        StartCoroutine(TimerCoroutine());
    }
    
    // TODO: 파이어 베이스로 변경 필요
    private void HeartLoadData()
    {
        if (System.IO.File.Exists(SavePath))
        {
            string json = System.IO.File.ReadAllText(SavePath);
            HeartData data = JsonUtility.FromJson<HeartData>(json);

            _currentHearts = data.currentHearts;

            if (!string.IsNullOrEmpty(data.lastSaveTime))
            {
                DateTime lastTime = DateTime.Parse(data.lastSaveTime);
                TimeSpan diff = DateTime.Now - lastTime;

                // 지난 시간만큼 하트 충전
                int recoveredHearts = (int)(diff.TotalSeconds / _startSeconds);
                _currentHearts = Mathf.Min(_currentHearts + recoveredHearts, _maxHearts);

                if (_currentHearts < _maxHearts)
                {
                    // 남은 시간 계산 (기존 저장된 남은 시간에서 경과 시간 빼기)
                    _remainingSeconds = data.remainingSeconds - (int)diff.TotalSeconds;

                    if (_remainingSeconds <= 0)
                    {
                        // 부족하면 추가로 하트 충전
                        int extraHearts = Mathf.Abs(_remainingSeconds) / _startSeconds + 1;
                        _currentHearts = Mathf.Min(_currentHearts + extraHearts, _maxHearts);

                        // 남은 시간 재설정
                        if (_currentHearts < _maxHearts)
                            _remainingSeconds = _startSeconds - (Mathf.Abs(_remainingSeconds) % _startSeconds);
                        else
                            _remainingSeconds = 0;
                    }
                }
                else
                {
                    _remainingSeconds = 0;
                }
            }
            else
            {
                _remainingSeconds = _startSeconds;
            }

            Debug.Log("JSON 불러오기 완료");
        }
        else
        {
            _currentHearts = _maxHearts;
            _remainingSeconds = _startSeconds;
            Debug.Log("JSON 파일 없음, 기본값으로 시작");
        }
    }



    /// <summary>
    /// 하트를 사용하여 스테이지를 시작합니다.
    /// </summary>
    /// <param name="requiredHearts"></param>
    /// <returns></returns>
    public bool TryUseHearts(int requiredHearts)
    {
        if (_currentHearts >= requiredHearts)
        {
            _currentHearts -= requiredHearts;
            UpdateHeartUI();
            HeartSaveData();
            Debug.Log($"스테이지 시작! 하트 {requiredHearts}개 사용, 남은 하트: {_currentHearts}");
            return true;
        }
        else
        {
            Debug.Log("하트 부족! 스테이지 시작 불가");
            return false;
        }
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
        HeartSaveData();

        Debug.Log($"하트 {amount}개 회복! ({beforeHearts} → {_currentHearts})");
    }
    
    /// <summary>
    /// 하트 UI를 업데이트합니다.
    /// </summary>
    private void UpdateHeartUI()
    {
        if (_textHeart != null)
            _textHeart.text = $"{_currentHearts}/{_maxHearts}";
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
                // if (_timerText != null)
                    // _timerText.gameObject.SetActive(false);

                yield return new WaitForSeconds(1f);
                continue;
            }

            if (_remainingSeconds > 0)
            {
                yield return new WaitForSeconds(1f);

                // 1초 감소
                _remainingSeconds--;

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

        if (_currentHearts == _maxHearts)
        {
            _timerText.text = "FULL";
            return;
        }
        
        int minutes = _remainingSeconds / 60;
        int seconds = _remainingSeconds % 60;
        _timerText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }
    
    /// <summary>
    /// 애플리케이션이 종료될 때 하트 데이터를 저장합니다.
    /// </summary>
    private void OnApplicationQuit()
    {
        HeartSaveData(); // 정상 종료 시 저장
    }

    /// <summary>
    /// 애플리케이션이 백그라운드로 갔을 때 하트 데이터를 저장합니다.
    /// </summary>
    /// <param name="pause"></param>
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            HeartSaveData(); // 앱이 백그라운드로 갔을 때 저장
        }
    }

    // TODO: 파이어 베이스로 변경 필요
    private void HeartSaveData()
    {
        HeartData data = new HeartData
        {
            currentHearts = _currentHearts,
            lastSaveTime = (_currentHearts < _maxHearts) ? DateTime.Now.ToString() : "",
            remainingSeconds = (_currentHearts < _maxHearts) ? _remainingSeconds : 0
        };

        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(SavePath, json);

        Debug.Log("JSON 저장 완료: " + SavePath);
    }
    
    /// <summary>
    /// 씬이 로드될 때마다 UI를 다시 찾아 연결합니다.
    /// </summary>
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
}
