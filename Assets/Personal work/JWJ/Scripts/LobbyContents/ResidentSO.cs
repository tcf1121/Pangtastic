using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Resident", menuName = "PangTasticSO/Resident")]
public class ResidentSO : ScriptableObject
{
    public string ResidentName;

    public bool IsUnlocked;
    public int UnlockStage;

    public DestinationType FavoriteDestination;

    public RewardType RewardType;
    public int RewardAmount;
    public int RewardIntervalSeconds;
}
