using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Serialization;

public class EffectSystem : MonoBehaviour
{
    public static EffectSystem Instance { get; private set; } // 싱글톤 인스턴스
    
    [SerializeField] private float _spawnInterval = 0.05f; // 생성 간격
    [SerializeField] private float _moveTime = 0.8f;       // 이동 시간
    [SerializeField] public float _fadeTime = 1.3f; // 투명도 시간

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
    
    public void CurrencyInPlayStartEffect(GameObject prefab, RectTransform spawnArea, RectTransform targetUI)
    {
        StartCoroutine(CurrencyInPlayEffect(prefab, spawnArea, targetUI));
    }
    
    private IEnumerator CurrencyInPlayEffect(GameObject prefab, RectTransform spawnArea, RectTransform targetUI)
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject obj = Instantiate(prefab, spawnArea.position, Quaternion.identity, spawnArea.parent);
            RectTransform rect = obj.GetComponent<RectTransform>();
            Image img = obj.GetComponent<Image>();

            // 시작 offset
            Vector3 startPos = spawnArea.position + new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), 0);
            rect.position = startPos;

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
            seq.Append(rect.DOPath(path, _moveTime, PathType.CatmullRom)
                .SetEase(Ease.InOutQuad));

            // 2) 이동하면서 투명도 0으로
            // seq.Join(img.DOFade(0f, _moveTime));
            seq.Join(img.DOFade(0f, _fadeTime));

            // 3) 완료 후 파괴 및 코인 갯수 증가
            seq.OnComplete(() =>
            {
                Destroy(obj);
            });

            yield return new WaitForSeconds(_spawnInterval);
        }
    }
    
    public void CurrencyInPlayStartEffectDown(GameObject prefab, RectTransform spawnArea, RectTransform targetUI)
    {
        StartCoroutine(CurrencyInPlayEffectDown(prefab, spawnArea, targetUI));
    }

    private IEnumerator CurrencyInPlayEffectDown(GameObject prefab, RectTransform spawnArea, RectTransform targetUI)
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject obj = Instantiate(prefab, spawnArea.position, Quaternion.identity, spawnArea.parent);
            RectTransform rect = obj.GetComponent<RectTransform>();
            Image img = obj.GetComponent<Image>();

            // 시작 offset
            Vector3 startPos = spawnArea.position + new Vector3(
                Random.Range(-100f, 100f),
                Random.Range(-100f, 100f),
                0
            );
            rect.position = startPos;

            // 중간 제어 포인트 (아래쪽으로 떨어지는 곡선)
            Vector3 controlPos = startPos + new Vector3(
                Random.Range(-50f, 50f),
                Random.Range(-150f, -250f), // 음수 → 아래쪽
                0
            );

            Vector3[] path = new Vector3[]
            {
                controlPos,
                targetUI.position
            };

            Sequence seq = DOTween.Sequence();

            // 경로 이동
            seq.Append(rect.DOPath(path, _moveTime, PathType.CatmullRom)
                .SetEase(Ease.InOutQuad));

            // 투명도 점점 감소
            seq.Join(img.DOFade(0f, _fadeTime));

            // 완료 후 파괴
            seq.OnComplete(() => Destroy(obj));

            yield return new WaitForSeconds(_spawnInterval);
        }
    }
    
}