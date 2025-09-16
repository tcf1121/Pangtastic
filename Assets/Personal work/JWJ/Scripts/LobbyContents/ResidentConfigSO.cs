using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResidentConfig", menuName = "PangTasticSO/ResidentConfig")]
public class ResidentConfigSO : ScriptableObject
{
    [Header("이동 관련")]
    public float MoveSpeed = 1f;
    public float IdleDuration = 2f;
    public float MoveDuration = 20f;
    public float StoppingDistance = 0.6f;
    [Range(0f, 1f)] public float PreferredWeight = 0.4f;

    [Header("상호작용 시간")]
    public float TouchCooldown = 0.5f;
    public float InteractDuration = 10f;
    public float WorkoutDuration = 10f;
    public float GreetDuration = 1.2f;
    public float TalkDuration = 12f;
    public float TouchDuration = 1.2f;

    [Header("주민들끼리 상호작용")]
    public float GreetCooldown = 3f;
    [Range(0f, 1f)] public float TalkChance = 0.2f;
}
