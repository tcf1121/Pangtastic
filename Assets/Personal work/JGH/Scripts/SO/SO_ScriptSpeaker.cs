using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ScriptSpeaker", menuName = "ScriptSystem/ScriptSpeakerData")]
public class SO_ScriptSpeaker : ScriptableObject
{
    public List<ScriptingSystem.SpeakerData> speakers;
}