using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine;
using UnityEngine.Localization;

public class ChangeLang : MonoBehaviour
{
    // 인스펙터에서 Locale Asset 드래그해서 지정
    [SerializeField] private Locale targetLocale;
    [SerializeField] private TMP_FontAsset targetFont;
    [SerializeField] private string excludeTag = "NoFontChange"; // 제외 태그

    // UI 버튼 OnClick() 이벤트에 연결
    public void OnClickChangeLanguage()
    {
        // 언어 변경
        if (targetLocale != null)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
            Debug.Log("언어 변경됨: " + targetLocale.LocaleName);
        }
        
        // 글꼴 전체 변경
        if (targetFont != null)
        {
            TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>(true);
            foreach (var text in allTexts)
            {
                // 제외 태그 달린 건 건너뛰기
                if (text.CompareTag(excludeTag)) continue;
                text.font = targetFont;
            }
        }
    }
}
