using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Customer", menuName = "PangTasticSO/Customer")]
public class CustomerSO : ScriptableObject
{
    [Header("id")]
    public int ID;

    [Header("이름")]
    public string Name;

    [Header("손님 이미지")]
    public Sprite CustomerPic;

    [Header("인내심")]
    public int BASE_PATIENCE = 100;

    [Header("인내심 감소속도")]
    public float DropPerSecond;

    [Header("좋아하는 레시피")]
    public RecipeSO[] FavoriteRecipes;

    [Header("등장 대사")]
    public string FirstDialogue;

    [Header("인내심에 따른 대사")]
    public string HighDialogue;
    public string MidDialogue;
    public string LeftDialogue;
}
