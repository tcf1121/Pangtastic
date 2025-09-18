using KDJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboNum : MonoBehaviour
{
    // 0~9까지는 레드. 10~11은 오렌지 2,3
    [SerializeField] private List<Sprite> _comboSprites;
    [SerializeField] private List<GameObject> _comboPrefabs;
    [SerializeField] private GameObject _comboXPrefab;
    [SerializeField] private GameObject _redComboPrefab;
    [SerializeField] private GameObject _redBangPrefab;
    [SerializeField] private GameObject _orangeComboPrefab;
    [SerializeField] private GameObject _orangeBangPrefab;
    [SerializeField] private GameObject _comboEffectPrefab;
    [SerializeField] private AnimationCurve _scaleCurve;
    [SerializeField] private float _scaleDuration = 0.2f;
    private List<Image> _images = new List<Image>();
    private List<RectTransform> _rectTransforms = new List<RectTransform>();
    private RectTransform _redBangRect;
    private RectTransform _orangeBangRect;
    private Coroutine _scaleCoroutine;
    private Vector2 _bangResetAnchoredPos;

    private void Awake()
    {
        foreach (var prefab in _comboPrefabs)
        {
            _images.Add(prefab.GetComponent<Image>());
            _rectTransforms.Add(prefab.GetComponent<RectTransform>());
        }
        _redBangRect = _redBangPrefab.GetComponent<RectTransform>();
        _orangeBangRect = _orangeBangPrefab.GetComponent<RectTransform>();

        _bangResetAnchoredPos = _redBangRect.anchoredPosition;
        _comboXPrefab.SetActive(true);
        _comboXPrefab.transform.localScale = Vector3.zero;
    }

    public void SetCombo(int combo)
    {
        if (combo < 0) return;

        SetActiveDigit(combo);
        SkipCombo(combo);

        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
            _comboXPrefab.transform.localScale = Vector3.zero;
        }

        if (combo != 0 && (combo == 2 || combo == 3 || combo == 7 || combo % 5 == 0))
            _scaleCoroutine = StartCoroutine(ComboScaleCoroutine());
    }

    public void SetActiveDigit(int combo)
    {
        if (combo != 0 && (combo == 2 || combo == 3 || combo == 7 || combo % 5 == 0))
        {
            _comboEffectPrefab.SetActive(true);
            _comboEffectPrefab.GetComponent<ParticleSystem>().Play();

            string comboStr = combo.ToString();
            int digitCount = comboStr.Length;

            // 콤보 길이에 따라 숫자 활성화/비활성화
            for (int i = 0; i < _rectTransforms.Count; i++)
            {
                _rectTransforms[i].gameObject.SetActive(i < digitCount);
            }

            // ! 위치 설정
            Vector2 bangPosition;
            if (digitCount > 0 && digitCount < _rectTransforms.Count)
            {
                // !를 다음 숫자 위치에 배치 (예: 2자리 콤보일 경우 3번째 자리에 배치)
                bangPosition = _rectTransforms[digitCount].anchoredPosition;
            }
            else
            {
                // 0 또는 4자리 이상일 경우 중앙으로 초기화
                bangPosition = _bangResetAnchoredPos;
            }
            _redBangRect.anchoredPosition = bangPosition;
            _orangeBangRect.anchoredPosition = bangPosition;
        }
        else
        {
            // 콤보가 표시되지 않는 경우, 모든 것을 끔
            _comboEffectPrefab.SetActive(false);

            for (int i = 0; i < _rectTransforms.Count; i++)
            {
                _rectTransforms[i].gameObject.SetActive(false);
            }
            _redBangRect.anchoredPosition = _bangResetAnchoredPos;
            _orangeBangRect.anchoredPosition = _bangResetAnchoredPos;
        }
    }

    public void SkipCombo(int combo)
    {
        // 콤보는 2, 3, 5, 7, 10으로 출력
        // 10 이후는 5마다
        // 2, 3일때는 오렌지 콤보 사용
        // 5부터 레드 콤보 사용

        if (combo == 0)
        {
            _orangeComboPrefab.SetActive(false);
            _orangeBangPrefab.SetActive(false);
            _redComboPrefab.SetActive(false);
            _redBangPrefab.SetActive(false);
            return;
        }
        else if (combo == 2 || combo == 3)
        {
            _orangeComboPrefab.SetActive(true);
            _orangeBangPrefab.SetActive(true);
            _redComboPrefab.SetActive(false);
            _redBangPrefab.SetActive(false);
            switch (combo)
            {
                case 2:
                    _images[0].sprite = _comboSprites[10];
                    break;
                case 3:
                    _images[0].sprite = _comboSprites[11];
                    break;
            }
        }
        else if (combo == 7 || combo % 5 == 0)
        {
            // 이후는 전부 여기
            _orangeComboPrefab.SetActive(false);
            _orangeBangPrefab.SetActive(false);
            _redComboPrefab.SetActive(true);
            _redBangPrefab.SetActive(true);
            SetComboNum();
        }
        else
        {
            _comboEffectPrefab.SetActive(false);
            _orangeComboPrefab.SetActive(false);
            _orangeBangPrefab.SetActive(false);
            _redComboPrefab.SetActive(false);
            _redBangPrefab.SetActive(false);
            return;
        }
    }

    public void SetComboNum()
    {
        if (BoardManager.Instance.MatchCombo.CurCombo / 1000 > 0)
        {
            // 네자리 설정
            _images[0].sprite = _comboSprites[BoardManager.Instance.MatchCombo.CurCombo / 1000];
            _images[1].sprite = _comboSprites[(BoardManager.Instance.MatchCombo.CurCombo % 1000) / 100];
            _images[2].sprite = _comboSprites[(BoardManager.Instance.MatchCombo.CurCombo % 100) / 10];
            _images[3].sprite = _comboSprites[BoardManager.Instance.MatchCombo.CurCombo % 10];
        }
        else if (BoardManager.Instance.MatchCombo.CurCombo / 100 > 0)
        {
            // 세자리 설정
            _images[0].sprite = _comboSprites[(BoardManager.Instance.MatchCombo.CurCombo % 1000) / 100];
            _images[1].sprite = _comboSprites[(BoardManager.Instance.MatchCombo.CurCombo % 100) / 10];
            _images[2].sprite = _comboSprites[BoardManager.Instance.MatchCombo.CurCombo % 10];
        }
        else if (BoardManager.Instance.MatchCombo.CurCombo / 10 > 0)
        {
            // 두자리 설정
            _images[0].sprite = _comboSprites[(BoardManager.Instance.MatchCombo.CurCombo % 100) / 10];
            _images[1].sprite = _comboSprites[BoardManager.Instance.MatchCombo.CurCombo % 10];
        }
        else if (BoardManager.Instance.MatchCombo.CurCombo % 10 > 0)
        {
            // 한자리 설정
            _images[0].sprite = _comboSprites[BoardManager.Instance.MatchCombo.CurCombo % 10];
        }

        for (int i = 0; i < _images.Count; i++)
        {
            if (_images[i].sprite == _comboSprites[1])
            {
                _rectTransforms[i].sizeDelta = new Vector2(50, 100);
            }
            else
            {
                _rectTransforms[i].sizeDelta = new Vector2(65, 100);
            }
        }
    }

    private IEnumerator ComboScaleCoroutine()
    {
        float timer = 0f;
        _comboEffectPrefab.transform.position = new Vector3(_comboEffectPrefab.transform.position.x, Camera.main.ScreenToWorldPoint(_orangeComboPrefab.transform.position).y, 0);
        Vector3 effectScale = _comboEffectPrefab.transform.localScale;
        while (timer < _scaleDuration)
        {
            float scale = _scaleCurve.Evaluate(timer / _scaleDuration);
            _comboXPrefab.transform.localScale = new Vector3(scale, scale, 1f);
            _comboEffectPrefab.transform.localScale = effectScale * scale;
            timer += Time.deltaTime;
            yield return null;
        }
        _comboXPrefab.transform.localScale = Vector3.one;
        _comboEffectPrefab.transform.localScale = effectScale;

        yield return new WaitForSeconds(3f - _scaleDuration);
        _comboEffectPrefab.SetActive(false);
        _comboXPrefab.transform.localScale = Vector3.zero;
        _scaleCoroutine = null;
    }
}
