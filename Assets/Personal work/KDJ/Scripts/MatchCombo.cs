using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchCombo : MonoBehaviour
{
    [SerializeField] private ComboNum _comboNum;

    public int CurCombo { get; private set; } = 0;
    private float _timer = 0f;

    private void Update()
    {
        if (CurCombo > 0)
        {
            _timer -= Time.deltaTime;

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
        if (CurCombo > 1)
        Debug.Log("콤보: " + CurCombo);
        _comboNum.SetCombo(CurCombo);
    }

    public void ResetTimer()
    {
        _timer = 3f;
    }

    private void ResetCombo()
    {
        CurCombo = 0;
        _comboNum.SetCombo(CurCombo);
    }
}
