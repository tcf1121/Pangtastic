using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stage", menuName = "PangTasticSO/Stage")]
public class StageSO : ScriptableObject
{
    [Header("스테이지 ID")]
    public int StageID;

    [Header("손님 등장 수")]
    public int TotalCustomerCount;

    [Header("등장 손님 목록")]
    public CustomerSO[] CustomerList;

    [Header("재료 추가 개수")]
    public IngredientAdjustment[] IngredientAdjustments;

    [System.Serializable]
    public struct IngredientAdjustment
    {
        public IngredientSO Ingredient;
        public int ExtraAmount;
    }
}
