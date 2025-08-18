using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stage", menuName = "PangTasticSO/Stage")]
public class StageSO : ScriptableObject
{
    [Header("�������� ID")]
    public int StageID;

    [Header("�մ� ���� ��")]
    public int TotalCustomerCount;

    [Header("���� �մ� ���")]
    public CustomerSO[] CustomerList;

    [Header("��� �߰� ����")]
    public IngredientAdjustment[] IngredientAdjustments;

    [Header("�մ� Ÿ�Ժ� ����ġ")]

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
