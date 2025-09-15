using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Speakers", menuName = "PangTasticSO/Speakers")]
public class SpeakerSO : ScriptableObject
{
    public List<StringSO> stringSO;

    public StringSO GetSpeaker(Speaker speaker)
    {
        return stringSO[(int)speaker];
    }
}