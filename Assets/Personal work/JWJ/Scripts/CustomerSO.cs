using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CustomerType
{
    Normal,
    Unique,
    Special
}

[CreateAssetMenu(fileName = "New Customer", menuName = "PangTasticSO/Customer")]
public class CustomerSO : ScriptableObject
{
    [Header("id")]
    public int ID;

    [Header("이름")]
    public string Name;

    [Header("손님 타입")]
    public CustomerType Type;

    [Header("손님 이미지")]
    public Sprite CustomerPic;

    [Header("인내심")]
    public int BASE_PATIENCE = 100;

    [Header("인내심 0에 도달하는 시간")]
    public float TimeToReachZero;

    [Header("좋아하는 레시피")]
    public RecipeSO[] FavoriteRecipes;

    [Header("등장 대사")]
    public string FirstDialogue;

    [Header("인내심에 따른 대사")]
    public string HighDialogue;
    public string MidDialogue;
    public string LeftDialogue;
}
