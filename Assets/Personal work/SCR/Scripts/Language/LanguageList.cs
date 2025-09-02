using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "LanguageList", menuName = "Language/LanguageList")]
public class LanguageList : ScriptableObject
{
    public List<LanguageInfo> Infos;
}

[Serializable]
public class LanguageInfo
{
    public string Language;
    public Locale Locale;
    public TMP_FontAsset Font;
}

public enum Language
{
    English,
    Russian,
    German,
    Chinese,
    Taiwanese,
    Japanese,
    Korean,
    Italian,
    French,
    Spanish,
    Nederlands,
    Turkish,
    Portuguese
}
