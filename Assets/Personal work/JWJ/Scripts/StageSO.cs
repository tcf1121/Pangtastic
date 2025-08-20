using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stage", menuName = "PangTasticSO/Stage")]
public class StageSO : ScriptableObject
{
    [Header("스테이지 ID")]
    public int StageID;

    [Header("스테이지 클리어 손님 수")]
    public int StageClearCustomerCount;

    [Header("최소 주문 수")]
    [Range(2, 4)] public int MinOrderNum;

    [Header("등장 손님 목록")]
    public CustomerSO[] CustomerList;

    [Header("스테이지 등장 레시피 목록")]
    public RecipeSO[] StageRecipes;

    [Header("재료 추가 개수")]
    public IngredientAdjustment[] IngredientAdjustments;

    [Header("손님 타입별 가중치")]

    [Range(0, 100)] public int WeightNormal = 80;
    [Range(0, 100)] public int WeightUnique = 18;
    [Range(0, 100)] public int WeightSpecial = 2;

    [System.Serializable]
    public struct IngredientAdjustment
    {
        public IngredientSO Ingredient;
        public int ExtraAmount;
    }
}
