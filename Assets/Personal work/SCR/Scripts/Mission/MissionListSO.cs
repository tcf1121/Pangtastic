using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New MissionList", menuName = "PangTasticSO/MissionList")]
public class MissionListSO : ScriptableObject
{
    public List<MissionSO> Missions;
}
