using UnityEngine;

[CreateAssetMenu(fileName = "LevelVolue", menuName = "LevelSO/LevelVolue")]
public class SO_LevelVolue : ScriptableObject
{
    public int LevelId;
    public int CustomerId;
    public float Reqmat1Multiplier;
    public float Reqmat2Multiplier;
    public float Reqmat3Multiplier;
    public float Reqmat4Multiplier;
}