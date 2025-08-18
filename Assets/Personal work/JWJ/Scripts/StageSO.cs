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

    [System.Serializable]
    public struct IngredientAdjustment
    {
        public IngredientSO Ingredient;
        public int ExtraAmount;
    }
}
