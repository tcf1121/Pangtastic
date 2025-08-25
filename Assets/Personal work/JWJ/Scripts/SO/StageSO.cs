using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stage", menuName = "PangTasticSO/Stage")]
public class StageSO : ScriptableObject
{
    [Header("스테이지 ID")]
    public int StageID;

    [Header("주문 수")]
    [Min(1)] public int OrderCount;

    [Header("등장 손님")]
    public CustomerSO Customer;

    [Header("스테이지 등장 레시피 목록")]
    public RecipeSO[] StageRecipes;

    [Header("재료 배수")]
    public IngredientAdjustment[] IngredientAdjustments;

    [System.Serializable]
    public struct IngredientAdjustment
    {
        public IngredientSO Ingredient;
        public float MuliflyBy;
    }
}
