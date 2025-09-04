using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeLang : MonoBehaviour
{
    [SerializeField] Language language;
    [SerializeField] LanguageList languageList;
    private Button _button;

    void Awake()
    {
        _button.onClick.AddListener(OnClickChangeLanguage);
    }

    private readonly string excludeTag = "NoFontChange"; // 제외 태그

    // UI 버튼 OnClick() 이벤트에 연결
    public void OnClickChangeLanguage()
    {

        // 언어 변경
        LocalizationSettings.SelectedLocale = languageList.Infos[(int)language].Locale;
        Debug.Log("언어 변경됨: " + languageList.Infos[(int)language].Language);

        // 글꼴 전체 변경
        // TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>(true);
        // foreach (var text in allTexts)
        // {
        //     // 제외 태그 달린 건 건너뛰기
        //     if (text.CompareTag(excludeTag)) continue;
        //     text.font = languageList.Infos[(int)language].Font;
        // }

        SceneManager.LoadScene(2/*로비씬*/);
    }
}
