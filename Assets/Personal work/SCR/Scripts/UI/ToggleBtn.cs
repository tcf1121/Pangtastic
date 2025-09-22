using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleBtn : MonoBehaviour
{

    private Toggle _toggle;
    private string DROPDOWN_KEY = "Setting_";
    [SerializeField] ToggleType _type;
    [SerializeField] GameObject _on;
    [SerializeField] GameObject _off;
    private bool _currentOption;
    void Awake()
    {
        DROPDOWN_KEY = $"{DROPDOWN_KEY}{_type.ToString()}";
        if (PlayerPrefs.HasKey(DROPDOWN_KEY) == false) _currentOption = true;
        else _currentOption = PlayerPrefs.GetInt(DROPDOWN_KEY) == 0 ? false : true;
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(CheckToggle);
        _toggle.isOn = _currentOption;
    }

    void OnEnable()
    {
        if (_type == ToggleType.BGM)
            SetToggleBtn(!Manager.Audio.BgmAudioSource.mute);
        else if (_type == ToggleType.SFX)
            SetToggleBtn(!Manager.Audio.SfxAudioSource.mute);
        else if (_type == ToggleType.Vibration)
            SetToggleBtn(Manager.Audio.OnVibrate);
    }

    private void SetToggleBtn(bool value)
    {
        _on.SetActive(value);
        _off.SetActive(!value);
        if (value) PlayerPrefs.SetInt(DROPDOWN_KEY, 1);
        else PlayerPrefs.SetInt(DROPDOWN_KEY, 0);
    }

    private void CheckToggle(bool value)
    {
        SetToggleBtn(value);
        if (_type == ToggleType.BGM)
            Manager.Audio.SetBGM(value);
        else if (_type == ToggleType.SFX)
            Manager.Audio.SetSFX(value);
        else if (_type == ToggleType.Vibration)
            Manager.Audio.SetVibrate(value);
    }
}

public enum ToggleType
{
    BGM,
    SFX,
    Vibration,
    Alarm
}
