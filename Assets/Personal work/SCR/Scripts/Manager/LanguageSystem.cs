using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class LanguageSystem : Singleton<LanguageSystem>
{
    private LanguageList languageList;
    private readonly string savedLanguage = "SavedLanguage"; // 제외 태그
    private Language currentLang;
    public Action ChangedLanguage;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log($"언어 매니저 준비");
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

    public TMP_FontAsset GetFont()
    {
        return languageList.Infos[(int)currentLang].Font;
    }

    public void ChangeLanguage()
    {
        if (languageList == null) return;
        int language = (int)currentLang;
        LocalizationSettings.SelectedLocale = languageList.Infos[language].Locale;
        Debug.Log("언어 변경됨: " + languageList.Infos[language].Language);

        //글꼴 전체 변경
        TextLanguage[] allTexts = FindObjectsOfType<TextLanguage>(true);
        foreach (var text in allTexts)
        {
            text.ChangeLanguage();
        }
        ChangedLanguage?.Invoke();
    }

    private void SetSystemLanguage()
    {
        int sysNum = PlayerPrefs.GetInt(savedLanguage, (int)Application.systemLanguage);

        SystemLanguage sysLanguage = (SystemLanguage)sysNum;
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
        PlayerPrefs.SetInt(savedLanguage, (int)sysLanguage);
    }

}
