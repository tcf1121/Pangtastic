using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleBtn : MonoBehaviour
{
    private enum ToggleType
    {
        BGM,
        SFX,
        Vibration,
        Alarm
    }
    private Toggle _toggle;
    [SerializeField] ToggleType _type;
    [SerializeField] GameObject _on;
    [SerializeField] GameObject _off;
    void Awake()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(CheckToggle);
    }

    private void CheckToggle(bool value)
    {
        _on.SetActive(value);
        _off.SetActive(!value);
        if (_type == ToggleType.BGM)
            AudioSystem.Instance.SetBGM(value);
        else if (_type == ToggleType.SFX)
            AudioSystem.Instance.SetSFX(value);
    }
}
