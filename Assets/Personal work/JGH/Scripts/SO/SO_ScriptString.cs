using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ScriptString", menuName = "ScriptSystem/ScriptStringData")]
public class SO_ScriptString : ScriptableObject
{
    public List<ScriptingSystem.StringData> strings;
}