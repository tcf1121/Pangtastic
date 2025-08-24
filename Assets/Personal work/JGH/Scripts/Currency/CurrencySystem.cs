using System;
using System.Collections;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;


public class CurrencySystem : MonoBehaviour
{
    public static CurrencySystem Instance { get; private set; } // 싱글톤 인스턴스
    
    [System.Serializable] 
    public class CoinData
    {
        public int coins; // 보유 코인 수
    }
    [System.Serializable] 
    public class StarData
    {
        public int stars; // 보유 코인 수
    }
    
    public enum SpawnType { None, Continue, Exit }
    public SpawnType pendingSpawnType = SpawnType.None;
    
    [SerializeField] private GameObject _coinPrefab; // 코인 프리팹 
    [SerializeField] private GameObject _starPrefab; // 별 프리팹
    
    [SerializeField] private TMP_Text _coinText; // Coin 갯수
    [SerializeField] private TMP_Text _starText; // 별 갯수
    
    private int _currentCoins = 0; // 현재 보유 코인 수
    private string _saveCoinPath; // JSON 저장 경로

    private int _currentStars = 0; // 현재 보유 코인 수
    private string _saveStarPath; // JSON 저장 경로
    
    private void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지
    }

    private void Start()
    {
        // 플랫폼별 JSON 저장 경로 지정
        _saveCoinPath = Path.Combine(Application.persistentDataPath, "CoinData.json");
        _saveStarPath = Path.Combine(Application.persistentDataPath, "StarData.json");

        // 기존 저장된 데이터 불러오기
        CoinLoad();
        StarLoad();

        // UI 갱신
        UpdateCoinUI();
        UpdateStarUI();
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
        if (scene.name != "JGH_OutGameUI") return;

        StartCoroutine(SetupAndPlayEffect());
    }
    
    public void SetCoinText(TMP_Text text)
    {
        _coinText = text;
        UpdateCoinUI(); // 바로 최신 값 반영
    }
    
    public void SetStarText(TMP_Text text)
    {
        _starText = text;
        UpdateStarUI(); // 바로 최신 값 반영
    }

    /// <summary>
    /// 코인 추가
    /// </summary>
    public void AddCoin(int amount)
    {
        _currentCoins += amount;  // 코인 증가
        CoinSave();                  // JSON 저장
        UpdateCoinUI();          // UI 갱신
    }
    
    /// <summary>
    /// 별 추가
    /// </summary>
    public void AddStar(int amount)
    {
        _currentStars += amount;  // 코인 증가
        StarSave();                  // JSON 저장
        UpdateStarUI();          // UI 갱신
    }
    
    /// <summary>
    /// 코인 사용 (부족하면 false 반환)
    /// </summary>
    public bool SpendCoin(int amount)
    {
        if (_currentCoins >= amount)
        {
            _currentCoins -= amount; // 코인 차감
            CoinSave();                 // JSON 저장
            UpdateCoinUI();         // UI 갱신
            return true;
        }
        return false; // 코인이 부족하면 실패
    }
    
    /// <summary>
    /// 별 사용 (부족하면 false 반환)
    /// </summary>
    public bool SpendStar(int amount)
    {
        if (_currentStars >= amount)
        {
            _currentStars -= amount; // 코인 차감
            StarSave();                 // JSON 저장
            UpdateStarUI();         // UI 갱신
            return true;
        }
        return false; // 코인이 부족하면 실패
    }

    /// <summary>
    /// 현재 코인 수 가져오기
    /// </summary>
    public int GetCoins()
    {
        return _currentCoins;
    }

    /// <summary>
    /// 현재 코인 수 가져오기
    /// </summary>
    public int GetStars()
    {
        return _currentStars;
    }
    
    /// <summary>
    /// UI(TextMeshPro) 업데이트
    /// </summary>
    private void UpdateCoinUI()
    {
        if (_coinText != null)
            _coinText.text = _currentCoins.ToString();
    }
    
    /// <summary>
    /// UI(TextMeshPro) 업데이트
    /// </summary>
    private void UpdateStarUI()
    {
        if (_starText != null)
            _starText.text = _currentStars.ToString();
    }

    /// <summary>
    /// JSON 파일로 저장
    /// </summary>
    private void CoinSave()
    {
        CoinData data = new CoinData { coins = _currentCoins }; // 데이터 클래스에 값 대입
        string json = JsonUtility.ToJson(data, true);          // 객체 → JSON 변환
        File.WriteAllText(_saveCoinPath, json);                     // 파일에 저장
    }
    
    /// <summary>
    /// JSON 파일로 저장
    /// </summary>
    private void StarSave()
    {
        StarData data = new StarData { stars = _currentStars }; // 데이터 클래스에 값 대입
        string json = JsonUtility.ToJson(data, true);          // 객체 → JSON 변환
        File.WriteAllText(_saveStarPath, json);                     // 파일에 저장
    }

    /// <summary>
    /// JSON 파일에서 불러오기
    /// </summary>
    private void CoinLoad()
    {
        if (File.Exists(_saveCoinPath))
        {
            string json = File.ReadAllText(_saveCoinPath);          // JSON 파일 읽기
            CoinData data = JsonUtility.FromJson<CoinData>(json); // JSON → 객체 변환
            _currentCoins = data.coins;
        }
        else
        {
            _currentCoins = 0; // 파일이 없으면 기본값 0
        }
    }
    
    /// <summary>
    /// JSON 파일에서 불러오기
    /// </summary>
    private void StarLoad()
    {
        if (File.Exists(_saveStarPath))
        {
            string json = File.ReadAllText(_saveStarPath);          // JSON 파일 읽기
            StarData data = JsonUtility.FromJson<StarData>(json); // JSON → 객체 변환
            _currentStars = data.stars;
        }
        else
        {
            _currentStars = 0; // 파일이 없으면 기본값 0
        }
    }
    
    private IEnumerator SetupAndPlayEffect()
    {
        yield return null; // UI 로딩 대기
        yield return null; // UI 로딩 대기
        yield return null; // UI 로딩 대기
        
        var coinText = GameObject.FindWithTag("CoinText");
        if (coinText)
        {
            var tmp = coinText.GetComponent<TMPro.TMP_Text>();
            if (tmp) SetCoinText(tmp);
        }
        
        var startText = GameObject.FindWithTag("StarText");
        if (startText)
        {
            var tmp = startText.GetComponent<TMPro.TMP_Text>();
            if (tmp) SetStarText(tmp);
        }

        if (pendingSpawnType == SpawnType.None)
        {
            yield break;
        }
        
        // 코인 :: S
        RectTransform spawn = null;

        if (pendingSpawnType == SpawnType.Continue)
        {
            var go = GameObject.FindWithTag("CurrencySpawnContinue");
            if (go) spawn = go.GetComponent<RectTransform>();
        }
        else if (pendingSpawnType == SpawnType.Exit)
        {
            var go = GameObject.FindWithTag("CurrencySpawnExit");
            if (go) spawn = go.GetComponent<RectTransform>();
        }

        EffectSystem.Instance.CurrencyInPlayStartEffect(_coinPrefab, spawn, GameObject.FindWithTag("CoinTargetUI").GetComponent<RectTransform>());
        // 코인 :: E
        
        // 별 :: S
        RectTransform startspawn = null;

        if (pendingSpawnType == SpawnType.Continue)
        {
            var go = GameObject.FindWithTag("CurrencySpawnContinue");
            if (go) startspawn = go.GetComponent<RectTransform>();
        }
        else if (pendingSpawnType == SpawnType.Exit)
        {
            var go = GameObject.FindWithTag("CurrencySpawnExit");
            if (go) startspawn = go.GetComponent<RectTransform>();
        }

        EffectSystem.Instance.CurrencyInPlayStartEffect(_starPrefab, startspawn, GameObject.FindWithTag("StarTargetUI").GetComponent<RectTransform>());
        // 별 :: E

        pendingSpawnType = SpawnType.None;
    }
}

