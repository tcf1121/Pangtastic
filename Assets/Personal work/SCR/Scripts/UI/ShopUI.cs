using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _content;
    [SerializeField] private RectTransform[] _panels;
    [SerializeField] private Toggle[] toggles;
    [SerializeField] private LayoutGroup _layout;
    [SerializeField] private float _scrollSpeed = 5f;
    private Coroutine _scrollRoutine;
    private bool _isScrollingByButton = false;

    void Awake()
    {
        _layout.enabled = false;
    }

    void Start()
    {
        _panels[2].gameObject.SetActive(false);
        _panels[2].gameObject.SetActive(true);
        _layout.enabled = true;
        _scrollRect.onValueChanged.AddListener((v) => UpdateToggleByScroll());
    }

    public void ScrollToPanelSmooth(int index)
    {
        if (index < 0 || index >= _panels.Length) return;

        float contentHeight = _content.rect.height - _scrollRect.viewport.rect.height;
        float targetY = Mathf.Abs(_panels[index].anchoredPosition.y);
        float normalizedPos = 1f - Mathf.Clamp01(targetY / contentHeight);

        if (_scrollRoutine != null) StopCoroutine(_scrollRoutine);
        _scrollRoutine = StartCoroutine(SmoothScroll(normalizedPos));

        _isScrollingByButton = true;
    }

    private IEnumerator SmoothScroll(float targetPos)
    {
        float start = _scrollRect.verticalNormalizedPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * _scrollSpeed;
            _scrollRect.verticalNormalizedPosition = Mathf.Lerp(start, targetPos, t);
            yield return null;
        }

        _isScrollingByButton = false;
    }

    private void UpdateToggleByScroll()
    {
        if (_isScrollingByButton) return;

        float closestDist = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < _panels.Length; i++)
        {
            float viewportCenterY = _scrollRect.viewport.position.y;
            float panelCenterY = _panels[i].position.y;

            float dist = Mathf.Abs(viewportCenterY - panelCenterY);

            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        // 해당 패널에 해당하는 토글 On
        for (int i = 0; i < toggles.Length; i++)
        {
            toggles[i].isOn = (i == closestIndex);
        }
    }
}
