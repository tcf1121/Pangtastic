using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CustomerType
{
    Normal,
    Unique,
    Special
}

[CreateAssetMenu(fileName = "New Customer", menuName = "PangTasticSO/Customer")]
public class CustomerSO : ScriptableObject
{
    [Header("id")]
    public int ID;

    [Header("�̸�")]
    public string Name;

    [Header("�մ� Ÿ��")]
    public CustomerType Type;

    [Header("�մ� �̹���")]
    public Sprite CustomerPic;

    [Header("�γ���")]
    public int BASE_PATIENCE = 100;

    [Header("�γ��� ���Ҽӵ�")]
    public float DropPerSecond;

    [Header("�����ϴ� ������")]
    public RecipeSO[] FavoriteRecipes;

    [Header("���� ���")]
    public string FirstDialogue;

    [Header("�γ��ɿ� ���� ���")]
    public string HighDialogue;
    public string MidDialogue;
    public string LeftDialogue;
}
