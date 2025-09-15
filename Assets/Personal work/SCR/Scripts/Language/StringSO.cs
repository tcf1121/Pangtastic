using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New String", menuName = "PangTasticSO/String")]
public class StringSO : ScriptableObject
{
    public string ID;
    public List<string> value;

    public string GetText(Language language)
    {
        return value[(int)language];
    }
}
