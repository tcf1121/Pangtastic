using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ScriptString", menuName = "ScriptSystem/ScriptSoundData")]
public class SO_ScriptSound : ScriptableObject
{
    public List<ScriptingSystem.SoundData> sounds;
}