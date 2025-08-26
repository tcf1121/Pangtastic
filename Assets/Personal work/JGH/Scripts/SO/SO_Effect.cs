using UnityEngine;

[CreateAssetMenu(fileName = "EffectData", menuName = "EffectSO/EffectData")]
public class SO_Effect : ScriptableObject
{
    public int VfxID;
    public string Name;
    public string FilePath;
    public string description;
}
