using UnityEngine;

[CreateAssetMenu(fileName = "Stage", menuName = "StageSO/StageData")]
public class SO_Stage : ScriptableObject
{
    public int StageID;
    public int PuzzleBoardId;
    public int StageType;
    public int LevelId;
    public int MaxGoldGain;
}
