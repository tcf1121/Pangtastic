using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageType
{
    Normal,
    Event,
    Hard
}

[CreateAssetMenu(fileName = "New Stage", menuName = "PangTasticSO/Stage")]
public class StageSO : ScriptableObject
{
    [Header("스테이지 ID")]
    public int StageID;

    [Header("등장 손님")]
    public CustomerSO Customer;

    [Header("주문 수")]
    [Min(1)] public int OrderCount;

    [Header("스테이지 등장 레시피 목록")]
    public RecipeSO[] StageRecipes;

    [Header("스테이지 타입")]
    public StageType Type;

    [Header("퍼즐보드 ID")]
    public int PuzzleBoardID;

    [Header("재료 배수")]
    public IngredientAdjustment[] IngredientAdjustments;

    [System.Serializable]
    public struct IngredientAdjustment
    {
        public IngredientSO Ingredient;
        public float MuliflyBy;
    }
}
