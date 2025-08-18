using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "PangTasticSO/Ingredient")]
public class IngredientSO : ScriptableObject
{
    [Header("id")]
    public int ID;

    [Header("�̸�")]
    public string Name;

    [Header("�̹���")]
    public Sprite IngredientPic;
}