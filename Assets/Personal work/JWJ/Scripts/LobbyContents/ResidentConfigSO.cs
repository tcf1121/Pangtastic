using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResidentConfig", menuName = "PangTasticSO/ResidentConfig")]
public class ResidentConfigSO : ScriptableObject
{
    [Header("이동 속도")]
    public float MoveSpeed = 1f;
    [Header("대기상태 시간")]
    public float IdleDuration = 3f;
    [Header("이동 최대 시간")]
    public float MoveDuration = 20f;
    [Header("목표지점에서 멈춤 거리")]
    public float StoppingDistance = 0.6f;
    [Header("선호 지점 갈 확률")]
    [Range(0f, 1f)] public float PreferredWeight = 0.5f;
    [Header("선호 지점 애니메이션 확률")]
    [Range(0f, 1f)] public float PreferredAnimation = 0.3f;

    [Header("상호작용 상태 시간")]
    public float InteractDuration = 5f;
    [Header("운동 상태 시간")]
    public float WorkoutDuration = 7f;
    [Header("이야기 상태 시간")]
    public float TalkDuration = 5f;
    [Header("터치 상태 시간")]
    public float TouchDuration = 3f;

    [Header("터치 쿨다운")]
    public float TouchCooldown = 1f;

    [Header("주민들끼리 대화 쿨다운")]
    public float TalkCooldown = 3f;

    [Header("대화 확률")]
    [Range(0f, 1f)] public float TalkChance = 0.4f;

    [Header("선호 지역 특별 애니메이션 확률")]
    [Range(0f, 1f)] public float SpecialAnimation = 0.3f;
}
