using UnityEngine;

[CreateAssetMenu(fileName = "CookingRecipe", menuName = "CookingSO/CookingRecipe")]
public class SO_CookingRecipe : ScriptableObject
{
    public int RecipeId;
    public string RecipeName;
    public int Material1Id;
    public int Material1Quantity;
    public int Material2Id;
    public int Material2Quantity;
    public int Material3Id;
    public int Material3Quantity;
    public int Material4Id;
    public int Material4Quantity;
}
