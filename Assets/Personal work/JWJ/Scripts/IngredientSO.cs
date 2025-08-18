using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "PangTasticSO/Ingredient")]
public class IngredientSO : ScriptableObject
{
    [Header("id")]
    public int ID;

    [Header("이름")]
    public string Name;

    [Header("이미지")]
    public Sprite IngredientPic;
}