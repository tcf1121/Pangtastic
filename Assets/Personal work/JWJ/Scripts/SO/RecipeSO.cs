using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "PangTasticSO/Recipe")]
public class RecipeSO : ScriptableObject
{
    [Header("id")]
    public int ID;

    [Header("�̸�")]
    public string Name;

    [Header("Ÿ��")]
    public string Type;

    [Header("�̹���")]
    public Sprite FoodPic;

    [System.Serializable]
    public struct IngredientRequirement
    {
        public IngredientSO Ingredient;
        public int Amount;
    }

    [Header("�ʿ� ��� ���")]
    public IngredientRequirement[] Ingredients;
}