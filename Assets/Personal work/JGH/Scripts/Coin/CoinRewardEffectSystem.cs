using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class CoinRewardEffectSystem : MonoBehaviour
{
    public static CoinRewardEffectSystem Instance { get; private set; } // 싱글톤 인스턴스
    
    [SerializeField] private GameObject coinPrefab; // 코인 프리팹 (UI Image)
    [SerializeField] private RectTransform spawnArea; // 코인이 생성될 시작 영역 (예: 버튼 위치)
    [SerializeField] private RectTransform targetUI;  // 코인 UI 목표 지점 (예: 상단 코인 아이콘)

    [SerializeField] private float spawnInterval = 0.05f; // 생성 간격
    [SerializeField] private float moveTime = 0.8f;       // 이동 시간

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
    
    public void CoinPlayEffect(int addCoin)
    {
        StartCoroutine(PlayCoinEffect(addCoin));
    }

    private IEnumerator PlayCoinEffect(int addCoin)
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject coin = Instantiate(coinPrefab, spawnArea.position, Quaternion.identity, spawnArea.parent);
            RectTransform coinRect = coin.GetComponent<RectTransform>();
            Image coinImage = coin.GetComponent<Image>();

            // 시작 offset
            Vector3 startPos = spawnArea.position + new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), 0);
            coinRect.position = startPos;

            // 중간 제어 포인트
            Vector3 controlPos = startPos + new Vector3(Random.Range(-50f, 50f), Random.Range(150f, 250f), 0);

            Vector3[] path = new Vector3[]
            {
                controlPos,
                targetUI.position
            };

            // 이동 & 알파 동시 처리
            Sequence seq = DOTween.Sequence();

            // 1) 경로 이동
            seq.Append(coinRect.DOPath(path, moveTime, PathType.CatmullRom)
                .SetEase(Ease.InOutQuad));

            // 2) 이동하면서 투명도 0으로
            seq.Join(coinImage.DOFade(0f, moveTime));

            // 3) 완료 후 파괴 및 코인 갯수 증가
            seq.OnComplete(() =>
            {
                Destroy(coin);
                CoinSystem.Instance.AddCoin(addCoin);
            });

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}