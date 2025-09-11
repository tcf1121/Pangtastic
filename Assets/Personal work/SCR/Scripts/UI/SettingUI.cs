using TMPro;
using UnityEngine;

public class SettingUI : MonoBehaviour
{
    [SerializeField] TMP_Text _currentLanguge;
    [SerializeField] TMP_Text _userID;

    private void Awake()
    {
        string authJson = Manager.DB.GetAuthInfo();
        DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);
        _userID.text = $"{info.uid}";
    }

    private void OnEnable()
    {
        SetLanguageBtn();
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
}
