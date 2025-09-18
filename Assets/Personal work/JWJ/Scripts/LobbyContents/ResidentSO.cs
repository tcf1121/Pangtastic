using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Resident", menuName = "PangTasticSO/Resident")]
public class ResidentSO : ScriptableObject
{
    [Header("이름")]
    public string ResidentName;

    [Header("해금 스테이지")]
    public int UnlockStage;

    [Header("선호장소 목록")]
    public DestinationType[] FavoriteDestinations;

    [Header("터치시 대사")]
    public StringSO[] DialogueTouched;
}
