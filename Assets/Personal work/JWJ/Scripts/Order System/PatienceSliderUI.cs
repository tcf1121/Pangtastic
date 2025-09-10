using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatienceSliderUI : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private Image _fillImage;

    [SerializeField] private Color _highColor;
    [SerializeField] private Color _midColor;
    [SerializeField] private Color _lowColor;
    [SerializeField] private Color _veryLowColor;

    [SerializeField] private float _blinkStart = 16f;
    [SerializeField] private float _blinkSpeed = 2f;

    private bool _isBlinking;
    private float _blinkTimer;

    public void SetPatience(float current, float max)
    {
        _slider.minValue = 0f;
        _slider.maxValue = max;
        _slider.value = current;

        float percent = (current / max) * 100f;

        if (percent > 70f) //초록 
        {
            _fillImage.color = _highColor;
            _isBlinking = false;
            //Debug.Log($"초록색 {current}%");
        }
        else if (percent > 40f) //노랑
        {
            _fillImage.color = _midColor;
            _isBlinking = false;
            //Debug.Log($"노랑색 {current}%");
        }
        else if (percent > 17f) //주황
        {
            _fillImage.color = _lowColor;
            _isBlinking = false;
            //Debug.Log($"주황색 {current}%");
        }
        else if (percent > _blinkStart) // 빨강
        {
            _fillImage.color = _veryLowColor;
            _isBlinking = true;
            //Debug.Log($"빨강색 {current}%");
        }
        else
        {
            Debug.Log($"슬라이더 체크 {current}%");
        }
    }

    private void Update()
    {
        if (_isBlinking)
        {
            _blinkTimer += Time.deltaTime * _blinkSpeed;
            float alpha = Mathf.Abs(Mathf.Sin(_blinkTimer)); 

            Color color = _fillImage.color;
            color.a = alpha;
            _fillImage.color = color;
        }
    }
}