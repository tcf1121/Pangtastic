using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AchivementSO", menuName = "PangTasticSO/AchivementSO")]
public class AchivementSO : ScriptableObject
{
    public List<AchivementInfo> achivements;
}

[Serializable]
public class AchivementInfo
{
    public int ID;
    public string Title;
    public int Target;
    public RewardSO reward;
}
