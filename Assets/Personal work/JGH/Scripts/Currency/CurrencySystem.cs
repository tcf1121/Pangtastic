using Firebase.Database;
using System;
using System.Collections;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;


public class CurrencySystem : MonoBehaviour
{
    public static CurrencySystem Instance { get; private set; } // 싱글톤 인스턴스

    // [System.Serializable]
    // public class CoinData
    // {
    // public int coins; // 보유 코인 수
    // }
    // [System.Serializable]
    // public class StarData
    // {
    // public int stars; // 보유 코인 수
    // }

    public enum SpawnType { None, Continue, Exit }
    public SpawnType pendingSpawnType = SpawnType.None;

    [SerializeField] private GameObject _coinPrefab; // 코인 프리팹 
    [SerializeField] private GameObject _starPrefab; // 별 프리팹

    private int _currentCoins = 0; // 현재 보유 코인 수
    // private string _saveCoinPath; // JSON 저장 경로

    private int _currentStars = 0; // 현재 보유 코인 수
    // private string _saveStarPath; // JSON 저장 경로

    protected void Awake()
    {
        //base.Awake();
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

    /// <summary>
    /// 코인 추가
    /// </summary>
    public void AddCoin(int amount)
    {
        _currentCoins += amount;  // 코인 증가
        CoinSave();                  // JSON 저장
    }

    /// <summary>
    /// 별 추가
    /// </summary>
    public void AddStar(int amount)
    {
        _currentStars += amount;  // 코인 증가
        StarSave();                  // JSON 저장
    }

    //TODO: TEST 제거 해도됨 
    public void useCoin(int amount)
    {
        SpendCoin(amount);
    }
    //TODO: TEST 제거 해도됨 
    public void useStar(int amount)
    {
        SpendStar(amount);
    }
    //TODO: TEST 제거 해도됨 
    public void TestShowCoins()
    {
        Debug.Log($"GetCoins : {GetCoins()}");
    }
    //TODO: TEST 제거 해도됨 
    public void TestShowStars()
    {
        Debug.Log($"GetStars : {GetStars()}");
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
            return true;
        }
        return false; // 코인이 부족하면 실패
    }


    /// <summary>
    /// 현재 코인 수 가져오기
    /// </summary>
    public int GetCoins()
    {
        StartCoroutine(CoinLoad());
        return _currentCoins;
    }

    /// <summary>
    /// 현재 코인 수 가져오기
    /// </summary>
    public int GetStars()
    {
        StartCoroutine(StarLoad());
        return _currentStars;
    }



    /// <summary>
    /// JSON 파일로 저장
    /// </summary>
    private void CoinSave()
    {
        string authJson = Manager.DB.GetAuthInfo();
        DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);

        Manager.DB.dbRef
            .Child("users")
            .Child(info.uid)
            .Child("coin")
            .Child("currentCoin")
            .SetValueAsync(_currentCoins);

    }

    /// <summary>
    /// JSON 파일로 저장
    /// </summary>
    private void StarSave()
    {
        string authJson = Manager.DB.GetAuthInfo();
        DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);

        Manager.DB.dbRef
            .Child("users")
            .Child(info.uid)
            .Child("star")
            .Child("currentStar")
            .SetValueAsync(_currentStars);

    }

    /// <summary>
    /// JSON 파일에서 불러오기
    /// </summary>
    private IEnumerator CoinLoad()
    {
        string authJson = Manager.DB.GetAuthInfo();
        DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);

        var task = Manager.DB.dbRef
            .Child("users")
            .Child(info.uid)
            .Child("coin")
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
            _currentCoins = int.Parse(snapshot.Child("currentCoin").Value.ToString());
        }
        else
        {
            _currentCoins = 0;
        }
        // if (File.Exists(_saveCoinPath))
        // {
        // string json = File.ReadAllText(_saveCoinPath);          // JSON 파일 읽기
        // CoinData data = JsonUtility.FromJson<CoinData>(json); // JSON → 객체 변환
        // _currentCoins = data.coins;
        // }
        // else
        // {
        // _currentCoins = 0; // 파일이 없으면 기본값 0
        // }
    }

    /// <summary>
    /// JSON 파일에서 불러오기
    /// </summary>
    private IEnumerator StarLoad()
    {
        string authJson = Manager.DB.GetAuthInfo();
        DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);

        var task = Manager.DB.dbRef
            .Child("users")
            .Child(info.uid)
            .Child("star")
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
            _currentStars = int.Parse(snapshot.Child("currentStar").Value.ToString());
        }
        else
        {
            _currentStars = 0;
        }
        // if (File.Exists(_saveStarPath))
        // {
        // string json = File.ReadAllText(_saveStarPath);          // JSON 파일 읽기
        // StarData data = JsonUtility.FromJson<StarData>(json); // JSON → 객체 변환
        // _currentStars = data.stars;
        // }
        // else
        // {
        // _currentStars = 0; // 파일이 없으면 기본값 0
        // }
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
            //if (tmp) SetCoinText(tmp);
        }

        var startText = GameObject.FindWithTag("StarText");
        if (startText)
        {
            var tmp = startText.GetComponent<TMPro.TMP_Text>();
            //if (tmp) SetStarText(tmp);
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

        Manager.Effect.CurrencyInPlayStartEffect(_coinPrefab, spawn, GameObject.FindWithTag("CoinTargetUI").GetComponent<RectTransform>());
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

        Manager.Effect.CurrencyInPlayStartEffect(_starPrefab, startspawn, GameObject.FindWithTag("StarTargetUI").GetComponent<RectTransform>());
        // 별 :: E

        pendingSpawnType = SpawnType.None;
    }
}

