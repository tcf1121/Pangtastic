using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class LanguageSystem : Singleton<LanguageSystem>
{
    private LanguageList languageList;
    private readonly string excludeTag = "NoFontChange"; // 제외 태그
    private Language currentLang;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log($"스테이지 매니저 준비");
        AsyncOperationHandle<LanguageList> handle = Addressables.LoadAssetAsync<LanguageList>("Language");
        handle.Completed += OnLanguageListLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnLanguageListLoaded(AsyncOperationHandle<LanguageList> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            languageList = handle.Result;
            Debug.Log($"languageList 로드 완료.");
            SetSystemLanguage();
            ChangeLanguage();
        }
        else
        {
            Debug.LogError($"languageList 로드 실패:{handle.OperationException}");
        }
    }

    public void SetLanguage(Language language)
    {
        currentLang = language;
    }

    public Language GetLanguage()
    {
        return currentLang;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ChangeLanguage();
    }

    public void ChangeLanguage()
    {
        if (languageList == null) return;
        int language = (int)currentLang;
        LocalizationSettings.SelectedLocale = languageList.Infos[language].Locale;
        Debug.Log("언어 변경됨: " + languageList.Infos[language].Language);

        //글꼴 전체 변경
        TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>(true);
        foreach (var text in allTexts)
        {
            // 제외 태그 달린 건 건너뛰기
            if (text.CompareTag(excludeTag)) continue;
            text.font = languageList.Infos[language].Font;
        }
    }

    private void SetSystemLanguage()
    {
        SystemLanguage sysLanguage = Application.systemLanguage;
        if (sysLanguage == SystemLanguage.Korean) SetLanguage(Language.Korean);
        else if (sysLanguage == SystemLanguage.ChineseSimplified) SetLanguage(Language.Chinese);
        else if (sysLanguage == SystemLanguage.French) SetLanguage(Language.French);
        else if (sysLanguage == SystemLanguage.German) SetLanguage(Language.German);
        else if (sysLanguage == SystemLanguage.Italian) SetLanguage(Language.Italian);
        else if (sysLanguage == SystemLanguage.Japanese) SetLanguage(Language.Japanese);
        else if (sysLanguage == SystemLanguage.Dutch) SetLanguage(Language.Nederlands);
        else if (sysLanguage == SystemLanguage.Portuguese) SetLanguage(Language.Portuguese);
        else if (sysLanguage == SystemLanguage.Russian) SetLanguage(Language.Russian);
        else if (sysLanguage == SystemLanguage.Spanish) SetLanguage(Language.Spanish);
        else if (sysLanguage == SystemLanguage.ChineseTraditional) SetLanguage(Language.Taiwanese);
        else if (sysLanguage == SystemLanguage.Turkish) SetLanguage(Language.Turkish);
        else SetLanguage(Language.English);
    }
}
