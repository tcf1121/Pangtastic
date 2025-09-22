using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingUI : MonoBehaviour
{
    [SerializeField] TMP_Text _currentLanguge;
    [SerializeField] TMP_Text _userID;
    [SerializeField] List<GameObject> _testCode;
    [SerializeField] TMP_InputField _testCodeInputField;

    private void Awake()
    {
        _userID.text = $"{Manager.DB.GetUid()}";
        _testCodeInputField.onEndEdit.AddListener(OnInputEnd);
    }

    private void OnEnable()
    {
        SetLanguageBtn();
        foreach (var obj in _testCode)
            obj.SetActive(false);
        _testCode[0].SetActive(true);
    }

    private void SetLanguageBtn()
    {
        Language language = Manager.Language.GetLanguage();

        switch (language)
        {
            case Language.English:
                _currentLanguge.text = $"English";
                break;
            case Language.Russian:
                _currentLanguge.text = $"Русский";
                break;
            case Language.German:
                _currentLanguge.text = $"Deutsch";
                break;
            case Language.Chinese:
                _currentLanguge.text = $"中文";
                break;
            case Language.Taiwanese:
                _currentLanguge.text = $"華語中文";
                break;
            case Language.Japanese:
                _currentLanguge.text = $"日本語";
                break;
            case Language.Korean:
                _currentLanguge.text = $"한국어";
                break;
            case Language.Italian:
                _currentLanguge.text = $"Italiano";
                break;
            case Language.French:
                _currentLanguge.text = $"Français";
                break;
            case Language.Spanish:
                _currentLanguge.text = $"Español";
                break;
            case Language.Nederlands:
                _currentLanguge.text = $"Nederlands";
                break;
            case Language.Turkish:
                _currentLanguge.text = $"Türkçe";
                break;
            case Language.Portuguese:
                _currentLanguge.text = $"Português";
                break;

        }
    }

    private void OnInputEnd(string value)
    {
        if (value == "ruddlf0922") // 특정 값 체크
        {
            Debug.Log("특정 값을 입력했습니다!");
            Manager.Data.OnTest();
            SceneManager.LoadScene(0);
        }
    }
}
