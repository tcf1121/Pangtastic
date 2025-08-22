using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartSystem : MonoBehaviour
{
    
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

        UpdateHeartUI();
        
        // 초기 시간 설정
        UpdateTimerUI();

        // 타이머 시작
        StartCoroutine(TimerCoroutine());
    }
    
    // TODO: 파이어 베이스로 변경 필요
    private void HeartLoadData()
    {
        _currentHearts = PlayerPrefs.GetInt("CurrentHearts", 0);

        string lastTimeStr = PlayerPrefs.GetString("LastSaveTime", "");
        if (!string.IsNullOrEmpty(lastTimeStr))
        {
            DateTime lastTime = DateTime.Parse(lastTimeStr);
            TimeSpan diff = DateTime.Now - lastTime;

            int recoveredHearts = (int)(diff.TotalSeconds / _startSeconds);
            _currentHearts = Mathf.Min(_currentHearts + recoveredHearts, _maxHearts);

            int leftoverSeconds = (int)(diff.TotalSeconds % _startSeconds);

            if (_currentHearts < _maxHearts)
                _remainingSeconds = _startSeconds - leftoverSeconds;
            else
                _remainingSeconds = 0;
        }
        else
        {
            _remainingSeconds = _startSeconds;
        }
    }
    
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
    
    private void UpdateHeartUI()
    {
        if (_textHeart != null)
            _textHeart.text = $"{_currentHearts}/{_maxHearts}";
    }

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

    private void UpdateTimerUI()
    {
        if (_timerText == null) return;

        if (_currentHearts >= _maxHearts)
        {
            _timerText.text = "FULL";
            return;
        }
        
        int minutes = _remainingSeconds / 60;
        int seconds = _remainingSeconds % 60;
        _timerText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }
    
    private void OnApplicationQuit()
    {
        HeartSaveData(); // 정상 종료 시 저장
    }

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
        PlayerPrefs.SetInt("CurrentHearts", _currentHearts);
        PlayerPrefs.SetString("LastSaveTime", DateTime.Now.ToString());
        PlayerPrefs.Save();

        Debug.Log("PlayerPrefs 저장 완료");
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

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
