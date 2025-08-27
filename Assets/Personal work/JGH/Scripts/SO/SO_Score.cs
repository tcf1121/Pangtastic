using UnityEngine;

[CreateAssetMenu(fileName = "ScoreData", menuName = "ScoreSO/ScoreData")]
public class SO_Score : ScriptableObject
{
    public int ScoreId;
    public int PatienceScoreMultiplier;
    public float ComboTimeout;
    public float ComboScoreMultiplier;
    public int GoldGainRate;
}
