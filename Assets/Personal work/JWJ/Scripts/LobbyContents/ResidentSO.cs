using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Resident", menuName = "PangTasticSO/Resident")]
public class ResidentSO : ScriptableObject
{
    public string ResidentName;

    //public Transform FavoriteDestination;
    public bool IsUnlocked;

    public int UnlockStage;
     
    public ResidentSO FavoriteResident;

    public RewardType RewardType;
}
