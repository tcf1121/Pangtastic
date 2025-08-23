using UnityEngine;
using TMPro;
using System.IO;


public class CoinSystem : MonoBehaviour
{
    [System.Serializable] 
    public class CoinData
    {
        public int coins; // 보유 코인 수
    }
    
    public static CoinSystem Instance { get; private set; } // 싱글톤 인스턴스

    [SerializeField] private TMP_Text _coinText; // Coin 갯수
    
    private int _currentCoins = 0; // 현재 보유 코인 수
    private string _savePath; // JSON 저장 경로

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

        // 플랫폼별 JSON 저장 경로 지정
        _savePath = Path.Combine(Application.persistentDataPath, "coinData.json");

        // 기존 저장된 데이터 불러오기
        CoinLoad();

        // UI 갱신
        UpdateCoinUI();
    }
    
    public void SetCoinText(TMP_Text text)
    {
        _coinText = text;
        UpdateCoinUI(); // 바로 최신 값 반영
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
    /// 현재 코인 수 가져오기
    /// </summary>
    public int GetCoins()
    {
        return _currentCoins;
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
    /// JSON 파일로 저장
    /// </summary>
    private void CoinSave()
    {
        CoinData data = new CoinData { coins = _currentCoins }; // 데이터 클래스에 값 대입
        string json = JsonUtility.ToJson(data, true);          // 객체 → JSON 변환
        File.WriteAllText(_savePath, json);                     // 파일에 저장
    }

    /// <summary>
    /// JSON 파일에서 불러오기
    /// </summary>
    private void CoinLoad()
    {
        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);          // JSON 파일 읽기
            CoinData data = JsonUtility.FromJson<CoinData>(json); // JSON → 객체 변환
            _currentCoins = data.coins;
        }
        else
        {
            _currentCoins = 0; // 파일이 없으면 기본값 0
        }
    }
}
