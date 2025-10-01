using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BGMSettingPopup : MonoBehaviour
{
    [SerializeField] TMP_Dropdown _dropdown;
    List<string> _optionList;
    [SerializeField] Button _saveBtn;
    [SerializeField] Button _cancleBtn;
    private int _beforeOption;
    private int _currentOption;
    private string DROPDOWN_KEY = "BGM_Setting";
    private bool _isSave;

    void Awake()
    {
        if (PlayerPrefs.HasKey(DROPDOWN_KEY) == false) _currentOption = 0;
        else _currentOption = PlayerPrefs.GetInt(DROPDOWN_KEY);
        _optionList = new();
        _dropdown.onValueChanged.AddListener(delegate { setDropDown(_dropdown.value); });
        _saveBtn.onClick.AddListener(SaveButton);
        _cancleBtn.onClick.AddListener(CancleButton);
    }

    void OnEnable()
    {
        _isSave = false;
        _beforeOption = _currentOption;
        GetBGM();
    }

    private void GetBGM()
    {
        _optionList.Clear();
        _dropdown.ClearOptions();
        _optionList.Add(Manager.Audio.GetBGMName(0));
        // for (int i = 1; i < (int)Manager.User.GetCurPlace(); i++)
        for (int i = 1; i < (int)Manager.UserInfoSystem.GetCurPlace(); i++)
        {
            _optionList.Add(Manager.Audio.GetBGMName(i));
        }
        _dropdown.AddOptions(_optionList);
        _dropdown.value = _currentOption;
    }

    private void setDropDown(int option)
    {
        PlayerPrefs.SetInt(DROPDOWN_KEY, option);
    }

    private void SaveButton()
    {
        _isSave = true;
        _currentOption = _dropdown.value;
    }

    private void CancleButton()
    {
        if (!_isSave)
            PlayerPrefs.SetInt(DROPDOWN_KEY, _beforeOption);
        else
        {
            Manager.Audio.SetLobbyPlace((MissionPlace)_currentOption);
            Manager.Audio.PlayLobbyBGM();
        }
        gameObject.SetActive(false);
    }

}
