using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CustomizeItem", menuName = "PangTasticSO/CustomizeItem")]
public class CustomizeItemSO : ScriptableObject
{
    public string Id;
    public CosmeticCategory Category;
    public Sprite Icon;
    public string DisplayName;

    [Header("Skin/Face 전용")]
    public Material SkinMaterial;

    [Header("프리팹 전용")]
    public GameObject Prefab;
}
