using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Serialization;

public class CoinRewardEffectSystem : MonoBehaviour
{
    public static CoinRewardEffectSystem Instance { get; private set; } // 싱글톤 인스턴스
    
    public enum SpawnType { None, Continue, Exit }
    public SpawnType pendingSpawnType = SpawnType.None;
    
    [SerializeField] private GameObject _coinPrefab; // 코인 프리팹 (UI Image)
    [SerializeField] private RectTransform _spawnArea; // 코인이 생성될 시작 영역 (예: 버튼 위치)
    [SerializeField] private RectTransform _targetUI;  // 코인 UI 목표 지점 (예: 상단 코인 아이콘)
    
    [SerializeField] private float _spawnInterval = 0.05f; // 생성 간격
    [SerializeField] private float _moveTime = 0.8f;       // 이동 시간

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

    public void CoinPlayEffect()
    {
        StartCoroutine(PlayCoinEffect());
    }
    
    public void SetSpawnArea(RectTransform rt) => _spawnArea = rt;
    public void SetTargetUI(RectTransform rt) => _targetUI = rt;
    
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

    private IEnumerator SetupAndPlayEffect()
    {
        yield return null; // UI 로딩 대기
        yield return null; // UI 로딩 대기
        yield return null; // UI 로딩 대기
        
        var textGo = GameObject.FindWithTag("CoinText");
        if (textGo)
        {
            var tmp = textGo.GetComponent<TMPro.TMP_Text>();
            if (tmp) CoinSystem.Instance.SetCoinText(tmp);
        }

        if (pendingSpawnType == SpawnType.None)
        {
            yield break;
        }

        RectTransform spawn = null;

        if (pendingSpawnType == SpawnType.Continue)
        {
            var go = GameObject.FindWithTag("CoinSpawnContinue");
            if (go) spawn = go.GetComponent<RectTransform>();
        }
        else if (pendingSpawnType == SpawnType.Exit)
        {
            var go = GameObject.FindWithTag("CoinSpawnExit");
            if (go) spawn = go.GetComponent<RectTransform>();
        }

        if (spawn != null)
            SetSpawnArea(spawn);

        var targetGo = GameObject.FindWithTag("CoinTargetUI");
        if (targetGo) 
            SetTargetUI(targetGo.GetComponent<RectTransform>());

        if (spawn != null && targetGo != null)
            CoinPlayEffect();

        pendingSpawnType = SpawnType.None;
    }

    private IEnumerator PlayCoinEffect()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject coin = Instantiate(_coinPrefab, _spawnArea.position, Quaternion.identity, _spawnArea.parent);
            RectTransform coinRect = coin.GetComponent<RectTransform>();
            Image coinImage = coin.GetComponent<Image>();

            // 시작 offset
            Vector3 startPos = _spawnArea.position + new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), 0);
            coinRect.position = startPos;

            // 중간 제어 포인트
            Vector3 controlPos = startPos + new Vector3(Random.Range(-50f, 50f), Random.Range(150f, 250f), 0);

            Vector3[] path = new Vector3[]
            {
                controlPos,
                _targetUI.position
            };

            // 이동 & 알파 동시 처리
            Sequence seq = DOTween.Sequence();

            // 1) 경로 이동
            seq.Append(coinRect.DOPath(path, _moveTime, PathType.CatmullRom)
                .SetEase(Ease.InOutQuad));

            // 2) 이동하면서 투명도 0으로
            seq.Join(coinImage.DOFade(0f, _moveTime));

            // 3) 완료 후 파괴 및 코인 갯수 증가
            seq.OnComplete(() =>
            {
                Destroy(coin);
            });

            yield return new WaitForSeconds(_spawnInterval);
        }
    }
}