using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchCombo : MonoBehaviour
{
    [SerializeField] private TMP_Text _comboCount;
    [SerializeField] private Slider _comboSlider;

    public int CurCombo { get; private set; } = 0;
    private float _timer = 0f;

    private void Update()
    {
        if (CurCombo > 0)
        {
            _timer -= Time.deltaTime;
            SetSliderValue();

            if (_timer <= 0f)
            {
                ResetCombo();
            }
        }
    }

    public void UpCombo()
    {
        CurCombo++;
        _timer = 3f;
        _comboCount.text = "Combo\n" + CurCombo;
    }

    private void ResetCombo()
    {
        CurCombo = 0;
        _comboCount.text = "Combo\n" + CurCombo;
    }

    private void SetSliderValue()
    {
        _comboSlider.value = _timer / 3f;
    }
}
