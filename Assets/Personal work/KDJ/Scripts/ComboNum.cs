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
    [SerializeField] private AnimationCurve _scaleCurve;
    [SerializeField] private float _scaleDuration = 0.2f;
    private List<Image> _images = new List<Image>();
    private List<RectTransform> _rectTransforms = new List<RectTransform>();
    private Coroutine _scaleCoroutine;

    private void Awake()
    {
        foreach (var prefab in _comboPrefabs)
        {
            _images.Add(prefab.GetComponent<Image>());
            _rectTransforms.Add(prefab.GetComponent<RectTransform>());
        }

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

        _scaleCoroutine = StartCoroutine(ComboScaleCoroutine());
    }

    public void SetActiveDigit(int combo)
    {
        if (combo == 2 || combo == 3 || combo == 7 || combo % 5 == 0)
        {
            if (combo / 1000 > 0)
            {
                // 네자리 전부 켜기
                for (int i = 0; i < _comboPrefabs.Count; i++)
                {
                    _comboPrefabs[i].SetActive(true);
                }
            }
            else if (combo / 100 > 0)
            {
                // 세자리 켜기
                for (int i = 0; i < _comboPrefabs.Count - 1; i++)
                {
                    _comboPrefabs[i].SetActive(true);
                }
                _comboPrefabs[_comboPrefabs.Count - 1].SetActive(false);
            }
            else if (combo / 10 > 0)
            {
                // 두자리 켜기
                for (int i = 0; i < _comboPrefabs.Count - 2; i++)
                {
                    _comboPrefabs[i].SetActive(true);
                }
                for (int i = _comboPrefabs.Count - 2; i < _comboPrefabs.Count; i++)
                {
                    _comboPrefabs[i].SetActive(false);
                }
            }
            else if (combo % 10 > 0)
            {
                // 한자리 켜기
                _comboPrefabs[0].SetActive(true);
                for (int i = 1; i < _comboPrefabs.Count; i++)
                {
                    _comboPrefabs[i].SetActive(false);
                }
            }
        }
        else
        {
            // 모두 끄기
            for (int i = 0; i < _comboPrefabs.Count; i++)
            {
                _comboPrefabs[i].SetActive(false);
            }
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
        while (timer < _scaleDuration)
        {
            float scale = _scaleCurve.Evaluate(timer / _scaleDuration);
            _comboXPrefab.transform.localScale = new Vector3(scale, scale, 1f);
            timer += Time.deltaTime;
            yield return null;
        }
        _comboXPrefab.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(3f - _scaleDuration);
        _comboXPrefab.transform.localScale = Vector3.zero;
        _scaleCoroutine = null;
    }
}
