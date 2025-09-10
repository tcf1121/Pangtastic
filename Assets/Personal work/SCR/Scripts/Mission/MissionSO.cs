using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Missions", menuName = "PangTasticSO/Missions")]
public class MissionSO : ScriptableObject
{
    public MissonPlace Place;
    public List<Mission> Mission;
}

[Serializable]
public class Mission
{
    public int MissionID;
    public int Star;
    public string Explane;
    public int Prerequisites;
}
