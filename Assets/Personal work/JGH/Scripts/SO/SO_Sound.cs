using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "SoundSO/SoundData")]
public class SO_Sound : ScriptableObject
{
    public int SoundId;
    public string Name;
    public string FilePath;
    public string Description;
}
