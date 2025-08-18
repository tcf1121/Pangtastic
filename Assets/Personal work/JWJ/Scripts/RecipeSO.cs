using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "PangTasticSO/Recipe")]
public class RecipeSO : ScriptableObject
{
    [Header("id")]
    public int ID;

    [Header("이름")]
    public string Name;

    [Header("타입")]
    public string Type;

    [Header("이미지")]
    public Sprite FoodPic;

    [System.Serializable]
    public struct IngredientRequirement
    {
        public IngredientSO Ingredient;
        public int Amount;
    }

    [Header("필요 재료 목록")]
    public IngredientRequirement[] Ingredients;
}