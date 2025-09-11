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
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClickChangeLanguage);
    }

    // UI 버튼 OnClick() 이벤트에 연결
    public void OnClickChangeLanguage()
    {
        Manager.Language.SetLanguage(language);
        Manager.Language.ChangeLanguage();
        //SceneManager.LoadScene(2/*로비씬*/);
    }
}
