using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageUI : MonoBehaviour
{
    [SerializeField] LanguageList languageList;
    [SerializeField] List<GameObject> languageOn;

    void OnEnable()
    {
        int i = 0;
        foreach (var languageInfo in languageList.Infos)
        {
            if (LocalizationSettings.SelectedLocale == languageInfo.Locale)
                languageOn[i].SetActive(true);
            else
                languageOn[i].SetActive(false);
            i++;
        }

    }
}
